using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
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
        public string Signature { get; }

        private IccSignatureType(string signature, byte[] rawData)
        {
            Signature = signature;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccSignatureType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4); // sig 

            if (typeSignature != "sig ")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Encoded value for standard observer
            // 8 to 11
            byte[] signatureBytes = bytes.Skip(8).Take(4).ToArray();

            string signature = IccTagsHelper.GetString(signatureBytes);

            return new IccSignatureType(signature, bytes);
        }
    }
}
