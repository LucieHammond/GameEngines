using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Core.Unity.Rendering
{
    /// <summary>
    /// A rendering class designed to manage the progressive scaling of a single graphic object
    /// </summary>
    public class ScaleRenderer
    {
        private enum ScaleState
        {
            Inactive,
            ScaleUp,
            ScaleDown,
        }

        /// <summary>
        /// The graphic object to be scaled
        /// </summary>
        public Graphic Graphic { get; private set; }

        /// <summary>
        /// If the graphic is fully enlarged, which means normally displayed
        /// </summary>
        public bool IsFullyUp => Graphic.isActiveAndEnabled && m_ScaleRate >= 1;

        /// <summary>
        /// If the graphic is fully shrunken, which means invisible
        /// </summary>
        public bool IsFullyDown => !Graphic.isActiveAndEnabled && m_ScaleRate <= 0;

        private Vector3 m_OriginalScale;
        private Vector3 m_ReducedScale;
        private float m_ScaleDuration;
        private float m_ScaleRate;
        private ScaleState m_CurrentState;
        private Action m_ScaleUpCallback;
        private Action m_ScaleDownCallback;

        /// <summary>
        /// Create a new instance of ScaleRenderer
        /// </summary>
        /// <param name="graphicToFade">The graphic object to be scaled</param>
        /// <param name="initiallyActive">If the graphic should be displayed in the initial state</param>
        public ScaleRenderer(Graphic graphicToFade, bool initiallyActive = false)
            : this(graphicToFade, Vector3.zero, initiallyActive)
        {
        }

        /// <summary>
        /// Create a new instance of ScaleRenderer
        /// </summary>
        /// <param name="graphicToFade">The graphic object to be scaled</param>
        /// <param name="reducedScale">The minimal scale that should be applied to the graphic when shrunken</param>
        /// <param name="initiallyActive">If the graphic should be displayed in the initial state</param>
        public ScaleRenderer(Graphic graphicToFade, Vector3 reducedScale, bool initiallyActive = false)
        {
            Graphic = graphicToFade;
            m_OriginalScale = graphicToFade.rectTransform.localScale;
            m_ReducedScale = reducedScale;
            m_ScaleDuration = 0f;
            m_CurrentState = ScaleState.Inactive;

            Graphic.enabled = true;
            Graphic.gameObject.SetActive(initiallyActive);
            SetGraphicScale(initiallyActive ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Update the rendering of the graphic given the ongoing scaling phase
        /// </summary>
        public void Update()
        {
            if (m_CurrentState != ScaleState.Inactive)
            {
                float deltaScale = m_ScaleDuration > 0 ? Time.deltaTime / m_ScaleDuration : 1f;
                int direction = m_CurrentState == ScaleState.ScaleUp ? 1 : -1;
                float scale = Mathf.Clamp(m_ScaleRate + direction * deltaScale, 0.0f, 1.0f);

                SetGraphicScale(scale);

                CheckScaleCompletion();
            }
        }

        /// <summary>
        /// Initiate a ScaleUp phase, that makes the graphic grow gradually
        /// </summary>
        /// <param name="scaleDuration">The duration of the enlargement (in seconds)</param>
        /// <param name="onFinish">A callback method to call when the graphic is fully enlarged</param>
        public void StartScaleUp(float scaleDuration, Action onFinish = null)
        {
            m_ScaleDuration = scaleDuration;
            m_ScaleUpCallback = onFinish;

            Graphic.gameObject.SetActive(true);
            m_CurrentState = ScaleState.ScaleUp;
        }

        /// <summary>
        /// Initiate a ScaleDown phase, that makes the graphic shrink gradually
        /// </summary>
        /// <param name="scaleDuration">The duration of the shrinkage (in seconds)</param>
        /// <param name="onFinish">A callback method to call when the graphic is fully shrunken</param>
        public void StartScaleDown(float scaleDuration, Action onFinish = null)
        {
            m_ScaleDuration = scaleDuration;
            m_ScaleDownCallback = onFinish;

            Graphic.gameObject.SetActive(true);
            m_CurrentState = ScaleState.ScaleDown;
        }

        /// <summary>
        /// Initiate a ScaleUp phase immediately followed by a ScaleDown phase, making the graphing grow and shrink
        /// </summary>
        /// <param name="scaleDuration">The duration of the scale (in seconds)</param>
        /// <param name="onHalf">A callback method to call when the first phase is finished</param>
        /// <param name="onFinish">A callback method to call when both consecutive phases are finished</param>
        public void StartScaleBetween(float scaleDuration, Action onHalf = null, Action onFinish = null)
        {
            m_ScaleDuration = scaleDuration;
            m_ScaleUpCallback = () => { onHalf?.Invoke(); m_CurrentState = ScaleState.ScaleDown; };
            m_ScaleDownCallback = onFinish;

            Graphic.gameObject.SetActive(true);
            m_CurrentState = ScaleState.ScaleUp;
        }

        public void SetFullActivation(bool active)
        {
            if (active)
                SetGraphicScale(1f);
            else
                SetGraphicScale(0f);
        }

        private void SetGraphicScale(float scale)
        {
            m_ScaleRate = scale;
            Graphic.rectTransform.localScale = Vector3.Lerp(m_ReducedScale, m_OriginalScale, m_ScaleRate);

            if (m_ScaleRate <= 0)
                Graphic.gameObject.SetActive(false);
        }

        private void CheckScaleCompletion()
        {
            if (m_ScaleRate <= 0)
            {
                m_CurrentState = ScaleState.Inactive;
                m_ScaleDownCallback?.Invoke();
            }

            if (m_ScaleRate >= 1)
            {
                m_CurrentState = ScaleState.Inactive;
                m_ScaleUpCallback?.Invoke();
            }
        }
    }
}