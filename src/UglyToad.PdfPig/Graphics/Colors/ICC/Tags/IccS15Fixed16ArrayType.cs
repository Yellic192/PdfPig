using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public class IccS15Fixed16ArrayType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[] Values { get; }

        private IccS15Fixed16ArrayType(float[] values, byte[] rawData)
        {
            Values = values;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IccS15Fixed16ArrayType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4); // sig 

            if (typeSignature != "sf32")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // An array of s15Fixed16Number values
            // 8 to end
            byte[] valuesBytes = bytes.Skip(8).ToArray();
            float[] values = IccTagsHelper.Reads15Fixed16Array(valuesBytes);

            return new IccS15Fixed16ArrayType(values, valuesBytes); // TODO - actual raw byte size
        }
    }
}
