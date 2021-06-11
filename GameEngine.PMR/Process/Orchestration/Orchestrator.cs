using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Process.Orchestration.States;
using GameEngine.PMR.Process.Structure;
using System;
using System.Collections.Generic;

namespace GameEngine.PMR.Process.Orchestration
{
    /// <summary>
    /// A subprocess called Orchestrator that manages the evolutions and transitions of a GameModule as well as its connections with other modules
    /// </summary>
    internal class Orchestrator
    {
        internal const string TAG = GameProcess.TAG;

        internal string Category { get; private set; }

        internal OrchestratorState State => m_StateMachine.CurrentStateId;

        internal bool IsOperational => State == OrchestratorState.Operational;

        internal GameProcess MainProcess;
        internal Orchestrator Parent;
        internal List<Orchestrator> Children;
        internal GameModule CurrentModule;
        internal TransitionActivity CurrentTransition;

        internal Action AwaitingAction;
        internal Action OnReset;
        internal Action OnOperational;
        internal Action OnTerminated;

        private FSM<OrchestratorState> m_StateMachine;
        private delegate void OperationDelegate(Action onFinish);

        internal Orchestrator(string category, GameProcess process, Orchestrator parent)
        {
            Category = category;
            MainProcess = process;
            Parent = parent;
            Children = new List<Orchestrator>();

            m_StateMachine = new FSM<OrchestratorState>($"{category}OrchestratorFSM",
                new List<FSMState<OrchestratorState>>()
                {
                    new WaitState(),
                    new EnterTransitionState(this),
                    new RunTransitionState(this),
                    new ExitTransitionState(this),
                    new OperationalState(this),
                    new ResetState(this)
                },
                OrchestratorState.Wait);
            m_StateMachine.Start();
        }

        ~Orchestrator()
        {
            m_StateMachine.Stop();
        }

        internal void Update()
        {
            m_StateMachine.Update();
        }

        internal void FixedUpdate()
        {
            m_StateMachine.FixedUpdate();
        }

        internal void LateUpdate()
        {
            m_StateMachine.LateUpdate();
        }

        internal void LoadModule(IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, Parent?.CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup.GetType());

            StartOrPlanModuleOperation((onFinish) =>
            {
                CurrentModule = new GameModule(setup, configuration, this);
                CurrentModule.InnerLoad();
                CurrentModule.OnFinishLoading += onFinish;
            },
            setup.GetTransitionActivity());
        }

        internal void UnloadModule()
        {
            StartOrPlanModuleOperation((onFinish) =>
            {
                CurrentModule.InnerUnload();
                CurrentModule.OnFinishUnloading += () =>
                {
                    CurrentModule.Destroy();
                    CurrentModule = null;
                    onFinish();
                };
            },
            CurrentTransition);
        }

        internal void ReloadModule()
        {
            StartOrPlanModuleOperation((onFinish) =>
            {
                CurrentModule.InnerReload();
                CurrentModule.OnFinishLoading += onFinish;
            },
            CurrentTransition);
        }

        internal void SwitchToModule(IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, Parent?.CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup.GetType());

