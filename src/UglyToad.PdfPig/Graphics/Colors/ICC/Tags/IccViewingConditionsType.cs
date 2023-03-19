using System;
using System.Linq;
using System.Text;
using IccProfile.Parsers;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public class IccViewingConditionsType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// Un-normalized CIEXYZ values for illuminant (in which Y is in cd/m2).
        /// </summary>
        public IccXyzType Illuminant { get; }

        /// <summary>
        /// Un-normalized CIEXYZ values for surround (in which Y is in cd/m2).
        /// </summary>
        public IccXyzType Surround { get; }

        /// <summary>
        /// Illuminant type.
        /// </summary>
        public byte[] IlluminantType { get; }

        private IccViewingConditionsType(IccXyzType illuminant, IccXyzType surround, byte[] illuminantType)
        {
            Illuminant = illuminant;
            Surround = surround;
            IlluminantType = illuminantType;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccViewingConditionsType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4); // view

            if (typeSignature != "view")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Un-normalized CIEXYZ values for illuminant (in which Y is in cd/m2)
            // 8 to 19
            var illuminant = IccXyzType.Parse(bytes.Skip(8).Take(12).ToArray());

            // Un-normalized CIEXYZ values for surround (in which Y is in cd/m2)
            // 20 to 31
            var surround = IccXyzType.Parse(bytes.Skip(20).Take(12).ToArray());

            // Illuminant type
            // 32 to 35
            // As described in measurementType
            var illuminantType = bytes.Skip(32).Take(4).ToArray(); // TODO

            return new IccViewingConditionsType(illuminant, surround, illuminantType);
        }
    }
}
