﻿using GameEngine.FSM;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Rules.Dependencies;
using System;
using System.Diagnostics;

namespace GameEngine.PJR.Jobs.States
{
    /// <summary>
    /// The FSM state corresponding to the DependencyInjection state of the GameJob, in which the dependencies referenced in the GameRules are filled
    /// </summary>
    internal class DependencyInjectionState : FSMState<GameJobState>
    {
        public override GameJobState Id => GameJobState.DependencyInjection;

        private GameJob m_GameJob;
        private DependencyProvider m_InternalProvider;
        private Stopwatch m_UpdateTime;
        private PerformancePolicy m_Performance;

        public DependencyInjectionState(GameJob gameMode)
        {
            m_GameJob = gameMode;
            m_UpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            m_Performance = m_GameJob.PerformancePolicy;
        }

        public override void Update()
        {
            m_UpdateTime.Restart();

            try
            {
                if (m_InternalProvider == null)
                {
                    m_InternalProvider = DependencyUtils.ExtractDependencies(m_GameJob.Rules);

                    if (m_GameJob.IsServiceJob)
                    {
                        m_GameJob.ParentProcess.ServiceProvider = m_InternalProvider;
                    }

                    if (m_UpdateTime.ElapsedMilliseconds >= m_Performance.MaxFrameDuration)
                        return;
                }

                DependencyProvider serviceProvider = m_GameJob.ParentProcess.ServiceProvider;
                DependencyProvider ruleProvider = m_GameJob.IsServiceJob ? null : m_InternalProvider;
                DependencyUtils.InjectDependencies(m_GameJob.Rules, serviceProvider, ruleProvider);

                m_GameJob.GoToNextState();
            }
            catch (Exception)
            {
                if (!m_GameJob.ErrorPolicy.IgnoreExceptions)
                {
                    m_GameJob.OnError();
                }
            }

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_UpdateTime.Reset();
        }
    }
}