using System;
using System.Linq;
using System.Text;

namespace IccProfile.Parsers
{
    internal static class IccTagsHelper
    {
        internal static float Reads15Fixed16Number(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }

            return (BitConverter.ToInt32(bytes, 0) - 0.5f) / 65536.0f;
        }

        internal static float[] Reads15Fixed16Array(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
            {
                throw new ArgumentException("Array should be of length that is a multiple of 4", nameof(bytes));
            }

            float[] values = new float[bytes.Length / 4];
            for (int e = 0; e < values.Length; e++)
            {
                byte[] a = bytes.Skip(e * 4).Take(4).ToArray();
                values[e] = Reads15Fixed16Number(a);
            }

            return values;
        }

        internal static string GetString(byte[] bytes, int index, int count)
        {
            string text = Encoding.ASCII.GetString(bytes, index, count);
            return text.Replace("\0", "");
        }

        internal static string GetString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes).Replace("\0", "");
        }

        internal static DateTime ReadDateTimeType(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();

                var secondsL = BitConverter.ToUInt16(bytes, 0);
                var minutesL = BitConverter.ToUInt16(bytes, 2);
                var hoursL = BitConverter.ToUInt16(bytes, 4);
                var dayL = BitConverter.ToUInt16(bytes, 6);
                var monthL = BitConverter.ToUInt16(bytes, 8);
                var yearL = BitConverter.ToUInt16(bytes, 10);
                return new DateTime(yearL, monthL, dayL, hoursL, minutesL, secondsL);
            }

            var year = BitConverter.ToUInt16(bytes, 0);
            var month = BitConverter.ToUInt16(bytes, 2);
            var day = BitConverter.ToUInt16(bytes, 4);
            var hours = BitConverter.ToUInt16(bytes, 6);
            var minutes = BitConverter.ToUInt16(bytes, 8);
            var seconds = BitConverter.ToUInt16(bytes, 10);
            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        internal static uint ReadUInt32(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        internal static ushort ReadUInt16(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        internal static byte ReadUInt8(byte[] bytes)
        {
            return bytes[0];
        }
    }
}
