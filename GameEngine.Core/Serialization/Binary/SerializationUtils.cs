using System;
using System.Text;

namespace GameEngine.Core.Serialization.Binary
{
    /// <summary>
    /// An utility class regrouping useful methods for serialization operations into binaries
    /// </summary>
    public static class SerializationUtils
    {
        private const string VARINT_OUT_OF_RANGE_MESSAGE = "Variable-length integer is out of range [0, 1073741823]";

        #region read methods
        /// <summary>
        /// Read a boolean from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The boolean that has been read</returns>
        public static bool ReadBool(byte[] data, ref int cursor) => Read(data, ref cursor, 1) != 0;

        /// <summary>
        /// Read a byte from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The byte that has been read</returns>
        public static byte ReadByte(byte[] data, ref int cursor) => (byte)Read(data, ref cursor, 1);

        /// <summary>
        /// Read a signed byte from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The signed byte that has been read</returns>
        public static sbyte ReadSByte(byte[] data, ref int cursor) => (sbyte)Read(data, ref cursor, 1);

        /// <summary>
        /// Read a short integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The short integer that has been read</returns>
        public static short ReadShort(byte[] data, ref int cursor) => (short)Read(data, ref cursor, 2);

        /// <summary>
        /// Read an unsigned short integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The unsigned short integer that has been read</returns>
        public static ushort ReadUShort(byte[] data, ref int cursor) => (ushort)Read(data, ref cursor, 2);

        /// <summary>
        /// Read an integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The integer that has been read</returns>
        public static int ReadInt(byte[] data, ref int cursor) => (int)Read(data, ref cursor, 4);

        /// <summary>
        /// Read an unsigned integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The unsigned integer that has been read</returns>
        public static uint ReadUInt(byte[] data, ref int cursor) => (uint)Read(data, ref cursor, 4);

        /// <summary>
        /// Read a long integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The long integer that has been read</returns>
        public static long ReadLong(byte[] data, ref int cursor) => (long)Read(data, ref cursor, 8);

        /// <summary>
        /// Read an unsigned long integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The unsigned long integer that has been read</returns>
        public static ulong ReadULong(byte[] data, ref int cursor) => (ulong)Read(data, ref cursor, 8);

        /// <summary>
        /// Read a float from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The float that has been read</returns>
        public static float ReadFloat(byte[] data, ref int cursor)
        {
            return BitConverter.ToSingle(GetBytes(data, ref cursor, 4), 0);
        }

        /// <summary>
        /// Read a double from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The double that has been read</returns>
        public static double ReadDouble(byte[] data, ref int cursor)
        {
            return BitConverter.ToDouble(GetBytes(data, ref cursor, 8), 0);
        }

        /// <summary>
        /// Read a character from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The character that has been read</returns>
        public static char ReadChar(byte[] data, ref int cursor) => (char)Read(data, ref cursor, 2);

        /// <summary>
        /// Read a variable-length integer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <returns>The variable-length integer that has been read</returns>
        public static int ReadVarInt(byte[] data, ref int cursor)
        {
            byte firstByte = data[cursor];
            int nbBytes = 1 + ((firstByte & 0xc0) >> 6);

            int value = (firstByte & 0x3f);
            for (int i = 1; i < nbBytes; i++)
            {
                value <<= 8;
                value |= data[cursor + i];
            }

            cursor += nbBytes;
            return value;
        }

        /// <summary>
        /// Read an ASCII string from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <param name="byteLength">The number of bytes to read. If not set or negative, it will be decoded from the byte array</param>
        /// <returns>The ASCII string that has been read</returns>
        public static string ReadStringAscii(byte[] data, ref int cursor, int byteLength = -1)
        {
            if (byteLength < 0)
                byteLength = ReadVarInt(data, ref cursor);

            string value = Encoding.ASCII.GetString(data, cursor, byteLength);
            cursor += byteLength;
            return value;
        }

