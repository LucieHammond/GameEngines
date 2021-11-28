using System;
using System.Text;

namespace GameEngine.Core.Serialization.Text
{
    /// <summary>
    /// Provides functionality to serialize and deserialize objects into specific text formats
    /// </summary>
    public abstract class ObjectSerializer
    {
        /// <summary>
        /// Type of character encoding to use when converting the textual serialization format to bytes
        /// </summary>
        public Encoding EncodingType = Encoding.UTF8;

        /// <summary>
        /// Create a kind of ObjectSerializer corresponding to the given serialization format
        /// </summary>
        /// <param name="format">The serialization format</param>
        /// <returns>An instance of a class derived from ObjectSerializer</returns>
        public static ObjectSerializer CreateSerializer(SerializerFormat format)
        {
            return CreateSerializer(format, Encoding.UTF8);
        }

        /// <summary>
        /// Create a kind of ObjectSerializer corresponding to the given serialization format
        /// </summary>
        /// <param name="format">The serialization format</param>
        /// <param name="encoding">The character encoding</param>
        /// <returns>An instance of a class derived from ObjectSerializer</returns>
        public static ObjectSerializer CreateSerializer(SerializerFormat format, Encoding encoding)
        {
            ObjectSerializer serializer;
            switch (format)
            {
                case SerializerFormat.Json:
                    serializer = new JsonObjectSerializer();
                    break;
                case SerializerFormat.Xml:
                    serializer = new XmlObjectSerializer();
                    break;
                case SerializerFormat.Csv:
                    throw new NotImplementedException();
                case SerializerFormat.Yaml:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException($"Unknown text format for serialization: {format}");
            }

            serializer.EncodingType = encoding;
            return serializer;
        }

        /// <summary>
        /// Serialize the specified object into an array of bytes encoding a structured text
        /// </summary>
        /// <typeparam name="T">The type of the given object</typeparam>
        /// <param name="objectValue">The object to serialize</param>
        /// <returns>An array of bytes representing the given object</returns>
        public byte[] SerializeToBytes<T>(T objectValue)
        {
            string data = Serialize<T>(objectValue);
            return EncodingType.GetBytes(data);
        }

        /// <summary>
        /// Deserialize into an instance of type T the specified array of bytes encoding a structured text
        /// </summary>
        /// <typeparam name="T">The type of the object to create</typeparam>
        /// <param name="objectData">The array of bytes to deserialize</param>
        /// <returns>An object of type T corresponding to the given array of bytes</returns>
        public T DeserializeFromBytes<T>(byte[] objectData)
        {
            string data = EncodingType.GetString(objectData);
            return Deserialize<T>(data);
        }

        /// <summary>
        /// Serialize the specified object into a structured string
        /// </summary>
        /// <typeparam name="T">The type of the given object</typeparam>
        /// <param name="objectValue">The object to serialize</param>
        /// <returns>A structured string representing the given object</returns>
        public abstract string Serialize<T>(T objectValue);

        /// <summary>
        /// Deserialize the specified structured string into an instance of type T
        /// </summary>
        /// <typeparam name="T">The type of the object to create</typeparam>
        /// <param name="objectData">The structured string to deserialize</param>
        /// <returns>An object of type T corresponding to the given structured string</returns>
        public abstract T Deserialize<T>(string objectData);
    }
}
