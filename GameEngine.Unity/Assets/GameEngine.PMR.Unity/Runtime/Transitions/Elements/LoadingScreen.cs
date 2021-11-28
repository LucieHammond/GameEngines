using GameEngine.Core.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions.Elements
{
    /// <summary>
    /// A loading screen with a progress bar, a progress legend and a presentation of the current loading action
    /// </summary>
    public class LoadingScreen
    {
        private const string TAG = "LoadingScreen";

        private readonly string m_ProgressBarName;
        private readonly string m_ProgressTextName;
        private readonly string m_ActionMessageName;

        private Slider m_ProgressBar;
        private Text m_ProgressText;
        private Text m_ActionMessage;

        /// <summary>
        /// Create a new instance of LoadingScreen
        /// </summary>
        /// <param name="progressBarName">The name of the progress bar gameobject in the scene</param>
        /// <param name="progressTextName">The name of the progress text gameobject in the scene</param>
        /// <param name="actionMessageName">The name of the loading action message gameobject in the scene</param>
        public LoadingScreen(string progressBarName, string progressTextName, string actionMessageName)
        {
            m_ProgressBarName = progressBarName;
            m_ProgressTextName = progressTextName;
            m_ActionMessageName = actionMessageName;
        }

        /// <summary>
        /// Initialize the loading screen elements by searching them in the current scene hierarchy
        /// </summary>
        public void Setup()
        {
            if (m_ProgressBarName != null)
                m_ProgressBar = FindComponent<Slider>(m_ProgressBarName);

            if (m_ProgressTextName != null)
                m_ProgressText = FindComponent<Text>(m_ProgressTextName);

            if (m_ActionMessageName != null)
                m_ActionMessage = FindComponent<Text>(m_ActionMessageName);
        }

        /// <summary>
        /// Update the loading screen elements with the given progress percentage and action message
        /// </summary>
        /// <param name="loadingProgress">The current loading progress, as a float number between 0 and 1</param>
        /// <param name="loadingAction">The current loading action</param>
        public void Update(float loadingProgress, string loadingAction)
        {
            if (m_ProgressBar != null)
                m_ProgressBar.value = loadingProgress;

            if (m_ProgressText != null)
                m_ProgressText.text = $"{loadingProgress * 100} %";

            if (m_ActionMessage != null)
                m_ActionMessage.text = loadingAction;
        }

        private T FindComponent<T>(string objectId)
        {
            GameObject gameObject = GameObject.Find(m_ProgressBarName);
            if (gameObject == null)
            {
                Log.Error(TAG, $"Failed to find a gameobject named {objectId} in the scene hierarchy");
                return default;
            }

            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                Log.Error(TAG, $"Failed to find a component of type {typeof(T)} on the gameobject {objectId}");
                return default;
            }

            return component;
        }
    }
}

