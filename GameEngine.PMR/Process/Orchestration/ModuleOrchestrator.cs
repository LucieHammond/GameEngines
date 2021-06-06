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
    /// A subprocess called ModuleOrchestrator that manages the evolutions and transitions of a GameModule as well as its connections with other modules
    /// </summary>
    internal class ModuleOrchestrator
    {
        internal const string TAG = GameProcess.TAG;

        internal ModuleOrchestratorState State => m_StateMachine.CurrentStateId;

        internal bool IsOperational => State == ModuleOrchestratorState.OperateModule;

        internal GameProcess MainProcess;
        internal ModuleOrchestrator ParentModule;
        internal List<ModuleOrchestrator> SubModules;
        internal GameModule CurrentModule;
        internal TransitionActivity CurrentTransition;

        internal Action AwaitingAction;
        internal Action OnReset;
        internal Action OnOperational;
        internal Action OnTerminated;

        private FSM<ModuleOrchestratorState> m_StateMachine;
        private delegate void TransformActionDelegate(Action OnFinish);

        internal ModuleOrchestrator(GameProcess process, ModuleOrchestrator parent)
        {
            MainProcess = process;
            ParentModule = parent;
            SubModules = new List<ModuleOrchestrator>();

            m_StateMachine = new FSM<ModuleOrchestratorState>($"{CurrentModule.Name}OrchestratorFSM",
                new List<FSMState<ModuleOrchestratorState>>()
                {
                    new WaitState(),
                    new EnterTransitionState(this),
                    new RunTransitionState(this),
                    new ExitTransitionState(this),
                    new OperateModuleState(this),
                    new ResetSubmodulesState(this)
                },
                ModuleOrchestratorState.Wait);
            m_StateMachine.Start();
        }

        internal void Update()
        {
            m_StateMachine.Update();
        }

        internal void LoadModule(IGameModuleSetup setup, Configuration configuration)
        {
            if (!CheckModuleValidity(setup, ParentModule?.CurrentModule))
                return;

            StartOrPlanTransformation((onFinish) =>
            {
                CurrentModule = new GameModule(setup, configuration, this);
                CurrentModule.InnerLoad();
                CurrentModule.OnFinishLoading += onFinish;
            },
            setup.GetTransitionActivity());
        }

        internal void UnloadModule()
        {
            StartOrPlanTransformation((onFinish) =>
            {
                CurrentModule.InnerUnload();
                CurrentModule.OnFinishUnloading += () =>
                {
                    CurrentModule.InnerStop();
                    CurrentModule = null;
                    onFinish();
                };
            },
            CurrentTransition);
        }

        internal void ReloadModule()
        {
            StartOrPlanTransformation((onFinish) =>
            {
                CurrentModule.InnerReload();
                CurrentModule.OnFinishLoading += onFinish;
            },
            CurrentTransition);
        }

        internal void SwitchToModule(IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, ParentModule?.CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup);

            StartOrPlanTransformation((onFinish) =>
            {
                CurrentModule.InnerUnload();
                CurrentModule.OnFinishUnloading += () =>
                {
                    CurrentModule.InnerStop();
                    CurrentModule = new GameModule(setup, configuration, this);
                    CurrentModule.InnerLoad();
                    CurrentModule.OnFinishLoading += onFinish;
                };
            },
            setup.GetTransitionActivity());
        }

        internal void AddSubmodule(IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup);

            if (State == ModuleOrchestratorState.ResetSubmodules)
                throw new InvalidOperationException($"Cannot add submodule during reset phase (all submodules are being unloaded)");

            if (CurrentModule.State == GameModuleState.UnloadRules || CurrentModule.State == GameModuleState.End)
                throw new InvalidOperationException($"Cannot add submodule when current module is unloading. Module state: {CurrentModule.State}");

            ModuleOrchestrator submodule = new ModuleOrchestrator(MainProcess, this);
            SubModules.Add(submodule);

            if (CurrentModule.State == GameModuleState.UpdateRules)
                submodule.LoadModule(setup, configuration);
            else
                CurrentModule.OnFinishLoading += () => submodule.LoadModule(setup, configuration);
        }

        internal void RemoveSubmodule(GameModule submodule)
        {
            if (!SubModules.Contains(submodule.Orchestrator))
                throw new InvalidOperationException($"Invalid submodule {submodule.Name}. Cannot be found in the reference list");

            submodule.Orchestrator.UnloadModule();
        }

        internal void Stop()
        {
            m_StateMachine.Stop();
        }

        internal void OnQuit()
        {
            foreach (ModuleOrchestrator submodule in SubModules)
            {
                submodule.OnQuit();
            }
            CurrentModule?.InnerQuit();
            m_StateMachine.Stop();
        }

        internal void GoToState(ModuleOrchestratorState state)
        {
            m_StateMachine.SetState(state);
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

        private void StartOrPlanTransformation(TransformActionDelegate transformActon, TransitionActivity transition)
        {
            void transformWithTransition(bool skipEnter = false)
            {
                if (transition != CurrentTransition)
                {
                    CurrentTransition?.BaseCleanup();
                    CurrentTransition = transition;
                    CurrentTransition?.BaseInitialize(MainProcess.Time);
                }

                if (!skipEnter)
                    m_StateMachine.SetState(ModuleOrchestratorState.EnterTransition);
                transformActon(() => m_StateMachine.SetState(ModuleOrchestratorState.ExitTransition));
            }

            if (State == ModuleOrchestratorState.EnterTransition || State == ModuleOrchestratorState.RunTransition || State == ModuleOrchestratorState.ExitTransition)
            {
                CurrentModule.OnFinishLoading = null;
                CurrentModule.OnFinishUnloading = null;

                if (transition == CurrentTransition)
                {
                    transformWithTransition(State != ModuleOrchestratorState.ExitTransition);
                }
                else
                {
                    m_StateMachine.SetState(ModuleOrchestratorState.ExitTransition);
                    AwaitingAction = () => transformWithTransition();
                }
            }
            else
            {
                if (SubModules.Count > 0)
                {
                    m_StateMachine.SetState(ModuleOrchestratorState.ResetSubmodules);
                    OnReset = () => transformWithTransition();
                }
                else
                {
                    transformWithTransition();
                }
            }
        }
        #endregion
    }
}
