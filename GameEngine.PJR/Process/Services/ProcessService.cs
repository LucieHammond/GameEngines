using GameEngine.PJR.Rules.Dependencies.Attributes;

namespace GameEngine.PJR.Process.Services
{
    /// <summary>
    /// A basic GameService implementing the IProcessAccessor interface
    /// </summary>
    [DependencyProvider(typeof(IProcessAccessor))]
    internal class ProcessService : GameService, IProcessAccessor
    {
        private GameProcess m_CurrentProcess;

        internal ProcessService(GameProcess process)
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
