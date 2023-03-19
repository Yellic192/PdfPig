using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// XYZ type.
    /// </summary>
    public sealed class IccXyzType : IIccTagType
    {
        //public string Signature { get; internal set; }

        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// X.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Y.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Z.
        /// </summary>
        public float Z { get; }

        private IccXyzType(float x, float y, float z, byte[] rawData)
        {
            X = x;
            Y = y;
            Z = z;
            RawData = rawData;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        /// <summary>
        /// Parse the bytes.
        /// </summary>
        public static IccXyzType Parse(byte[] bytes)
        {
            // Length 12 is for header
            if (bytes.Length == 20)
            {
                bytes = bytes.Skip(8).Take(12).ToArray();
            }
            else if (bytes.Length != 12)
            {
                throw new ArgumentException("Length is not correct", nameof(bytes));
            }

            var xyz = IccTagsHelper.Reads15Fixed16Array(bytes);
            return new IccXyzType(xyz[0], xyz[1], xyz[2], bytes);
        }
    }
}
