namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO
    /// </summary>
    public sealed class IccSignatureType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public byte[] Signature { get; }

        private IccSignatureType(byte[] signature, byte[] rawData)
        {
            Signature = signature;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccSignatureType Parse(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4); // sig 

            if (typeSignature != "sig ")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Encoded value for standard observer
            // 8 to 11
            byte[] signature = bytes.Skip(8).Take(4).ToArray();

            return new IccSignatureType(signature, bytes);
        }
    }
}
