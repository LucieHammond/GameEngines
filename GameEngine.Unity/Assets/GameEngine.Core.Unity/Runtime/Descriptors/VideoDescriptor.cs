using UnityEngine;
using UnityEngine.Video;

namespace GameEngine.Core.Unity.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a video player
    /// </summary>
    [CreateAssetMenu(fileName = "NewVideoDescriptor", menuName = "Content Descriptors/Video Descriptor", order = 12)]
    public class VideoDescriptor : ScriptableObject
    {
        /// <summary>
        /// The clip being played by the video player
        /// </summary>
        public VideoClip SourceVideo;

        /// <summary>
        /// Whether the content will start playing back as soon as the component awakes
        /// </summary>
        public bool PlayOnAwake;

        /// <summary>
        /// Determines whether the video player restarts from the beginning when it reaches the end of the clip
        /// </summary>
        public bool Loop;

        /// <summary>
        /// Whether the video player is allowed to skip frames to catch up with current time
        /// </summary>
        public bool SkipOnDrop;

        /// <summary>
        /// Factor by which the basic playback rate will be multiplied
        /// </summary>
        public float PlaybackSpeed;

        /// <summary>
        /// Defines how the video content will be stretched to fill the target area
        /// </summary>
        public VideoAspectRatio AspectRatio;

        /// <summary>
        /// The direct-output audio mute status for the default track
        /// </summary>
        public bool AudioMute;

        /// <summary>
        /// The direct-output audio volume for the default track
        /// </summary>
        public float AudioVolume;
    }
}
