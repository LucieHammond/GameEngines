using GameEngine.Core.Descriptors;
using GameEngine.Core.Rendering;
using GameEngine.Core.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable video on screen
    /// </summary>
    public class VideoTransition : Transition
    {
        private VideoDescriptor m_VideoDescriptor;
        private GameObject m_CanvasObject;
        private RawImage m_VideoScreenComponent;
        private float m_FadeDuration;
        private FadeRenderer m_FadeRenderer;

        /// <summary>
        /// Create a new instance of VideoTransition
        /// </summary>
        /// <param name="video">A descriptor characterizing the video to display</param>
        /// <param name="fadeDuration">The time it should take to fade the video (in seconds)</param>
        public VideoTransition(VideoDescriptor video, float fadeDuration)
        {
            m_VideoDescriptor = video;
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

            m_VideoScreenComponent = imageObject.CreateVideo(m_VideoDescriptor);
            m_FadeRenderer = new FadeRenderer(m_VideoScreenComponent, m_FadeDuration, false);
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
