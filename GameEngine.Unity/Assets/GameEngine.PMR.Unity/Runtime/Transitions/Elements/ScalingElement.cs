using GameEngine.Core.Unity.Rendering;
using GameEngine.PMR.Process.Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions.Elements
{
    /// <summary>
    /// A transition element specialized in managing the scaling up and down of a graphic component
    /// </summary>
    public class ScalingElement : ITransitionElement
    {
        private ScaleRenderer m_ScaleRenderer;

        private float m_ScaleUpDelay;
        private float m_ScaleUpDuration;
        private float m_ScaleDownDelay;
        private float m_ScaleDownDuration;

        /// <summary>
        /// Create a new instance of a ScalingElement
        /// </summary>
        /// <param name="graphicElement">The graphic object to be scaled during transition</param>
        /// <param name="scaleUpDuration">The duration of the scale up (in seconds)</param>
        /// <param name="scaleDownDuration">The duration of the scale down (in seconds)</param>
        /// <param name="scaleUpDelay">The delay of the scale up (in seconds)</param>
        /// <param name="scaleDownDelay">The delay of the scale down (in seconds)</param>
        public ScalingElement(Graphic graphicElement,  float scaleUpDuration, float scaleDownDuration, float scaleUpDelay = 0, float scaleDownDelay = 0)
        {
            m_ScaleRenderer = new ScaleRenderer(graphicElement, false);

            m_ScaleUpDelay = scaleUpDelay;
            m_ScaleUpDuration = scaleUpDuration;
            m_ScaleDownDelay = scaleDownDelay;
            m_ScaleDownDuration = scaleDownDuration;
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionEntry()"/>
        /// </summary>
        public void OnStartTransitionEntry()
        {
            m_ScaleRenderer.StartScaleUp(m_ScaleUpDuration);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionEntry()"/>
        /// </summary>
        public void UpdateTransitionEntry()
        {
            if (m_ScaleUpDelay > 0)
                m_ScaleUpDelay -= Time.deltaTime;
            else
                m_ScaleRenderer.Update();
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionEntry()"/>
        /// </summary>
        public void OnFinishTransitionEntry()
        {
            m_ScaleRenderer.SetFullActivation(true);
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnStartTransitionExit()"/>
        /// </summary>
        public void OnStartTransitionExit()
        {
            m_ScaleRenderer.StartScaleDown(m_ScaleDownDuration);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateTransitionExit()"/>
        /// </summary>
        public void UpdateTransitionExit()
        {
            if (m_ScaleDownDelay > 0)
                m_ScaleDownDelay -= Time.deltaTime;
            else
                m_ScaleRenderer.Update();
        }

        /// <summary>
        /// <see cref="ITransitionElement.OnFinishTransitionExit()"/>
        /// </summary>
        public void OnFinishTransitionExit()
        {
            m_ScaleRenderer.SetFullActivation(false);
        }

        /// <summary>
        /// <see cref="ITransitionElement.UpdateRunningTransition(float, string)"/>
        /// </summary>
        public void UpdateRunningTransition(float loadingProgress, string loadingAction) { }
    }
}
