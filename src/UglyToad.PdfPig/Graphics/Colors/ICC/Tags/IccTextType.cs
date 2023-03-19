using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public class IccTextType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public string Text { get; }

        private IccTextType(string text, byte[] rawData)
        {
            Text = text;
            RawData = rawData;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccTextType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);

            if (typeSignature != "text" && typeSignature != "desc")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // A string of (element size 8) 7-bit ASCII characters
            // Variable
            string text = IccTagsHelper.GetString(bytes.Skip(8).ToArray());
            return new IccTextType(text, bytes);
        }
    }
}