            StartOrPlanModuleOperation((onFinish) =>
            {
                CurrentModule.InnerUnload();
                CurrentModule.OnFinishUnloading += () =>
                {
                    CurrentModule.Destroy();
                    CurrentModule = new GameModule(setup, configuration, this);
                    CurrentModule.InnerLoad();
                    CurrentModule.OnFinishLoading += onFinish;
                };
            },
            setup.GetTransitionActivity());
        }

        internal void AddSubmodule(string subcategory, IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup.GetType());

            if (State == OrchestratorState.Reset)
                throw new InvalidOperationException($"Cannot add submodule during reset phase (all submodules are being unloaded)");

            if (CurrentModule.State == GameModuleState.UnloadRules || CurrentModule.State == GameModuleState.End)
                throw new InvalidOperationException($"Cannot add submodule when current module is unloading. Module state: {CurrentModule.State}");

            Orchestrator childOrchestrator = new Orchestrator(subcategory, MainProcess, this);
            Children.Add(childOrchestrator);

            if (CurrentModule.State == GameModuleState.UpdateRules)
                childOrchestrator.LoadModule(setup, configuration);
            else
                CurrentModule.OnFinishLoading += () => childOrchestrator.LoadModule(setup, configuration);
        }

        internal void RemoveSubmodule(string subcategory)
        {
            Orchestrator childOrchestrator = Children.Find((orchestrator) => orchestrator.Category == subcategory);

            if (childOrchestrator == null)
                throw new InvalidOperationException($"Invalid submodule category {subcategory}. Cannot be found as child of module {CurrentModule.Name}");

            if (childOrchestrator.CurrentModule != null)
                childOrchestrator.UnloadModule();
        }

        internal GameModule GetSubmodule(string subcategory)
        {
            Orchestrator childOrchestrator = Children.Find((orchestrator) => orchestrator.Category == subcategory);

            if (childOrchestrator == null)
                throw new InvalidOperationException($"Invalid submodule category {subcategory}. Cannot be found as child of module {CurrentModule.Name}");

            return childOrchestrator.CurrentModule;
        }

        internal void OnQuit()
        {
            foreach (Orchestrator childOrchestrator in Children)
            {
                childOrchestrator.OnQuit();
            }
            CurrentModule?.InnerQuit();
            CurrentTransition?.BaseCleanup();
        }

        #region private
        private bool CheckModuleValidity(IGameModuleSetup moduleSetup, GameModule parent)
        {
            if (moduleSetup is IGameSubmoduleSetup submoduleSetup)
            {
                if (submoduleSetup.RequiredServiceSetup != null && submoduleSetup.RequiredServiceSetup != MainProcess.Services.Id)
                {
                    Log.Error(TAG, $"Invalid submodule {submoduleSetup.Name}. Current services: {MainProcess.Services.Id}. Expected services: {submoduleSetup.RequiredServiceSetup}");
                    return false;
                }

                if (submoduleSetup.RequiredParentSetup != null && submoduleSetup.RequiredParentSetup != parent?.Id)
                {
                    Log.Error(TAG, $"Invalid submodule {submoduleSetup.Name}. Current parent module: {parent?.Id}. Expected parent module: {submoduleSetup.RequiredParentSetup}");
                    return false;
                }
            }
            else if (moduleSetup is IGameModeSetup modeSetup)
            {
                if (modeSetup.RequiredServiceSetup != null && modeSetup.RequiredServiceSetup != MainProcess.Services.Id)
                {
                    Log.Error(TAG, $"Invalid game mode {modeSetup.Name}. Current services: {MainProcess.Services.Id}. Expected services: {modeSetup.RequiredServiceSetup}");
                    return false;
                }
            }
            else if (moduleSetup is IGameServiceSetup serviceSetup)
            {
                if (!serviceSetup.CheckAppRequirements())
                {
                    Log.Error(TAG, $"Invalid services {serviceSetup.Name}. Application requirements are not met for these services");
                    return false;
                }
            }

            return true;
        }

        private void StartOrPlanModuleOperation(OperationDelegate moduleOperation, TransitionActivity transition)
        {
            void performOperationWithTransition(bool skipEnter = false)
            {
                if (CurrentModule == MainProcess.Services && MainProcess.CurrentGameMode != null)
                    MainProcess.ResetGameMode();

                if (transition != CurrentTransition)
                {
                    CurrentTransition?.BaseCleanup();
                    CurrentTransition = transition;
                    CurrentTransition?.BaseInitialize(MainProcess.Time);
                }

                if (!skipEnter)
                    m_StateMachine.SetState(OrchestratorState.EnterTransition, priority: 100);
                else
                    m_StateMachine.SetState(State, priority: 100);
                moduleOperation(onFinish: () => m_StateMachine.SetState(OrchestratorState.ExitTransition));
            }

            if (State == OrchestratorState.EnterTransition || State == OrchestratorState.RunTransition || State == OrchestratorState.ExitTransition)
            {
                if (CurrentModule != null)
                {
                    CurrentModule.OnFinishLoading = null;
                    CurrentModule.OnFinishUnloading = null;
                }

                if (transition == CurrentTransition)
                {
                    performOperationWithTransition(State != OrchestratorState.ExitTransition);
                }
                else
                {
                    m_StateMachine.SetState(OrchestratorState.ExitTransition, priority: 100);
                    AwaitingAction = () => performOperationWithTransition();
                }
            }
            else
            {
                if (Children.Count > 0)
                {
                    m_StateMachine.SetState(OrchestratorState.Reset, priority: 100);
                    OnReset = () => performOperationWithTransition();
                }
                else
                {
                    performOperationWithTransition();
                }
            }
        }
        #endregion
    }
}
