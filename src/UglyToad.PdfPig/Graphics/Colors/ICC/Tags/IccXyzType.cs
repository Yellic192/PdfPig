namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System.Linq;
    using System;

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
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
                var zL = IccTagsHelper.Reads15Fixed16Number(bytes, 0);
                var yL = IccTagsHelper.Reads15Fixed16Number(bytes, 4);
                var xL = IccTagsHelper.Reads15Fixed16Number(bytes, 8);
                return new IccXyzType(xL, yL, zL, bytes);
            }

            var x = IccTagsHelper.Reads15Fixed16Number(bytes, 0);
            var y = IccTagsHelper.Reads15Fixed16Number(bytes, 4);
            var z = IccTagsHelper.Reads15Fixed16Number(bytes, 8);
            return new IccXyzType(x, y, z, bytes);
        }
    }
}
