using System;
using System.Reflection;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// Base model to be derived when defining a certain type of game content
    /// This content takes the form of structured data stored in files
    /// </summary>
    [Serializable]
    public class ContentData
    {
        /// <summary>
        /// Id of the content data, by which it is referenced
        /// </summary>
        public string ContentId;

        /// <summary>
        /// Returns a string representing the content data
        /// </summary>
        /// <returns>A string representing the content data</returns>
        public override string ToString()
        {
            string description = "";
            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (description.Length > 0)
                    description += ", ";
                description += $"{field.Name}: {field.GetValue(this)}";
            }

            return $"{ContentId} ({GetType().Name}) -> {{{description}}}";
        }
    }
}
