namespace GameEngine.Core.Serialization.Text
{
    /// <summary>
    /// Different types of textual formats used for serialization
    /// </summary>
    public enum SerializerFormat
    {
        /// <summary>
        /// JSON format (JavaScript Objet Notation)
        /// </summary>
        Json,

        /// <summary>
        /// XML format (Extensible Markup Language)
        /// </summary>
        Xml,

        /// <summary>
        /// CSV format (Comma Sseparated Values)
        /// </summary>
        Csv,

        /// <summary>
        /// YAML format (Yet Another Markup Language)
        /// </summary>
        Yaml
    }
}
