using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Utilities;
using GameEngine.PMR.Process.Transitions;
using GameEngine.PMR.Unity.Transitions.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable text on screen
    /// </summary>
    public class TextTransition : StandardTransition
    {
        private TextDescriptor m_TextDescriptor;
        private GameObject m_CanvasObject;
        private Text m_TextComponent;
        private float m_FadeDuration;

        /// <summary>
        /// Create a new instance of TextTransition
        /// </summary>
        /// <param name="text">A descriptor characterizing the text to display</param>
        /// <param name="displayDuration">The minimum time for which the text should be displayed (in seconds)</param>
        /// <param name="fadeDuration">The time it should take to fade the text (in seconds)</param>
        public TextTransition(TextDescriptor text, float displayDuration, float fadeDuration)
        {
            m_TextDescriptor = text;
            m_FadeDuration = fadeDuration;

            SetTransitionTimes(displayDuration, fadeDuration, fadeDuration);
        }

        /// <summary>
        /// <see cref="Transition.Prepare()"/>
        /// </summary>
        protected override void Prepare()
        {
            m_CanvasObject = new GameObject("Transition Root");
            Canvas canvas = m_CanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            GameObject textObject = new GameObject("Transition Text");
            textObject.transform.parent = m_CanvasObject.transform;

            m_TextComponent = textObject.CreateText(m_TextDescriptor);
            m_CustomElements.Add(new FadingElement(m_TextComponent, m_FadeDuration, m_FadeDuration));

            MarkReady();
        }

        /// <summary>
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup()
        {
            m_CustomElements.Clear();
            Object.Destroy(m_CanvasObject);
        }
    }
}
