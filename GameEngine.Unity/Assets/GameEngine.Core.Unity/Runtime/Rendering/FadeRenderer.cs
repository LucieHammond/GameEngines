using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Core.Unity.Rendering
{
    /// <summary>
    /// A rendering class designed to manage the progressive fading of a single graphic object
    /// </summary>
    public class FadeRenderer
    {
        private enum FadeState
        {
            Inactive,
            FadeIn,
            FadeOut,
        }

        /// <summary>
        /// The graphic object to be faded
        /// </summary>
        public Graphic Graphic { get; private set; }

        /// <summary>
        /// If the graphic is fully unfaded, which means normally displayed
        /// </summary>
        public bool IsFullyIn => Graphic.isActiveAndEnabled && m_OpacityRate >= 1;

        /// <summary>
        /// If the graphic is fully faded, which means invisible
        /// </summary>
        public bool IsFullyOut => !Graphic.isActiveAndEnabled && m_OpacityRate <= 0;

        private Color m_OriginalColor;
        private Color m_FadeColor;
        private float m_FadeDuration;
        private float m_OpacityRate;
        private FadeState m_CurrentState;
        private Action m_FadeInCallback;
        private Action m_FadeOutCallback;

        /// <summary>
        /// Create a new instance of FadeRenderer
        /// </summary>
        /// <param name="graphicToFade">The graphic object to be faded</param>
        /// <param name="initiallyActive">If the graphic should be displayed in the initial state</param>
        public FadeRenderer(Graphic graphicToFade, bool initiallyActive = false)
            : this(graphicToFade, new Color(graphicToFade.color.r, graphicToFade.color.g, graphicToFade.color.b, 0), initiallyActive)
        {
        }

        /// <summary>
        /// Create a new instance of FadeRenderer
        /// </summary>
        /// <param name="graphicToFade">The graphic object to be faded</param>
        /// <param name="fadeColor">The color that should be applied to the graphic when faded</param>
        /// <param name="initiallyActive">If the graphic should be displayed in the initial state</param>
        public FadeRenderer(Graphic graphicToFade, Color fadeColor, bool initiallyActive = false)
        {
            Graphic = graphicToFade;
            m_OriginalColor = graphicToFade.color;
            m_FadeColor = fadeColor;
            m_FadeDuration = 0f;
            m_CurrentState = FadeState.Inactive;

            Graphic.enabled = true;
            Graphic.gameObject.SetActive(initiallyActive);
            SetGraphicOpacity(initiallyActive ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Update the rendering of the graphic given the ongoing fading phase
        /// </summary>
        public void Update()
        {
            if (m_CurrentState != FadeState.Inactive)
            {
                float deltaFade = m_FadeDuration > 0 ? Time.deltaTime / m_FadeDuration : 1f;
                int direction = m_CurrentState == FadeState.FadeIn ? 1 : -1;
                float opacity = Mathf.Clamp(m_OpacityRate + direction * deltaFade, 0.0f, 1.0f);

                SetGraphicOpacity(opacity);

                CheckFadeCompletion();
            }
        }

        /// <summary>
        /// Initiate a FadeIn phase, that makes the graphic appear gradually
        /// </summary>
        /// <param name="fadeDuration">The duration of the fade (in seconds)</param>
        /// <param name="onFinish">A callback method to call when the graphic has fully appeared</param>
        public void StartFadeIn(float fadeDuration, Action onFinish = null)
        {
            m_FadeDuration = fadeDuration;
            m_FadeInCallback = onFinish;

            Graphic.gameObject.SetActive(true);
            m_CurrentState = FadeState.FadeIn;
        }

        /// <summary>
        /// Initiate a FadeOut phase, that makes the graphic disappear gradually
        /// </summary>
        /// <param name="fadeDuration">The duration of the fade (in seconds)</param>
        /// <param name="onFinish">A callback method to call when the graphic has fully disappeared</param>
        public void StartFadeOut(float fadeDuration, Action onFinish = null)
        {
            m_FadeDuration = fadeDuration;
            m_FadeOutCallback = onFinish;

            Graphic.gameObject.SetActive(true);
            m_CurrentState = FadeState.FadeOut;
        }

        /// <summary>
        /// Initiate a FadeIn phase immediately followed by a FadeOut phase, making the graphing appear and disappear
        /// </summary>
        /// <param name="fadeDuration">The duration of the fade (in seconds)</param>
        /// <param name="onHalf">A callback method to call when the first phase is finished</param>
        /// <param name="onFinish">A callback method to call when both consecutive phases are finished</param>
        public void StartFadeBetween(float fadeDuration, Action onHalf = null, Action onFinish = null)
        {
            m_FadeDuration = fadeDuration;
            m_FadeInCallback = () => { onHalf?.Invoke(); m_CurrentState = FadeState.FadeOut; };
            m_FadeOutCallback = onFinish;

            Graphic.gameObject.SetActive(true);
            m_CurrentState = FadeState.FadeIn;
        }

        public void SetFullActivation(bool active)
        {
            if (active)
                SetGraphicOpacity(1f);
            else
                SetGraphicOpacity(0f);
        }

        private void SetGraphicOpacity(float opacity)
        {
            m_OpacityRate = opacity;
            Graphic.color = Color.Lerp(m_FadeColor, m_OriginalColor, m_OpacityRate);

            if (m_OpacityRate <= 0)
                Graphic.gameObject.SetActive(false);
        }

        private void CheckFadeCompletion()
        {
            if (m_OpacityRate <= 0)
            {
                m_CurrentState = FadeState.Inactive;
                m_FadeOutCallback?.Invoke();
            }

            if (m_OpacityRate >= 1)
            {
                m_CurrentState = FadeState.Inactive;
                m_FadeInCallback?.Invoke();
            }
        }
    }
}