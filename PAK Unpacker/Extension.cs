using System.Collections.Generic;
using System.IO;
namespace PAK_Unpacker
{
    public static class Extension
    {

        public static void SaveText(this string text, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(text);
            }
        }

        public static int ExtractInt32LE(this Stream st) => st.ExtractPiece(4).ExtractInt32LE();

        public static uint ExtractUInt32LE(this Stream st) => st.ExtractPiece(4).ExtractUInt32LE();

        public static int ExtractInt32BE(this Stream st) => st.ExtractPiece(4).ExtractInt32BE();

        public static uint ExtractUInt32BE(this Stream st) => st.ExtractPiece(4).ExtractUInt32BE();

        public static short ExtractInt16(this Stream st) => st.ExtractPiece(2).ExtractInt16();

        public static ushort ExtractUInt16(this Stream st) => st.ExtractPiece(2).ExtractUInt16();


        public static int ExtractInt32LE(this byte[] bytes, int index = 0)
        {
            return (bytes[index + 3] << 24) | (bytes[index + 2] << 16) | (bytes[index + 1] << 8) | bytes[index + 0];
        }

        public static uint ExtractUInt32LE(this byte[] bytes, int index = 0)
        {
            return (uint)ExtractInt32LE(bytes, index);
        }

        public static uint ExtractUInt32BE(this byte[] bytes, int index = 0)
        {
            return (uint)ExtractInt32BE(bytes, index);
        }

        public static int ExtractInt32BE(this byte[] bytes, int index = 0)
        {
            return ((bytes[index + 0] << 24) | (bytes[index + 1] << 16) | (bytes[index + 2] << 8) | bytes[index + 3]);
        }

        public static ushort ExtractUInt16(this byte[] bytes, int index = 0)
        {
            return (ushort)ExtractUInt16(bytes, index);
        }

        public static short ExtractInt16(this byte[] bytes, int index = 0)
        {
            return (short)((bytes[index + 1] << 8) | bytes[index + 0]);
        }

        public static byte[] ExtractPiece(this Stream ms, int length)
        {
            byte[] data = new byte[length];
            ms.Read(data, 0, length);

            return data;
        }

        public static byte[] ExtractSector(this Stream ms, int offset, int length)
        {
            ms.Position = offset + 0x18;

            List<byte> dt = new List<byte>();

            for (int i = 0; i < length / 0x800; i++)
            {
                byte[] data = new byte[0x800];
                ms.Read(data, 0, 0x800);
                dt.AddRange(data);

                ms.Position += 0x130;
            }
            return dt.ToArray();
        }

        public static void Save(this byte[] data, string path, int offset = -1, int length = -1)
        {
            int _offset = (offset > -1) ? offset : 0;
            int _length = (length > -1) ? length : data.Length;

            using (FileStream fs = File.Create(path))
            {
                fs.Write(data, _offset, _length);
            }
        }

        public static byte[] Int32ToByteArray(this int value)
        {
            byte[] result = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return result;
        }

        public static byte[] UInt32ToByteArrayBE(this uint value)
        {
            byte[] result = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)((value >> (4 - i) * 8) & 0xFF);
            }
            return result;
        }

        public static byte[] Uint32ToByteArray(this uint value)
        {
            return ((int)value).Int32ToByteArray();
        }

        public static byte[] Int16ToByteArray(this ushort value)
        {
            return ((short)value).Int16ToByteArray();
        }

        public static byte[] Int16ToByteArray(this short value)
        {
            byte[] result = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                result[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return result;
        }

        public static byte[] CopyFrom(this byte[] self, byte[] data, int copyOffset, int length, int destinyOffset = 0)
        {

            for (int i = copyOffset; i < length; i++)
            {
                self[destinyOffset + (i - copyOffset)] = data[i];
            }

            return self;
        }

    }
}
