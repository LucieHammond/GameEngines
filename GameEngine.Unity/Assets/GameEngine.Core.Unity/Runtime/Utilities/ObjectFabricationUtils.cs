using GameEngine.Core.Descriptors;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for GameObject fabrication operations at runtime
    /// </summary>
    public static class ObjectFabricationUtils
    {
        /// <summary>
        /// Create and add on the given gameobject a RectTransform component that matches the shape of the parent transform
        /// </summary>
        /// <param name="gameObject">The gameobject on which to add the RectTransform component</param>
        /// <returns>The newly created RectTranform component</returns>
        public static RectTransform CreateDefaultRectTransform(this GameObject gameObject)
        {
            RectTransform transform = gameObject.AddComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = new Vector2(0.5f, 0.5f);
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;

            return transform;
        }

        /// <summary>
        /// Create and add on the given gameobject an Image component corresponding to the descriptor
        /// </summary>
        /// <param name="gameObject">The gameobject on which to add the Image component</param>
        /// <param name="descriptor">The descriptor characterizing the image</param>
        /// <returns>The newly created Image component</returns>
        public static Image CreateImage(this GameObject gameObject, ImageDescriptor descriptor)
        {
            if (gameObject.GetComponent<RectTransform>() == null)
            {
                gameObject.CreateDefaultRectTransform();
            }

            Image image = gameObject.AddComponent<Image>();
            image.sprite = descriptor.SourceImage;
            image.color = descriptor.Color;
            image.material = descriptor.Material;
            image.type = descriptor.ImageType;
            image.preserveAspect = descriptor.PreserveAspect;

            if (descriptor.ImageType == Image.Type.Simple)
            {
                image.useSpriteMesh = descriptor.UseSpriteMesh;
            }
            else if (descriptor.ImageType == Image.Type.Filled)
            {
                image.fillMethod = descriptor.FillMethod;
                image.fillOrigin = descriptor.FillOrigin;
                image.fillAmount = descriptor.FillAmount;
                image.fillClockwise = descriptor.FillClockwise;
            }

            return image;
        }

        /// <summary>
        /// Create and add on the given gameobject a Text component corresponding to the descriptor
        /// </summary>
        /// <param name="gameObject">The gameobject on which to add the Text component</param>
        /// <param name="descriptor">The descriptor characterizing the text</param>
        /// <returns>The newly created Text component</returns>
        public static Text CreateText(this GameObject gameObject, TextDescriptor descriptor)
        {
            if (gameObject.GetComponent<RectTransform>() == null)
            {
                gameObject.CreateDefaultRectTransform();
            }

            Text text = gameObject.AddComponent<Text>();
            text.text = descriptor.Text;
            text.color = descriptor.Color;
            text.material = descriptor.Material;

            text.font = descriptor.Font;
            text.fontStyle = descriptor.FontStyle;
            text.fontSize = descriptor.FontSize;
            text.lineSpacing = descriptor.LineSpacing;
            text.supportRichText = descriptor.SupportRichText;

            text.alignment = descriptor.Alignment;
            text.alignByGeometry = descriptor.AlignByGeometry;
            text.horizontalOverflow = descriptor.HorizontalOverflow;
            text.verticalOverflow = descriptor.VerticalOverflow;
            text.resizeTextForBestFit = descriptor.ResizeForBestFit;
            if (descriptor.ResizeForBestFit)
            {
                text.resizeTextMinSize = descriptor.ResizeMinSize;
                text.resizeTextMaxSize = descriptor.ResizeMaxSize;
            }

            return text;
        }

        /// <summary>
        /// Create and add on the given gameobject a VideoPlayer component corresponding to the descriptor 
        /// and a RawImage component displaying the target render texture of the VideoPlayer
        /// </summary>
        /// <param name="gameObject">The gameobject on which to add the VideoPlayer and the RawImage components</param>
        /// <param name="descriptor">The descriptor characterizing the video player</param>
        /// <returns>The newly created RawImage component</returns>
        public static RawImage CreateVideo(this GameObject gameObject, VideoDescriptor descriptor)
        {
            if (gameObject.GetComponent<RectTransform>() == null)
            {
                gameObject.CreateDefaultRectTransform();
            }

            int width = (int)descriptor.SourceVideo.width;
            int height = (int)descriptor.SourceVideo.height;
            RenderTexture render = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
            render.Create();

            VideoPlayer video = gameObject.AddComponent<VideoPlayer>();
            video.source = VideoSource.VideoClip;
            video.clip = descriptor.SourceVideo;
            video.playOnAwake = descriptor.PlayOnAwake;
            video.waitForFirstFrame = true;
            video.isLooping = descriptor.Loop;
            video.skipOnDrop = descriptor.SkipOnDrop;

            video.playbackSpeed = descriptor.PlaybackSpeed;
            video.renderMode = VideoRenderMode.RenderTexture;
            video.targetTexture = render;
            video.aspectRatio = descriptor.AspectRatio;

            video.SetDirectAudioMute(0, descriptor.AudioMute);
            video.SetDirectAudioVolume(0, descriptor.AudioVolume);

            RawImage rawImage = gameObject.AddComponent<RawImage>();
            rawImage.texture = render;

            return rawImage;
        }
    }
}