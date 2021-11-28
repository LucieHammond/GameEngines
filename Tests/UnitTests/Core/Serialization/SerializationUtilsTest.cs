using GameEngine.Core.Serialization.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the class SerializationUtils
    /// <see cref="SerializationUtils"/>
    /// </summary>
    [TestClass]
    public class SerializationUtilsTest
    {
        private const int BASE_DATA_SIZE = 128;
        private byte[] m_Data;
        private int m_Cursor;

        public SerializationUtilsTest()
        {
            m_Data = new byte[BASE_DATA_SIZE];
            m_Cursor = 0;
        }

        [TestMethod]
        public void SerializeBoolTest()
        {
            Action<bool> write = (value) => SerializationUtils.WriteBool(m_Data, value, ref m_Cursor);
            Func<bool> read = () => SerializationUtils.ReadBool(m_Data, ref m_Cursor);

            bool[] testValues = new bool[2] { false, true };
            TestSerializationForBaseValues(write, read, 1, testValues);
        }

        [TestMethod]
        public void SerializeByteTest()
        {
            Action<byte> write = (value) => SerializationUtils.WriteByte(m_Data, value, ref m_Cursor);
            Func<byte> read = () => SerializationUtils.ReadByte(m_Data, ref m_Cursor);

            byte[] testValues = new byte[7] { byte.MinValue, 1, 5, 15, 40, 101, byte.MaxValue };
            TestSerializationForBaseValues(write, read, 1, testValues);
        }

        [TestMethod]
        public void SerializeSByteTest()
        {
            Action<sbyte> write = (value) => SerializationUtils.WriteSByte(m_Data, value, ref m_Cursor);
            Func<sbyte> read = () => SerializationUtils.ReadSByte(m_Data, ref m_Cursor);

            sbyte[] testValues = new sbyte[7] { sbyte.MinValue, -20, -3, 0, 7, 50, sbyte.MaxValue };
            TestSerializationForBaseValues(write, read, 1, testValues);
        }

        [TestMethod]
        public void SerializeShortTest()
        {
            Action<short> write = (value) => SerializationUtils.WriteShort(m_Data, value, ref m_Cursor);
            Func<short> read = () => SerializationUtils.ReadShort(m_Data, ref m_Cursor);

            short[] testValues = new short[9] { short.MinValue, -2048, -128, -8, 0, 31, 511, 8191, short.MaxValue };
            TestSerializationForBaseValues(write, read, 2, testValues);
        }

        [TestMethod]
        public void SerializeUShortTest()
        {
            Action<ushort> write = (value) => SerializationUtils.WriteUShort(m_Data, value, ref m_Cursor);
            Func<ushort> read = () => SerializationUtils.ReadUShort(m_Data, ref m_Cursor);

            ushort[] testValues = new ushort[9] { ushort.MinValue, 1, 15, 63, 255, 1023, 4095, 16383, ushort.MaxValue };
            TestSerializationForBaseValues(write, read, 2, testValues);
        }

        [TestMethod]
        public void SerializeIntTest()
        {
            Action<int> write = (value) => SerializationUtils.WriteInt(m_Data, value, ref m_Cursor);
            Func<int> read = () => SerializationUtils.ReadInt(m_Data, ref m_Cursor);

            int[] testValues = new int[11] { int.MinValue, -25482645, -302384, -3588, -43, 0, 390, 32938, 2775886, 233930680, int.MaxValue };
            TestSerializationForBaseValues(write, read, 4, testValues);
        }

        [TestMethod]
        public void SerializeUIntTest()
        {
            Action<uint> write = (value) => SerializationUtils.WriteUInt(m_Data, value, ref m_Cursor);
            Func<uint> read = () => SerializationUtils.ReadUInt(m_Data, ref m_Cursor);

            uint[] testValues = new uint[11] { uint.MinValue, 1, 84, 781, 7175, 65878, 604767, 5551773, 50965289, 467861360, uint.MaxValue };
            TestSerializationForBaseValues(write, read, 4, testValues);
        }

        [TestMethod]
        public void SerializeLongTest()
        {
            Action<long> write = (value) => SerializationUtils.WriteLong(m_Data, value, ref m_Cursor);
            Func<long> read = () => SerializationUtils.ReadLong(m_Data, ref m_Cursor);

            long[] testValues = new long[13] { long.MinValue, -5676284186327223, -3493321318408, -2149873656, -1323084, -814,
                0, 32822, 53333506, 86661407055, 140815782345006, 228811015550850313, long.MaxValue };
            TestSerializationForBaseValues(write, read, 8, testValues);
        }

        [TestMethod]
        public void SerializeULongTest()
        {
            Action<ulong> write = (value) => SerializationUtils.WriteULong(m_Data, value, ref m_Cursor);
            Func<ulong> read = () => SerializationUtils.ReadULong(m_Data, ref m_Cursor);

            ulong[] testValues = new ulong[13] { ulong.MinValue, 1, 1628, 65644, 2646167, 106667012, 4299747310, 173322814110,
                6986642636814, 281631564690012, 11352568372654451, 457622031101700613, ulong.MaxValue };
            TestSerializationForBaseValues(write, read, 8, testValues);
        }

        [TestMethod]
        public void SerializeFloatTest()
        {
            Action<float> write = (value) => SerializationUtils.WriteFloat(m_Data, value, ref m_Cursor);
            Func<float> read = () => SerializationUtils.ReadFloat(m_Data, ref m_Cursor);

            float[] testValues = new float[11] { 0.0f, 1.0f, 4.8f, -17.76f, 915.086f, 8223.436f, -67908.6428f,
                float.MinValue, float.MaxValue, float.Epsilon, -float.Epsilon };
            TestSerializationForBaseValues(write, read, 4, testValues);
        }

        [TestMethod]
        public void SerializeDoubleTest()
        {
            Action<double> write = (value) => SerializationUtils.WriteDouble(m_Data, value, ref m_Cursor);
            Func<double> read = () => SerializationUtils.ReadDouble(m_Data, ref m_Cursor);

            double[] testValues = new double[13] { 0.0d, 1.0d, -2.5d, 48.47d, 866.167d, 9220.5660d, -38249.88494d,
                37793018.0d, -0.51322687d, double.MinValue, double.MaxValue, double.Epsilon, -double.Epsilon };
            TestSerializationForBaseValues(write, read, 8, testValues);
        }

        [TestMethod]
        public void SerializeCharTest()
        {
            Action<char> write = (value) => SerializationUtils.WriteChar(m_Data, value, ref m_Cursor);
            Func<char> read = () => SerializationUtils.ReadChar(m_Data, ref m_Cursor);

            char[] testValues = new char[9] { char.MinValue, '\x0001', '!', 'A', 'a', '~', '\x00ff', '\x0fff', char.MaxValue };
            TestSerializationForBaseValues(write, read, 2, testValues);
        }

        [TestMethod]
        public void SerializeVarIntTest()
        {
            Action<int> write = (value) => SerializationUtils.WriteVarInt(m_Data, value, ref m_Cursor);
            Func<int> read = () => SerializationUtils.ReadVarInt(m_Data, ref m_Cursor);

            // If value is in [0, 63] -> the serialization uses 1 byte
            int[] values1Byte = new int[5] { 0, 1, 4, 16, 63 };
            TestSerializationForBaseValues(write, read, 1, values1Byte);

            // If value is in [64, 16383] -> the serialization uses 2 bytes
            int[] values2Bytes = new int[5] { 64, 255, 1023, 4095, 16383 };
            TestSerializationForBaseValues(write, read, 2, values2Bytes);

            // If value is in [16384, 4194303] -> the serialization uses 3 bytes
            int[] values3Bytes = new int[5] { 16384, 65535, 262143, 1048575, 4194303 };
            TestSerializationForBaseValues(write, read, 3, values3Bytes);

            // If value is in [4194304, 1073741823] -> the serialization uses 4 bytes
            int[] values4Bytes = new int[5] { 4194304, 16777215, 67108863, 268435455, 1073741823 };
            TestSerializationForBaseValues(write, read, 4, values4Bytes);

            // If value < 0 or value >= 1073741824 -> WriteVarInt throws ArgumentOutOfRangeException
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write(-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write(1073741824));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => write(int.MaxValue));
        }

        [TestMethod]
        public void SerializeStringAsciiTest()
        {
            Action<string, bool> write =
                (value, encodeLength) => SerializationUtils.WriteStringAscii(m_Data, value, ref m_Cursor, encodeLength);
            Func<int, string> read = (length) => SerializationUtils.ReadStringAscii(m_Data, ref m_Cursor, length);
            Func<string, int> expectedLength = (value) => value.Length;

            string[] testValues = new string[7] { string.Empty, "1", "abc", "example", "This is a test string !",
                @"{ ""path"": ""C:\Directory\"", ""numbers"": [0, 42] }", new string('z', 200)};

            // Without length encoding
            TestSerializationForStructuredValues<string, char>(write, read, expectedLength, testValues, false);

            // With length encoding
            TestSerializationForStructuredValues<string, char>(write, read, expectedLength, testValues, true);

            // For basic conversion
            foreach (string value in testValues)
                Assert.AreEqual(value, value.BytesFromStringAscii().BytesToStringAscii());
        }

        [TestMethod]
        public void SerializeStringUtf8Test()
        {
            Action<string, bool> write =
                (value, encodeLength) => SerializationUtils.WriteStringUtf8(m_Data, value, ref m_Cursor, encodeLength);
            Func<int, string> read = (length) => SerializationUtils.ReadStringUtf8(m_Data, ref m_Cursor, length);
            Func<string, int> expectedLength = (value) => Encoding.UTF8.GetByteCount(value);

            string[] testValues = new string[10] { string.Empty, "1", "abc", "example", "This is a test string !",
                @"{ ""path"": ""C:\Directory\"", ""numbers"": [0, 42] }", new string('z', 200),
                "àéèïòùçµ€£", "Là, une chaine écrite en français", "\x0000\x000f\x00ff\x0fff\xffff"};

            // Without length encoding
            TestSerializationForStructuredValues<string, char>(write, read, expectedLength, testValues, false);

            // With length encoding
            TestSerializationForStructuredValues<string, char>(write, read, expectedLength, testValues, true);

            // For basic conversion
            foreach (string value in testValues)
                Assert.AreEqual(value, value.BytesFromStringUtf8().BytesToStringUtf8());
        }

        [TestMethod]
        public void SerializeBufferTest()
        {
            Action<byte[], bool> write =
                (value, encodeLength) => SerializationUtils.WriteBuffer(m_Data, value, ref m_Cursor, encodeLength);
            Func<int, byte[]> read = (length) => SerializationUtils.ReadBuffer(m_Data, ref m_Cursor, length);
            Func<byte[], int> expectedLength = (value) => value.Length;

            byte[][] testValues = new byte[2][] { Array.Empty<byte>(), new byte[3] { 2, 22, 222 } };

            // Without length encoding
            TestSerializationForStructuredValues<byte[], byte>(write, read, expectedLength, testValues, false);

            // With length encoding
            TestSerializationForStructuredValues<byte[], byte>(write, read, expectedLength, testValues, true);
        }

        private void TestSerializationForBaseValues<T>(Action<T> write, Func<T> read, int expectedLength, T[] values)
        {
            // Write all values
            m_Cursor = 0;
            foreach (T value in values)
            {
                int start = m_Cursor;
                write(value);
                Assert.AreEqual(expectedLength, m_Cursor - start);
            }

            // Read all values
            m_Cursor = 0;
            foreach (T value in values)
            {
                int start = m_Cursor;
                T resultValue = read();
                Assert.AreEqual(expectedLength, m_Cursor - start);
                Assert.AreEqual(value, resultValue);
            }

            Array.Clear(m_Data, 0, m_Data.Length);
        }

        private void TestSerializationForStructuredValues<T, U>(
            Action<T, bool> write, Func<int, T> read, Func<T, int> expectedLength, T[] values, bool encodeLength)
            where T : IEnumerable<U>
        {
            // Write all values
            m_Cursor = 0;
            foreach (T value in values)
            {
                int length = expectedLength(value);
                int maxLength = length + (encodeLength ? 4 : 0);
                ResizeDataIfNecessary(maxLength);

                int start = m_Cursor;
                write(value, encodeLength);
                Assert.IsTrue(length <= m_Cursor - start && maxLength >= m_Cursor - start);
            }

            // Read all values
            m_Cursor = 0;
            foreach (T value in values)
            {
                int length = expectedLength(value);
                int maxLength = length + (encodeLength ? 4 : 0);

                int start = m_Cursor;
                T resultValue = read(encodeLength ? -1 : length);
                Assert.IsTrue(length <= m_Cursor - start && maxLength >= m_Cursor - start);
                Assert.IsTrue(resultValue.SequenceEqual(value));
            }

            Array.Clear(m_Data, 0, m_Data.Length);
        }

        private void ResizeDataIfNecessary(int size)
        {
            if (m_Data.Length - m_Cursor < size)
            {
                int newSize = BASE_DATA_SIZE * (1 + ((m_Cursor + size) / BASE_DATA_SIZE));
                Array.Resize(ref m_Data, newSize);
            }
        }
    }
}
