using GameEngine.Core.Descriptors;
using GameEngine.Core.Rendering;
using GameEngine.Core.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable text on screen
    /// </summary>
    public class TextTransition : Transition
    {
        private const string TRANSITION_LAYER = "Transition";

        private TextDescriptor m_TextDescriptor;
        private GameObject m_CanvasObject;
        private Text m_TextComponent;
        private float m_FadeDuration;
        private FadeRenderer m_FadeRenderer;

        /// <summary>
        /// Create a new instance of TextTransition
        /// </summary>
        /// <param name="text">A descriptor characterizing the text to display</param>
        /// <param name="fadeDuration">The time it should take to fade the text (in seconds)</param>
        public TextTransition(TextDescriptor text, float fadeDuration)
        {
            m_TextDescriptor = text;
            m_FadeDuration = fadeDuration;
        }

        /// <summary>
        /// <see cref="Transition.Initialize()"/>
        /// </summary>
        protected override void Initialize()
        {
            m_CanvasObject = new GameObject("Transition Root");
            m_CanvasObject.layer = LayerMask.NameToLayer(TRANSITION_LAYER);

            Canvas canvas = m_CanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            GameObject textObject = new GameObject("Transition Text");
            textObject.transform.parent = m_CanvasObject.transform;

            m_TextComponent = textObject.CreateText(m_TextDescriptor);
            m_FadeRenderer = new FadeRenderer(m_TextComponent, m_FadeDuration, false);
        }

        /// <summary>
        /// <see cref="Transition.Enter()"/>
        /// </summary>
        protected override void Enter()
        {
            m_FadeRenderer.StartFadeIn(MarkActivated);
        }

        /// <summary>
        /// <see cref="Transition.Update()"/>
        /// </summary>
        protected override void Update()
        {
            if (State != TransitionState.Active)
                m_FadeRenderer.Update();
        }

        /// <summary>
        /// <see cref="Transition.Exit()"/>
        /// </summary>
        protected override void Exit()
        {
            m_FadeRenderer.StartFadeOut(MarkDeactivated);
        }

        /// <summary>
        /// <see cref="Transition.Cleanup()"/>
        /// </summary>
        protected override void Cleanup()
        {
            GameObject.Destroy(m_CanvasObject);
        }
    }
}
