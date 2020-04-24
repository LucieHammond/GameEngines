﻿using GameEngine.PSMR.Process;

namespace GameEngine.PSMR.Services.Standard
{
    /// <summary>
    /// A basic GameService implementing the IProcessAccessor interface
    /// </summary>
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
