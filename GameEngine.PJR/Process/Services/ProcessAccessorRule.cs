using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Dependencies.Attributes;

namespace GameEngine.PJR.Process.Services
{
    /// <summary>
    /// A basic Rule implementing the IProcessAccessor interface, for accessing the process instance.
    /// </summary>
    [DependencyProvider(typeof(IProcessAccessor))]
    internal class ProcessAccessorRule : GameRule, IProcessAccessor
    {
        private GameProcess m_CurrentProcess;

        internal ProcessAccessorRule(GameProcess process)
        {
            m_CurrentProcess = process;
        }

        public GameProcess GetCurrentProcess()
        {
            return m_CurrentProcess;
        }

        protected override void Initialize()
        {

        }

        protected override void Update()
        {

        }

        protected override void Unload()
        {

        }
    }
}
