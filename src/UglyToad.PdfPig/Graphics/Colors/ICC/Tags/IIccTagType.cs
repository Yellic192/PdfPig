namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    /// <summary>
    /// Interface for ICC tage type.
    /// </summary>
    public interface IIccTagType
    {
        /*
        /// <summary>
        /// Tag Signature.
        /// </summary>
        public string Signature { get; }
        */

        /// <summary>
        /// Tag raw data.
        /// </summary>
        public byte[] RawData { get; }
    }
}
