using GameEngine.Core.Serialization.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the class JsonObjectSerializer
    /// <see cref="JsonObjectSerializer"/>
    /// </summary>
    [TestClass]
    public class JsonObjectSerializerTest : ObjectSerializerTest
    {
        public JsonObjectSerializerTest()
        {
            m_Serializer = new JsonObjectSerializer();
        }

        protected override string GetFormattedString(TestObject testObject)
        {
            return $@"{{
                'IntValue': {testObject.IntValue.ToString(CultureInfo.InvariantCulture)},
                'FloatValue': {testObject.FloatValue.ToString(CultureInfo.InvariantCulture)},
                'BoolValue': {(testObject.BoolValue ? "true" : "false")},
                'StringValue': '{testObject.StringValue.ToString(CultureInfo.InvariantCulture)}',
                'DateTimeValue': '{testObject.DateTimeValue.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}',
                'ArrayValue': [{string.Join(',', testObject.ArrayValue)}],
                'ObjectValue':{{
                    'A': '{testObject.ObjectValue.A}',
                    'B': '{testObject.ObjectValue.B}'}}
                }}";
        }

        protected override bool IsValid(string formattedData)
        {
            try
            {
                JToken.Parse(formattedData);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
    }
}
