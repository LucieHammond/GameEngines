using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Utilities;
using GameEngine.PMR.Process.Transitions;
using GameEngine.PMR.Unity.Transitions.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.PMR.Unity.Transitions
{
    /// <summary>
    /// A predefined transition that displays a customizable image on screen
    /// </summary>
    public class ImageTransition : StandardTransition
    {
        private ImageDescriptor m_ImageDescriptor;
        private GameObject m_CanvasObject;
        private Image m_ImageComponent;
        private float m_FadeDuration;

        /// <summary>
        /// Create a new instance of ImageTransition
        /// </summary>
        /// <param name="image">A descriptor characterizing the image to display</param>
        /// <param name="displayDuration">The minimum time for which the image should be displayed (in seconds)</param>
        /// <param name="fadeDuration">The time it should take to fade the image (in seconds)</param>
        public ImageTransition(ImageDescriptor image, float displayDuration, float fadeDuration)
        {
            m_ImageDescriptor = image;
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

            m_ImageComponent = imageObject.CreateImage(m_ImageDescriptor);
            m_CustomElements.Add(new FadingElement(m_ImageComponent, m_FadeDuration, m_FadeDuration));

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