        /// <summary>
        /// Deserialize a string from a byte array, using ASCII encoding
        /// </summary>
        /// <param name="data">The byte array encoding the string</param>
        /// <returns>The ASCII string that has been decoded</returns>
        public static string BytesToStringAscii(this byte[] data) => Encoding.ASCII.GetString(data);

        /// <summary>
        /// Read an UTF-8 string from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <param name="byteLength">The number of bytes to read. If not set or negative, it will be decoded from the byte array</param>
        /// <returns>The UTF-8 string that has been read</returns>
        public static string ReadStringUtf8(byte[] data, ref int cursor, int byteLength = -1)
        {
            if (byteLength < 0)
                byteLength = ReadVarInt(data, ref cursor);

            string value = Encoding.UTF8.GetString(data, cursor, byteLength);
            cursor += byteLength;
            return value;
        }

        /// <summary>
        /// Deserialize a string from a byte array, using UTF-8 encoding
        /// </summary>
        /// <param name="data">The byte array encoding the string</param>
        /// <returns>The UTF-8 string that has been decoded</returns>
        public static string BytesToStringUtf8(this byte[] data) => Encoding.UTF8.GetString(data);

        /// <summary>
        /// Extract a byte buffer from a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <param name="cursor">The cursor indicating the reading index (to update)</param>
        /// <param name="length">The length of the buffer. If not set or negative, it will be decoded from the byte array</param>
        /// <returns>The byte buffer that has been read</returns>
        public static byte[] ReadBuffer(byte[] data, ref int cursor, int length = -1)
        {
            if (length < 0)
                length = ReadVarInt(data, ref cursor);

            return GetBytes(data, ref cursor, length);
        }
        #endregion

        #region write methods
        /// <summary>
        /// Write a boolean to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The boolean to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteBool(byte[] data, bool value, ref int cursor) => Write(data, value ? 1 : 0, ref cursor, 1);

        /// <summary>
        /// Write a byte to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The byte to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteByte(byte[] data, byte value, ref int cursor) => Write(data, (long)value, ref cursor, 1);

        /// <summary>
        /// Write a signed byte to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The signed byte to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteSByte(byte[] data, sbyte value, ref int cursor) => Write(data, (long)value, ref cursor, 1);

        /// <summary>
        /// Write a short integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The short integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteShort(byte[] data, short value, ref int cursor) => Write(data, (long)value, ref cursor, 2);

        /// <summary>
        /// Write an unsigned short integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The unsigned short integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteUShort(byte[] data, ushort value, ref int cursor) => Write(data, (long)value, ref cursor, 2);

        /// <summary>
        /// Write an integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteInt(byte[] data, int value, ref int cursor) => Write(data, (long)value, ref cursor, 4);

        /// <summary>
        /// Write an unsigned integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The unsigned integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteUInt(byte[] data, uint value, ref int cursor) => Write(data, (long)value, ref cursor, 4);

        /// <summary>
        /// Write a long integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The long integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteLong(byte[] data, long value, ref int cursor) => Write(data, (long)value, ref cursor, 8);

        /// <summary>
        /// Write an unsigned long integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The unsigned long integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteULong(byte[] data, ulong value, ref int cursor) => Write(data, (long)value, ref cursor, 8);

        /// <summary>
        /// Write a float to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The float to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteFloat(byte[] data, float value, ref int cursor)
        {
            SetBytes(data, BitConverter.GetBytes(value), ref cursor, 4);
        }

        /// <summary>
        /// Write a double to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The double to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteDouble(byte[] data, double value, ref int cursor)
        {
            SetBytes(data, BitConverter.GetBytes(value), ref cursor, 8);
        }

        /// <summary>
        /// Write a character to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The character to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteChar(byte[] data, char value, ref int cursor) => Write(data, (long)value, ref cursor, 2);

