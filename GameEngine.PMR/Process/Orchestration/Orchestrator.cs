using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Process.Orchestration.States;
using GameEngine.PMR.Process.Transitions;
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
        internal Transition CurrentTransition;

        internal Queue<Action> NextOperations;
        internal Queue<Transition> PastTransitions;
        internal Action OnOperational;
        internal Action OnTerminated;

        private FSM<OrchestratorState> m_StateMachine;
        private Action m_Requirements;

        internal Orchestrator(string category, GameProcess process, Orchestrator parent)
        {
            Category = category;
            MainProcess = process;
            Parent = parent;
            Children = new List<Orchestrator>();
            NextOperations = new Queue<Action>();
            PastTransitions = new Queue<Transition>();

            m_StateMachine = new FSM<OrchestratorState>($"{category}OrchestratorFSM",
                new List<FSMState<OrchestratorState>>()
                {
                    new WaitState(this),
                    new EnterTransitionState(this),
                    new RunTransitionState(this),
                    new ExitTransitionState(this),
                    new OperationalState(this),
                    new ChangeTransitionState(this)
                },
                OrchestratorState.Wait);
            m_StateMachine.Start();
        }

        ~Orchestrator()
        {
            m_StateMachine.Stop();
        }

        internal void SetPreconditionForChange(Action precondition)
        {
            m_Requirements = precondition;
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

            m_Requirements?.Invoke();
            NextOperations.Clear();
            NextOperations.Enqueue(() =>
            {
                CurrentModule = new GameModule(setup, configuration, this);
                CurrentModule.InnerLoad();
            });

            Transition transition = setup.GetTransition();
            transition?.Configure(MainProcess.Time, configuration);
            LaunchTransition(transition);
        }

        internal void UnloadModule()
        {
            m_Requirements?.Invoke();
            NextOperations.Clear();
            NextOperations.Enqueue(() => { CurrentModule?.InnerUnload(); });

            LaunchTransition(CurrentTransition);
        }

        internal void ReloadModule()
        {
            m_Requirements?.Invoke();
            NextOperations.Clear();
            NextOperations.Enqueue(() => { CurrentModule?.InnerReload(); });

            LaunchTransition(CurrentTransition);
        }

        internal void SwitchToModule(IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, Parent?.CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup.GetType());

            m_Requirements?.Invoke();
            NextOperations.Clear();
            NextOperations.Enqueue(() => { CurrentModule?.InnerUnload(); });
            NextOperations.Enqueue(() =>
            {
                CurrentModule = new GameModule(setup, configuration, this);
                CurrentModule.InnerLoad();
            });

            Transition transition = setup.GetTransition();
            transition?.Configure(MainProcess.Time, configuration);
            LaunchTransition(transition);
        }

        internal void AddSubmodule(string subcategory, IGameModuleSetup setup, Configuration configuration = null)
        {
            if (!CheckModuleValidity(setup, CurrentModule))
                return;

            configuration = configuration ?? MainProcess.GetModuleConfiguration(setup.GetType());

            if (State != OrchestratorState.Operational)
                throw new InvalidOperationException($"Cannot add a submodule during a transition phase of the parent module");

            Orchestrator childOrchestrator = new Orchestrator(subcategory, MainProcess, this);
            Children.Add(childOrchestrator);
            childOrchestrator.LoadModule(setup, configuration);
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
            CurrentModule = null;
        }

        #region private
        private bool CheckModuleValidity(IGameModuleSetup moduleSetup, GameModule parent)
        {
            if (moduleSetup.RequiredServiceSetup != null && moduleSetup.RequiredServiceSetup != MainProcess.Services.Id)
            {
                Log.Error(TAG, $"Requirements are not met for module {moduleSetup.Name}. Current services: {MainProcess.Services.Id}. Expected services: {moduleSetup.RequiredServiceSetup}");
                return false;
            }

            if (moduleSetup.RequiredParentSetup != null && moduleSetup.RequiredParentSetup != parent?.Id)
            {
                Log.Error(TAG, $"Requirements are not met for module {moduleSetup.Name}. Current parent module: {parent?.Id}. Expected parent module: {moduleSetup.RequiredParentSetup}");
                return false;
            }

            return true;
        }

        private void LaunchTransition(Transition transition)
        {
            switch (State)
            {
                case OrchestratorState.Wait:
                case OrchestratorState.Operational:
                    UpdateTransition(transition, true);
                    m_StateMachine.SetState(OrchestratorState.EnterTransition, priority: 100);
                    break;

                case OrchestratorState.EnterTransition:
                case OrchestratorState.RunTransition:
                case OrchestratorState.ExitTransition:
                case OrchestratorState.ChangeTransition:
                    if (transition == CurrentTransition)
                    {
                        OrchestratorState state = State != OrchestratorState.ExitTransition ? State : OrchestratorState.EnterTransition;
                        m_StateMachine.SetState(state, priority: 100);
                    }
                    else
                    {
                        UpdateTransition(transition, false);
                        m_StateMachine.SetState(OrchestratorState.ChangeTransition, priority: 100);
                    }
                    break;
            }
        }

        private void UpdateTransition(Transition transition, bool immediate)
        {
            if (transition != CurrentTransition)
            {
                if (immediate)
                    CurrentTransition?.BaseCleanup();
                else if (CurrentTransition != null)
                    PastTransitions.Enqueue(CurrentTransition);

                CurrentTransition = transition;
                CurrentTransition?.BasePrepare();
            }
        }
        #endregion
    }
}
