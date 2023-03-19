using IccProfile.Parsers;
using IccProfile.Tags;
using System;
using System.Collections.Generic;

namespace IccProfile
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

        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyDictionary<string, IIccTagType> GetTags()
        {
            switch (this.Header.VersionMajor)
            {
                case 4:
                    {
                        var tags = new Dictionary<string, IIccTagType>();
                        for (int t = 0; t < TagTable.Length; t++)
                        {
                            var tag = TagTable[t];
                            tags.Add(tag.Signature, IccProfileV4TagParser.Parse(Data, TagTable[t]));
                        }
                        return tags;
                    }

                case 2:
                    {
                        var tags = new Dictionary<string, IIccTagType>();
                        for (int t = 0; t < TagTable.Length; t++)
                        {
                            var tag = TagTable[t];
                            tags.Add(tag.Signature, IccProfileV24TagParser.Parse(Data, TagTable[t]));
                        }
                        return tags;
                    }

                default:
                    throw new NotImplementedException($"ICC Profile v{this.Header.VersionMajor}{this.Header.VersionMinor} is not supported.");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccProfile Create(byte[] bytes)
        {
            return IccProfileParser.Create(bytes);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ICC Profile v{Header}";
        }
    }
}