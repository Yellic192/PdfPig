namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;

    /// <summary>
    /// TODO
    /// </summary>
    public class IccDateTimeType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// Value.
        /// </summary>
        public DateTime DateTime { get; }

        private IccDateTimeType(DateTime dateTime, byte[] rawData)
        {
            DateTime = dateTime;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static IccDateTimeType Parse(byte[] bytes)
        {
            var dt = IccTagsHelper.ReadDateTimeType(bytes);
            return new IccDateTimeType(dt, bytes);
        }
    }
}
