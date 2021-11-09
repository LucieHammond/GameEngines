using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Rendering;
using GameEngine.Core.Unity.Utilities;
using GameEngine.PMR.Process.Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable image on screen
    /// </summary>
    public class ImageTransition : Transition
    {
        private ImageDescriptor m_ImageDescriptor;
        private GameObject m_CanvasObject;
        private Image m_ImageComponent;
        private float m_FadeDuration;
        private FadeRenderer m_FadeRenderer;

        /// <summary>
        /// Create a new instance of ImageTransition
        /// </summary>
        /// <param name="image">A descriptor characterizing the image to display</param>
        /// <param name="fadeDuration">The time it should take to fade the image (in seconds)</param>
        public ImageTransition(ImageDescriptor image, float fadeDuration)
        {
            m_ImageDescriptor = image;
            m_FadeDuration = fadeDuration;
        }

        /// <summary>
        /// <see cref="Transition.Initialize()"/>
        /// </summary>
        protected override void Initialize()
        {
            m_CanvasObject = new GameObject("Transition Root");
            Canvas canvas = m_CanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            GameObject imageObject = new GameObject("Transition Image");
            imageObject.transform.parent = m_CanvasObject.transform;

            m_ImageComponent = imageObject.CreateImage(m_ImageDescriptor);
            m_FadeRenderer = new FadeRenderer(m_ImageComponent, m_FadeDuration, false);
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
