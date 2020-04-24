using GameEngine.FSM;
using GameEngine.PSMR.Modes.Policies;
using GameEngine.PSMR.Rules.Dependencies;
using System;
using System.Diagnostics;

namespace GameEngine.PSMR.Modes.States
{
    /// <summary>
    /// The FSM state corresponding to the DependencyInjection state of the GameMode, in which the dependencies referenced in the GameRules are filled
    /// </summary>
    internal class DependencyInjectionState : FSMState<GameModeState>
    {
        public override GameModeState Id => GameModeState.DependencyInjection;

        private GameMode m_GameMode;
        private DependencyProvider m_InternalProvider;
        private Stopwatch m_UpdateTime;
        private PerformancePolicy m_Performance;

        public DependencyInjectionState(GameMode gameMode)
        {
            m_GameMode = gameMode;
            m_UpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            m_Performance = m_GameMode.PerformancePolicy;
        }

        public override void Update()
        {
            m_UpdateTime.Restart();

            try
            {
                if (m_InternalProvider == null)
                {
                    m_InternalProvider = DependencyUtils.ExtractDependencies(m_GameMode.Rules);

                    if (m_GameMode.IsServiceMode)
                    {
                        m_GameMode.ParentProcess.ServiceProvider = m_InternalProvider;
                    }

                    if (m_UpdateTime.ElapsedMilliseconds >= m_Performance.MaxFrameDuration)
                        return;
                }

                DependencyProvider serviceProvider = m_GameMode.ParentProcess.ServiceProvider;
                DependencyProvider ruleProvider = m_GameMode.IsServiceMode ? null : m_InternalProvider;
                DependencyUtils.InjectDependencies(m_GameMode.Rules, serviceProvider, ruleProvider, m_GameMode.InitialConfiguration);

                m_GameMode.GoToNextState();
            }
            catch (Exception)
            {
                if (!m_GameMode.ErrorPolicy.IgnoreExceptions)
                {
                    m_GameMode.OnError();
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