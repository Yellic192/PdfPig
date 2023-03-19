using IccProfile.Parsers;
using System;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public abstract class IccBaseCurveType : IIccTagType
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
        protected IccBaseCurveType(string signature, float[] values, byte[] rawData, int bytesRead)
        {
            Signature = signature;
            Values = values;
            RawData = rawData;
            BytesRead = bytesRead;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public abstract double Compute(double values);

        /// <summary>
        /// TODO
        /// </summary>
        public static IccBaseCurveType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);
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
