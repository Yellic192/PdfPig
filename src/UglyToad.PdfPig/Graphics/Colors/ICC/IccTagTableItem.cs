namespace IccProfile
{
    /// <summary>
    /// TODO
    /// </summary>
    public readonly struct IccTagTableItem
    {
        /// <summary>
        /// Tag Signature.
        /// </summary>
        public string Signature { get; }

        /// <summary>
        /// Offset to beginning of tag data element.
        /// </summary>
        public uint Offset { get; }

        /// <summary>
        /// Size of tag data element.
        /// </summary>
        public uint Size { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccTagTableItem(string signature, uint offset, uint size)
        {
            Signature = signature;
            Offset = offset;
            Size = size;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Signature}: offset={Offset}, size={Size}";
        }
    }
}
