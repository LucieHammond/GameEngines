﻿using GameEngine.Core.Unity.System;
using UnityEngine;
using UnityEngine.UI;

namespace GameEngine.Core.Unity.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure an UI image
    /// </summary>
    [CreateAssetMenu(fileName = "NewImageDescriptor", menuName = "Content/Unity Objects/Image Descriptor", order = 20)]
    public class ImageDescriptor : ContentDescriptor
    {
        /// <summary>
        /// The source image data (registered as sprite) that is used to render the image object
        /// </summary>
        public Sprite SourceImage;

        /// <summary>
        /// The base color to apply to the image
        /// </summary>
        public Color Color;

        /// <summary>
        /// The material that will be used to render the image, if specified
        /// </summary>
        public Material Material;

        /// <summary>
        /// A type defining how to display the image
        /// </summary>
        public Image.Type ImageType;

        /// <summary>
        /// Whether the image should be displayed using the mesh generated by the TextureImporter, or by a simple quad mesh
        /// </summary>
        public bool UseSpriteMesh;

        /// <summary>
        /// Whether the image should preserve its sprite aspect ratio
        /// </summary>
        public bool PreserveAspect;

        /// <summary>
        /// What type of fill method to use
        /// </summary>
        public Image.FillMethod FillMethod;

        /// <summary>
        /// Controls the origin point of the fill process. Value means different things with each fill method
        /// </summary>
        public int FillOrigin;

        /// <summary>
        /// Amount of the image shown when the ImageType is set to Filled
        /// </summary>
        public float FillAmount;

        /// <summary>
        /// Whether the image should be filled clockwise (true) or counter-clockwise (false)
        /// </summary>
        public bool FillClockwise;
    }
}
