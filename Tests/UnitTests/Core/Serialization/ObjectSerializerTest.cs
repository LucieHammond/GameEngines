using GameEngine.Core.Serialization.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the serializers that inherit from the ObjectSerializer abtract class
    /// <see cref="ObjectSerializer"/>
    /// </summary>
    [TestClass]
    public abstract class ObjectSerializerTest
    {
        protected ObjectSerializer m_Serializer;

        protected abstract string GetFormattedString(TestObject testObject);

        protected abstract bool IsValid(string formattedData);

        [TestMethod]
        public void SerializeTest()
        {
            // Create the object to serialize
            TestObject objectValue = CreateTestObject();

            // Serialize object
            string serializeResult = m_Serializer.Serialize(objectValue);

            // Check result
            Assert.IsTrue(IsValid(serializeResult));
            Assert.IsTrue(ContainObjectData(serializeResult));
            Assert.IsTrue(AreEquals(objectValue, m_Serializer.Deserialize<TestObject>(serializeResult)));
        }

        [TestMethod]
        public void DeserializeTest()
        {
            // Create the object to deserialize
            TestObject objectValue = CreateTestObject();
            string objectData = GetFormattedString(objectValue);

            // Deserialize object
            TestObject deserializeResult = m_Serializer.Deserialize<TestObject>(objectData);

            // Check result
            Assert.IsNotNull(deserializeResult);
            Assert.IsTrue(AreEquals(objectValue, deserializeResult));
        }

        private TestObject CreateTestObject()
        {
            return new TestObject()
            {
                IntValue = 41,
                FloatValue = 2.36f,
                BoolValue = true,
                StringValue = "String",
                DateTimeValue = new DateTime(2020, 12, 28),
                ArrayValue = new short[] { 1, 2, 3 },
                ObjectValue = new SubObject() { A = "a", B = "b" }
            };
        }

        private bool AreEquals(TestObject objectRef, TestObject objectResult)
        {
            if (objectResult == null)
                return false;

            return objectRef.IntValue == objectResult.IntValue
                && objectRef.FloatValue == objectResult.FloatValue
                && objectRef.BoolValue == objectResult.BoolValue
                && objectRef.StringValue == objectResult.StringValue
                && objectRef.DateTimeValue == objectResult.DateTimeValue
                && Enumerable.SequenceEqual(objectRef.ArrayValue, objectResult.ArrayValue)
                && objectRef.ObjectValue.A == objectResult.ObjectValue.A
                && objectRef.ObjectValue.B == objectResult.ObjectValue.B;
        }

        private bool ContainObjectData(string formattedData)
        {
            if (formattedData == null)
                return false;

            foreach (FieldInfo field in typeof(TestObject).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!formattedData.Contains(field.Name))
                    return false;
            }

            return true;
        }
    }

    public class TestObject
    {
        public int IntValue;

        public float FloatValue;

        public bool BoolValue;

        public string StringValue;

        public DateTime DateTimeValue;

        public short[] ArrayValue;

        public SubObject ObjectValue;
    }

    public class SubObject
    {
        public string A;

        public string B;
    }
}
