using GameEngine.PMR.Process.Transitions;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions.Elements
{
    /// <summary>
    /// A loading screen with a progress bar, a progress legend and a presentation of the current loading action
    /// </summary>
    public class LoadingScreen : ITransitionElement
    {
        private Slider m_ProgressBar;
        private Text m_ProgressText;
        private Text m_ActionMessage;

        /// <summary>
        /// Create a new instance of LoadingScreen
        /// </summary>
        /// <param name="progressBar">The progress bar gameobject in the scene</param>
        /// <param name="progressText">The progress text gameobject in the scene</param>
        /// <param name="actionMessage">The loading action message gameobject in the scene</param>
        public LoadingScreen(Slider progressBar, Text progressText, Text actionMessage)
        {
            m_ProgressBar = progressBar;
            m_ProgressText = progressText;
            m_ActionMessage = actionMessage;
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionEntry()"/>
        /// </summary>
        public void OnStartTransitionEntry() { }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionEntry()"/>
        /// </summary>
        public void UpdateTransitionEntry() { }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionEntry()"/>
        /// </summary>
        public void OnFinishTransitionEntry() { }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionExit()"/>
        /// </summary>
        public void OnStartTransitionExit() { }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionExit()"/>
        /// </summary>
        public void UpdateTransitionExit() { }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionExit()"/>
        /// </summary>
        public void OnFinishTransitionExit() { }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateRunningTransition(float, string)"/>
        /// </summary>
        public void UpdateRunningTransition(float loadingProgress, string loadingAction)
        {
            if (m_ProgressBar != null)
                m_ProgressBar.value = loadingProgress;

            if (m_ProgressText != null)
                m_ProgressText.text = $"{loadingProgress * 100} %";

            if (m_ActionMessage != null)
                m_ActionMessage.text = loadingAction;
        }
    }
}

