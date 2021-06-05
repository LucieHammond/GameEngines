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
        internal ModuleOrchestratorState State => m_StateMachine.CurrentStateId;

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

        internal void Reset()
        {
            CurrentModule = null;
        }

        internal void Stop()
        {
            m_StateMachine.Stop();
        }

        internal void GoToState(ModuleOrchestratorState state)
        {
            m_StateMachine.SetState(state);
        }
    }
}
