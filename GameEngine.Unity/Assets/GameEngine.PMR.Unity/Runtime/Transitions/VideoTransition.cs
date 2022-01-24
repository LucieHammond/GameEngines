using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Utilities;
using GameEngine.PMR.Process.Transitions;
using GameEngine.PMR.Unity.Transitions.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable video on screen
    /// </summary>
    public class VideoTransition : StandardTransition
    {
        private VideoDescriptor m_VideoDescriptor;
        private GameObject m_CanvasObject;
        private RawImage m_VideoScreenComponent;
        private float m_FadeDuration;

        /// <summary>
        /// Create a new instance of VideoTransition
        /// </summary>
        /// <param name="video">A descriptor characterizing the video to display</param>
        /// <param name="displayDuration">The minimum time for which the video should be displayed (in seconds)</param>
        /// <param name="fadeDuration">The time it should take to fade the video (in seconds)</param>
        public VideoTransition(VideoDescriptor video, float displayDuration, float fadeDuration)
        {
            m_VideoDescriptor = video;
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

            GameObject imageObject = new GameObject("Transition Image");
            imageObject.transform.parent = m_CanvasObject.transform;

            m_VideoScreenComponent = imageObject.CreateVideo(m_VideoDescriptor);
            m_CustomElements.Add(new FadingElement(m_VideoScreenComponent, m_FadeDuration, m_FadeDuration));

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
