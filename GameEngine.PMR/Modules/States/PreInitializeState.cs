using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Modules.Specialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the PreInitialize state of the GameModule, in which all specialized tasks have their initialization phase executed
    /// </summary>
    internal class PreInitializeState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.PreInitialize;

        private GameModule m_GameModule;
        private IEnumerator<SpecialTask> m_TasksEnumerator;
        private Stopwatch m_UpdateTime;
        private int m_NbStepsExecuted;
        private float m_StepProgress;

        internal PreInitializeState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Pre-Initialize");

            m_GameModule.ReportLoadingProgress(0f);
            m_StepProgress = 1.0f / (m_GameModule.SpecialTasks.Count + 3);

            m_NbStepsExecuted = 0;
            m_TasksEnumerator = m_GameModule.SpecialTasks.GetEnumerator();
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
                    if (m_TasksEnumerator.Current.State == SpecialTaskState.Created ||
                    m_TasksEnumerator.Current.State == SpecialTaskState.UnloadCompleted)
                    {
                        m_TasksEnumerator.Current.BaseInitialize(m_GameModule.Rules);
                    }

                    m_TasksEnumerator.Current.BaseUpdate(m_GameModule.PerformancePolicy.MaxFrameDuration);

                    ReportProgress(m_TasksEnumerator.Current.GetProgress());
                }
                catch (Exception e)
                {
                    Log.Exception(m_GameModule.Name, e);
                    m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad);
                }

                if (m_TasksEnumerator.Current.State == SpecialTaskState.InitCompleted)
                {
                    m_NbStepsExecuted++;
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

        private void ReportProgress(float stepProgress)
        {
            float totalProgress = MathUtils.Lerp(stepProgress, m_NbStepsExecuted * m_StepProgress, (m_NbStepsExecuted + 1) * m_StepProgress);
            m_GameModule.ReportLoadingProgress(totalProgress);
        }
    }
}
