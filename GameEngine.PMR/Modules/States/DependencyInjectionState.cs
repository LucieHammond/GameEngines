using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the DependencyInjection state of the GameModule, in which the dependencies referenced in the GameRules are filled
    /// </summary>
    internal class DependencyInjectionState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.DependencyInjection;

        private GameModule m_GameModule;
        private DependencyProvider m_InternalProvider;
        private Stopwatch m_UpdateTime;
        private PerformancePolicy m_Performance;

        public DependencyInjectionState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameModule.Name, $"Inject dependencies");
            m_Performance = m_GameModule.PerformancePolicy;
        }

        public override void Update()
        {
            m_UpdateTime.Restart();

            try
            {
                if (m_InternalProvider == null)
                {
                    m_InternalProvider = DependencyUtils.ExtractDependencies(m_GameModule.Rules);

                    if (m_GameModule.IsService)
                    {
                        m_GameModule.ParentProcess.ServiceProvider = m_InternalProvider;
                    }

                    if (m_UpdateTime.ElapsedMilliseconds >= m_Performance.MaxFrameDuration)
                        return;
                }

                DependencyProvider serviceProvider = m_GameModule.ParentProcess.ServiceProvider;
                DependencyProvider ruleProvider = m_GameModule.IsService ? null : m_InternalProvider;
                DependencyUtils.InjectDependencies(m_GameModule.Rules, serviceProvider, ruleProvider);

                m_GameModule.GoToNextState();
            }
            catch (Exception e)
            {
                Log.Exception(m_GameModule.Name, e);
                m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad);
            }

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            Log.Info(m_GameModule.Name, $"Dependency injection completed");
            m_UpdateTime.Reset();
        }
    }
}