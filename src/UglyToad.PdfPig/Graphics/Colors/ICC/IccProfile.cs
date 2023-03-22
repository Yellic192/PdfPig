using System;
using System.Collections.Generic;
using System.Linq;
using IccProfileNet.Parsers;
using IccProfileNet.Tags;

namespace IccProfileNet
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

        private readonly Lazy<IccTagTableItem[]> _tagTable;
        /// <summary>
        /// The tag table acts as a table of contents for the tags and an index into the tag data element in the profiles.
        /// </summary>
        public IccTagTableItem[] TagTable => _tagTable.Value;

        private readonly Lazy<IReadOnlyDictionary<string, IccTagTypeBase>> _tags;

        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyDictionary<string, IccTagTypeBase> Tags => _tags.Value;

        /// <summary>
        /// TODO
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// ICC profile v4.
        /// </summary>
        public IccProfile(byte[] data)
        {
            Data = data;
            Header = new IccProfileHeader(data);
            _tagTable = new Lazy<IccTagTableItem[]>(() => ParseTagTable(data.Skip(128).ToArray()));
            _tags = new Lazy<IReadOnlyDictionary<string, IccTagTypeBase>>(() => GetTags());
        }

        private static IccTagTableItem[] ParseTagTable(byte[] bytes)
        {
            // Tag count (n)
            // 0 to 3
            uint tagCount = IccHelper.ReadUInt32(bytes
                .Skip(IccTagTableItem.TagCountOffset)
                .Take(IccTagTableItem.TagCountLength).ToArray());

            IccTagTableItem[] tagTableItems = new IccTagTableItem[tagCount];

            for (var i = 0; i < tagCount; ++i)
            {
                int currentOffset = i * (IccTagTableItem.TagSignatureLength +
                                         IccTagTableItem.TagOffsetLength +
                                         IccTagTableItem.TagSizeLength);

                // Tag Signature
                // 4 to 7
                string signature = IccHelper.GetString(bytes,
                    currentOffset + IccTagTableItem.TagSignatureOffset, IccTagTableItem.TagSignatureLength);

                // Offset to beginning of tag data element
                // 8 to 11
                uint offset = IccHelper.ReadUInt32(bytes
                    .Skip(currentOffset + IccTagTableItem.TagOffsetOffset)
                    .Take(IccTagTableItem.TagOffsetLength).ToArray());

                // Size of tag data element
                // 12 to 15
                uint size = IccHelper.ReadUInt32(bytes
                    .Skip(currentOffset + IccTagTableItem.TagSizeOffset)
                    .Take(IccTagTableItem.TagSizeLength).ToArray());

                tagTableItems[i] = new IccTagTableItem(signature, offset, size);
            }

            return tagTableItems;
        }

        private IReadOnlyDictionary<string, IccTagTypeBase> GetTags()
        {
            switch (Header.VersionMajor)
            {
                case 4:
                    {
                        var tags = new Dictionary<string, IccTagTypeBase>();
                        for (int t = 0; t < TagTable.Length; t++)
                        {
                            IccTagTableItem tag = TagTable[t];
                            tags.Add(tag.Signature, IccProfileV4TagParser.Parse(Data, tag));
                        }
                        return tags;
                    }

                case 2:
                    {
                        var tags = new Dictionary<string, IccTagTypeBase>();
                        for (int t = 0; t < TagTable.Length; t++)
                        {
                            IccTagTableItem tag = TagTable[t];
                            tags.Add(tag.Signature, IccProfileV2TagParser.Parse(Data, tag));
                        }
                        return tags;
                    }

                default:
                    throw new NotImplementedException($"ICC Profile v{Header.VersionMajor}.{Header.VersionMinor} is not supported.");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool TryProcess(double[] values, out double[] output)
        {
            try
            {
                var key = IccLutABType.GetProfileTag(Header.ProfileClass, Header.RenderingIntent, IccLutABType.LutABType.AB);
                if (Tags.TryGetValue(key, out var value) && value is IccLutABType lutAB)
                {
                    output = lutAB.Process(values, Header);
                    return true;
                }

                //Three-component matrix-based profiles

                output = null;
                return false;
            }
            catch (Exception)
            {
                output = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ICC Profile v{Header}";
        }
    }
}