        /// <summary>
        /// Write a variable-length integer to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The variable-length integer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        public static void WriteVarInt(byte[] data, int value, ref int cursor)
        {
            int nbBytes = (value & 0xffffffc0) == 0 ? 1 :
                (value & 0xffffc000) == 0 ? 2 :
                (value & 0xffc00000) == 0 ? 3 :
                (value & 0xc0000000) == 0 ? 4 :
                throw new ArgumentOutOfRangeException(nameof(value), VARINT_OUT_OF_RANGE_MESSAGE);

            for (int i = nbBytes - 1; i >= 1; i--)
            {
                data[cursor + i] = (byte)(value & 0xff);
                value >>= 8;
            }
            data[cursor] = (byte)((value & 0x3f) | ((nbBytes - 1) << 6));

            cursor += nbBytes;
        }

        /// <summary>
        /// Write an ASCII string to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The ASCII string to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        /// <param name="encodeLength">Whether to encode the corresponding byte count with the string or not</param>
        public static void WriteStringAscii(byte[] data, string value, ref int cursor, bool encodeLength = true)
        {
            int byteLength = value.Length;

            if (encodeLength)
                WriteVarInt(data, byteLength, ref cursor);

            Encoding.ASCII.GetBytes(value, 0, value.Length, data, cursor);
            cursor += byteLength;
        }

        /// <summary>
        /// Serialize a string into a byte array, using ASCII encoding
        /// </summary>
        /// <param name="value">The ASCII string to encode</param>
        /// <returns>The byte array encoding the string</returns>
        public static byte[] BytesFromStringAscii(this string value) => Encoding.ASCII.GetBytes(value);

        /// <summary>
        /// Write an UTF-8 string to a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="value">The UTF-8 string to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        /// <param name="encodeLength">Whether to encode the corresponding byte count with the string or not</param>
        public static void WriteStringUtf8(byte[] data, string value, ref int cursor, bool encodeLength = true)
        {
            int byteLength = Encoding.UTF8.GetByteCount(value);

            if (encodeLength)
                WriteVarInt(data, byteLength, ref cursor);

            Encoding.UTF8.GetBytes(value, 0, value.Length, data, cursor);
            cursor += byteLength;
        }

        /// <summary>
        /// Serialize a string into a byte array, using UTF-8 encoding
        /// </summary>
        /// <param name="value">The UTF-8 string to encode</param>
        /// <returns>The byte array encoding the string</returns>
        public static byte[] BytesFromStringUtf8(this string value) => Encoding.UTF8.GetBytes(value);

        /// <summary>
        /// Insert a byte buffer into a byte array, starting at cursor
        /// </summary>
        /// <param name="data">The byte array in which to write</param>
        /// <param name="buffer">The byte buffer to write</param>
        /// <param name="cursor">The cursor indicating the writing index (to update)</param>
        /// <param name="encodeLength">Whether to encode the buffer length with the buffer or not</param>
        public static void WriteBuffer(byte[] data, byte[] buffer, ref int cursor, bool encodeLength = true)
        {
            if (encodeLength)
                WriteVarInt(data, buffer.Length, ref cursor);

            SetBytes(data, buffer, ref cursor, buffer.Length);
        }
        #endregion

        #region private
        private static long Read(byte[] data, ref int cursor, int length)
        {
            long value = 0;
            for (int i = 0; i < length; i++)
            {
                value <<= 8;
                value |= data[cursor + i];
            }
            cursor += length;
            return value;
        }

        private static void Write(byte[] data, long value, ref int cursor, int length)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                data[cursor + i] = (byte)(value & 0xff);
                value >>= 8;
            }
            cursor += length;
        }

        private static byte[] GetBytes(byte[] data, ref int cursor, int length)
        {
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(data, cursor, buffer, 0, length);
            cursor += length;
            return buffer;
        }

        private static void SetBytes(byte[] data, byte[] buffer, ref int cursor, int length)
        {
            Buffer.BlockCopy(buffer, 0, data, cursor, length);
            cursor += length;
        }
        #endregion
    }
}
