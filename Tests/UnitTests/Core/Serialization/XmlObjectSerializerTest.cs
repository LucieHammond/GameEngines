using GameEngine.Core.Serialization.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the class JsonObjectSerializer
    /// <see cref="XmlObjectSerializer"/>
    /// </summary>
    [TestClass]
    public class XmlObjectSerializerTest : ObjectSerializerTest
    {
        public XmlObjectSerializerTest()
        {
            m_Serializer = new XmlObjectSerializer();
        }

        protected override string GetFormattedString(TestObject testObject)
        {
            return @$"<?xml version=""1.0""?>
                <TestObject>
                    <IntValue>{testObject.IntValue.ToString(CultureInfo.InvariantCulture)}</IntValue>
                    <FloatValue>{testObject.FloatValue.ToString(CultureInfo.InvariantCulture)}</FloatValue>
                    <BoolValue>{(testObject.BoolValue ? "true" : "false")}</BoolValue>
                    <StringValue>{testObject.StringValue.ToString(CultureInfo.InvariantCulture)}</StringValue>
                    <DateTimeValue>{testObject.DateTimeValue.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}</DateTimeValue>
                    <ArrayValue>
                        {string.Join('\n', testObject.ArrayValue.Select((value) => $"<short>{value}</short>"))}
                    </ArrayValue>
                    <ObjectValue>
                        <A>{testObject.ObjectValue.A}</A>
                        <B>{testObject.ObjectValue.B}</B>
                    </ObjectValue>
                </TestObject>";
        }

        protected override bool IsValid(string formattedData)
        {
            try
            {
                new XmlDocument().LoadXml(formattedData);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
