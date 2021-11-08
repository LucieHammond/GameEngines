using UnityEngine;

namespace GameEngine.Core.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a UI text
    /// </summary>
    [CreateAssetMenu(fileName = "NewTextDescriptor", menuName = "Content Descriptors/Text Descriptor", order = 11)]
    public class TextDescriptor : ScriptableObject
    {
        /// <summary>
        /// The string value the text object should display
        /// </summary>
        public string Text;

        /// <summary>
        /// The base color of the text
        /// </summary>
        public Color Color;

        /// <summary>
        /// The material that will be used to render the text, if specified
        /// </summary>
        public Material Material;

        /// <summary>
        /// The font used by the text
        /// </summary>
        public Font Font;

        /// <summary>
        /// The font style used by the text
        /// </summary>
        public FontStyle FontStyle;

        /// <summary>
        /// The size that the dont should render at
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Line spacing, specified as a factor of font line height (1 for normal line spacing)
        /// </summary>
        public float LineSpacing;

        /// <summary>
        /// Whether this Text will support rich text in markup format
        /// </summary>
        public bool SupportRichText;

        /// <summary>
        /// The positioning of the text reliative to its RectTransform
        /// </summary>
        public TextAnchor Alignment;

        /// <summary>
        /// Use the extents of glyph geometry to perform horizontal alignment rather than glyph metrics
        /// </summary>
        public bool AlignByGeometry;

        /// <summary>
        /// Horizontal overflow mode
        /// </summary>
        public HorizontalWrapMode HorizontalOverflow;

        /// <summary>
        /// Vertical overflow mode
        /// </summary>
        public VerticalWrapMode VerticalOverflow;

        /// <summary>
        /// Should the text be allowed to auto resized
        /// </summary>
        public bool ResizeForBestFit;

        /// <summary>
        /// The minimum size the text is allowed to be
        /// </summary>
        public int ResizeMinSize;

        /// <summary>
        /// The maximum size the text is allowed to be
        /// </summary>
        public int ResizeMaxSize;
    }
}
