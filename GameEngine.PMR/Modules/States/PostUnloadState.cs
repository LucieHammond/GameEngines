using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Specialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the PostUnload state of the GameModule, in which all specialized tasks have their unload phase executed
    /// </summary>
    internal class PostUnloadState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.PostUnload;

        private GameModule m_GameModule;
        private IEnumerator<SpecializedTask> m_TasksEnumerator;
        private Stopwatch m_UpdateTime;

        internal PostUnloadState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Post-Unload");

            m_GameModule.ReportLoadingProgress(0f);

            m_TasksEnumerator = m_GameModule.SpecializedTasks.GetEnumerator();
            if (!m_TasksEnumerator.MoveNext())
                m_TasksEnumerator = null;
        }

        public override void Update()
        {
            m_UpdateTime.Restart();

            do
            {
                if (m_TasksEnumerator == null)
                {
                    m_GameModule.GoToNextState();
                    break;
                }

                try
                {
                    if (m_TasksEnumerator.Current.State == SpecializedTaskState.InitRunning ||
                    m_TasksEnumerator.Current.State == SpecializedTaskState.InitCompleted)
                    {
                        m_TasksEnumerator.Current.BaseUnload(m_GameModule.Rules);
                    }

                    m_TasksEnumerator.Current.BaseUpdate(m_GameModule.PerformancePolicy.MaxFrameDuration);
                }
                catch (Exception e)
                {
                    Log.Exception(m_GameModule.Name, e);
                    m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUnload);
                }

                if (m_TasksEnumerator.Current.State == SpecializedTaskState.UnloadCompleted ||
                    m_TasksEnumerator.Current.State == SpecializedTaskState.Created)
                {
                    if (!m_TasksEnumerator.MoveNext())
                        m_TasksEnumerator = null;
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_GameModule.PerformancePolicy.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_UpdateTime.Reset();
        }
    }
}
