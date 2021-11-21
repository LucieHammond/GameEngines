using System.IO;
using System.Xml.Serialization;

namespace GameEngine.Core.Serialization.Text
{
    /// <summary>
    /// An ObjectSerializer providing serialization procedures into and from the XML format
    /// </summary>
    public class XmlObjectSerializer : ObjectSerializer
    {
        /// <summary>
        /// Serialize the specified object into a XML string
        /// </summary>
        /// <typeparam name="T">The type of the given object</typeparam>
        /// <param name="objectValue">The object to serialize</param>
        /// <returns>A XML structured string representing the given object</returns>
        public override string Serialize<T>(T objectValue)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, objectValue);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Deserialize the specified XML string into an instance of type T
        /// </summary>
        /// <typeparam name="T">The type of the object to create</typeparam>
        /// <param name="objectData">The XML string to deserialize</param>
        /// <returns>An object of type T corresponding to the given XML string</returns>
        public override T Deserialize<T>(string objectData)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(objectData))
            {
                return (T) serializer.Deserialize(stringReader);
            }
        }
    }
}
