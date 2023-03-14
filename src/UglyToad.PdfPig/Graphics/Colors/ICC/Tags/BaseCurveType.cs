namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;
    using System.Text;

    /// <summary>
    /// TODO
    /// </summary>
    public class BaseCurveType : IIccTagType
    {
        /// <summary>
        /// Curve type signature.
        /// </summary>
        public string Signature { get; }

        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[] Values { get; }

        internal int BytesRead { get; }

        /// <summary>
        /// TODO
        /// </summary>
        protected BaseCurveType(string signature, float[] values, byte[] rawData, int bytesRead)
        {
            Signature = signature;
            Values = values;
            RawData = rawData;
            BytesRead = bytesRead;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static BaseCurveType Parse(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);
            switch (typeSignature)
            {
                case "curv":
                    return IccCurveType.Parse(bytes);

                case "para":
                    return IccParametricCurveType.Parse(bytes);

                default:
                    throw new InvalidOperationException(typeSignature);
            }
        }
    }
}
