using GameEngine.Core.System;
using GameEngine.Core.Unity.Logger;
using GameEngine.Core.Unity.System;
using GameEngine.PMR.Process;
using UnityEngine;

namespace GameEngine.PMR.Unity.Process
{
    /// <summary>
    /// Root monobehaviour script that serves as an entry point for the application.
    /// </summary>
    public abstract class ApplicationRoot : MonoBehaviour
    {
        private GameProcess m_ApplicationProcess;

        /// <summary>
        /// Called when the root is being loaded
        /// </summary>
        public void Awake()
        {
            SetupLogs();

            m_ApplicationProcess = new GameProcess(GetProcessSetup(), GetTime());
        }

        /// <summary>
        /// Called when the root object becomes enabled and active
        /// </summary>
        public void OnEnable()
        {
            if (m_ApplicationProcess.IsStarted)
                m_ApplicationProcess.Restart();
        }

        /// <summary>
        /// Called on the first time the root is enabled just before any of the update methods
        /// </summary>
        public void Start()
        {
            m_ApplicationProcess.Start();
        }

        /// <summary>
        /// Called at frame-rate independant frequency, used for physics calculations.
        /// </summary>
        public void FixedUpdate()
        {
            m_ApplicationProcess.FixedUpdate();
        }

        /// <summary>
        /// Called every frame if the root is enabled and active
        /// </summary>
        public void Update()
        {
            m_ApplicationProcess.Update();
        }

        /// <summary>
        /// Called every frame after the update if the root is enabled and active
        /// </summary>
        public void LateUpdate()
        {
            m_ApplicationProcess.LateUpdate();
        }

        /// <summary>
        /// Called before the application quits
        /// </summary>
        public void OnApplicationQuit()
        {
            m_ApplicationProcess.OnQuit();
        }

        /// <summary>
        /// Called when the root becomes disabled and inactive
        /// </summary>
        public void OnDisable()
        {
            if (m_ApplicationProcess.IsStarted)
                m_ApplicationProcess.Pause();
        }

        /// <summary>
        /// Called when the root instance is being destroyed
        /// </summary>
        public void OnDestroy()
        {
            m_ApplicationProcess = null;
        }

        /// <summary>
        /// Setup the logs using the LogSettings defined in the project.
        /// The method can be overriden if necessary for better customization
        /// </summary>
        protected virtual void SetupLogs()
        {
            LogSettings logSettings = LogSettings.GetSettings();
            LogSetup.InitializeLogs(logSettings);
        }

        /// <summary>
        /// Get a provider for the time information that will be used in the process (default is given by UnityEngine.Time).
        /// The method can be overriden if necessary for better customization
        /// </summary>
        /// <returns>The time provider, implementing IProcessTime interface</returns>
        protected virtual ITime GetTime()
        {
            return new UnityTime();
        }

        /// <summary>
        /// Get the setup defining the characteristics of the main application process
        /// </summary>
        /// <returns>The main process setup</returns>
        protected abstract IGameProcessSetup GetProcessSetup();
    }
}
