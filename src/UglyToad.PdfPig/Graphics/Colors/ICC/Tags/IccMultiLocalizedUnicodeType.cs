namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO
    /// </summary>
    public class IccMultiLocalizedUnicodeType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// Number of records (n).
        /// </summary>
        public int NumberOfRecord { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccMultiLocalizedUnicodeRecord[] Records { get; }

        private IccMultiLocalizedUnicodeType(IccMultiLocalizedUnicodeRecord[] records, int numberOfRecord, byte[] rawData)
        {
            NumberOfRecord = numberOfRecord;
            Records = records;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccMultiLocalizedUnicodeType Parse(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4); // mluc

            if (typeSignature != "mluc")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Number of records (n)
            // 8 to 11
            uint numberOfRecord = IccTagsHelper.ReadUInt32(bytes.Skip(8).Take(4).ToArray());

            // Record size: the length in bytes of every record. The value is 12.
            // 12 to 15
            uint recordSize = IccTagsHelper.ReadUInt32(bytes.Skip(12).Take(4).ToArray());

            if (recordSize != 12)
            {
                throw new ArgumentException(nameof(recordSize));
            }

            IccMultiLocalizedUnicodeRecord[] records = new IccMultiLocalizedUnicodeRecord[numberOfRecord];
            for (var i = 0; i < numberOfRecord; ++i)
            {
                byte[] input = bytes.Skip(12 + 4 + i * 12).Take(12).ToArray();

                // First record language code: in accordance with the
                // language code specified in ISO 639-1
                // 16 to 17
                string language = Encoding.ASCII.GetString(input.Skip(0).Take(2).ToArray());

                // First record country code: in accordance with the country
                // code specified in ISO 3166-1
                // 18 to 19
                string country = Encoding.ASCII.GetString(input.Skip(2).Take(2).ToArray());

                // First record string length: the length in bytes of the string
                // 20 to 23
                uint length = IccTagsHelper.ReadUInt32(input.Skip(4).Take(4).ToArray());

                // First record string offset: the offset from the start of the tag
                // to the start of the string, in bytes.
                // 24 to 27
                uint offset = IccTagsHelper.ReadUInt32(input.Skip(8).Take(4).ToArray());

                string text = Encoding.ASCII.GetString(bytes, (int)offset, (int)length).Replace("\0", "");

                records[i] = new IccMultiLocalizedUnicodeRecord(language, country, text);
            }

            return new IccMultiLocalizedUnicodeType(records, (int)numberOfRecord, bytes); // TODO bytes actual lenght
        }

        /// <summary>
        /// TODO
        /// </summary>
        public struct IccMultiLocalizedUnicodeRecord
        {
            /// <summary>
            /// Language code specified in ISO 639-1.
            /// </summary>
            public string Language { get; }

            /// <summary>
            /// Country code specified in ISO 3166-1.
            /// </summary>
            public string Country { get; }

            /// <summary>
            /// TODO
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// TODO
            /// </summary>
            public IccMultiLocalizedUnicodeRecord(string language, string country, string text)
            {
                Language = language;
                Country = country;
                Text = text;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"{Country}-{Language}: {Text}";
            }
        }
    }
}
