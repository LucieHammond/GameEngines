using Newtonsoft.Json;

namespace GameEngine.Core.Serialization.Text
{
    /// <summary>
    /// An ObjectSerializer providing serialization procedures into and from the JSON format
    /// </summary>
    public class JsonObjectSerializer : ObjectSerializer
    {
        /// <summary>
        /// Serialize the specified object into a JSON string
        /// </summary>
        /// <typeparam name="T">The type of the given object</typeparam>
        /// <param name="objectValue">The object to serialize</param>
        /// <returns>A JSON structured string representing the given object</returns>
        public override string Serialize<T>(T objectValue)
        {
            return JsonConvert.SerializeObject(objectValue);
        }

        /// <summary>
        /// Deserialize the specified JSON string into an instance of type T
        /// </summary>
        /// <typeparam name="T">The type of the object to create</typeparam>
        /// <param name="objectData">The JSON string to deserialize</param>
        /// <returns>An object of type T corresponding to the given JSON string</returns>
        public override T Deserialize<T>(string objectData)
        {
            return JsonConvert.DeserializeObject<T>(objectData);
        }
    }
}
