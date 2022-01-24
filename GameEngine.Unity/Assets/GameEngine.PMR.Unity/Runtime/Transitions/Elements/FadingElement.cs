using GameEngine.Core.Unity.Rendering;
using GameEngine.PMR.Process.Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions.Elements
{
    /// <summary>
    /// A transition element specialized in managing the fading in and out of a graphic component
    /// </summary>
    public class FadingElement : ITransitionElement
    {
        private FadeRenderer m_FadeRenderer;

        private float m_FadeInDelay;
        private float m_FadeInDuration;
        private float m_FadeOutDelay;
        private float m_FadeOutDuration;

        /// <summary>
        /// Create a new instance of FadingElement
        /// </summary>
        /// <param name="graphicElement">The graphic object to be faded during transition</param>
        /// <param name="fadeInDuration">The duration of the fade in (in seconds)</param>
        /// <param name="fadeOutDuration">The duration of the fade out (in seconds)</param>
        /// <param name="fadeInDelay">The delay of the fade in (in seconds)</param>
        /// <param name="fadeOutDelay">The delay of the fade out (in seconds)</param>
        public FadingElement(Graphic graphicElement, float fadeInDuration, float fadeOutDuration, float fadeInDelay = 0, float fadeOutDelay = 0)
        {
            m_FadeRenderer = new FadeRenderer(graphicElement, false);

            m_FadeInDelay = fadeInDelay;
            m_FadeInDuration = fadeInDuration;
            m_FadeOutDelay = fadeOutDelay;
            m_FadeOutDuration = fadeOutDuration;
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionEntry()"/>
        /// </summary>
        public void OnStartTransitionEntry()
        {
            m_FadeRenderer.StartFadeIn(m_FadeInDuration);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionEntry()"/>
        /// </summary>
        public void UpdateTransitionEntry()
        {
            if (m_FadeInDelay > 0)
                m_FadeInDelay -= Time.deltaTime;
            else
                m_FadeRenderer.Update();
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionEntry()"/>
        /// </summary>
        public void OnFinishTransitionEntry()
        {
            m_FadeRenderer.SetFullActivation(true);
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionExit()"/>
        /// </summary>
        public void OnStartTransitionExit()
        {
            m_FadeRenderer.StartFadeOut(m_FadeOutDuration);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionExit()"/>
        /// </summary>
        public void UpdateTransitionExit()
        {
            if (m_FadeOutDelay > 0)
                m_FadeOutDelay -= Time.deltaTime;
            else
                m_FadeRenderer.Update();
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionExit()"/>
        /// </summary>
        public void OnFinishTransitionExit()
        {
            m_FadeRenderer.SetFullActivation(false);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateRunningTransition(float, string)"/>
        /// </summary>
        public void UpdateRunningTransition(float loadingProgress, string loadingAction) { }
    }
}
