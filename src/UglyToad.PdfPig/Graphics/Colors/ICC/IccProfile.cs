namespace UglyToad.PdfPig.Graphics.Colors.ICC
{
    /// <summary>
    /// ICC profile.
    /// </summary>
    public class IccProfile
    {
        /// <summary>
        /// ICC profile header.
        /// </summary>
        public IccProfileHeader Header { get; }

        /// <summary>
        /// The tag table acts as a table of contents for the tags and an index into the tag data element in the profiles.
        /// </summary>
        public IccTagTableItem[] TagTable { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// ICC profile v4.
        /// </summary>
        public IccProfile(IccProfileHeader header, IccTagTableItem[] tagTable, byte[] data)
        {
            Header = header;
            TagTable = tagTable;
            Data = data;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ICC Profile v{Header}";
        }
    }
}
