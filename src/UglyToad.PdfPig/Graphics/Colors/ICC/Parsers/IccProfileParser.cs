using System;
using System.Linq;
using System.Security.Cryptography;
using IccProfile.Tags;

namespace IccProfile.Parsers
{
    /// <summary>
    /// TODO
    /// </summary>
    internal static class IccProfileParser
    {
        /// <summary>
        /// Header - The profile header is 128 bytes in length and contains 18 fields.
        /// </summary>
        private static IccProfileHeader ParseHeader(byte[] profile)
        {
            // Profile size
            // 0 to 3 - UInt32Number
            uint profileSize = BitConverter.ToUInt32(profile.Take(4).ToArray(), 0);

            // Preferred CMM type
            // 4 to 7
            var preferredCmmType = IccTagsHelper.GetString(profile, 4, 4);

            // Profile version number
            // 8 to 11
            /*
             * The profile version with which the profile is compliant shall be encoded as binary-coded decimal in the profile
             * version field. The first byte (byte 8) shall identify the major version and byte 9 shall identify the minor version
             * and bug fix version in each 4-bit half of the byte. Bytes 10 and 11 are reserved and shall be set to zero. The
             * major and minor versions are set by the International Color Consortium. The profile version number consistent
             * with this ICC specification is “4.4.0.0” (encoded as 04400000h)
             */
            var profileVersionNumber = profile.Skip(8).Take(4).ToArray();
            int major = profileVersionNumber[0];
            int minor = (int)((uint)(profileVersionNumber[1] & 0xf0) >> 4);
            int bugFix = (int)(uint)(profileVersionNumber[1] & 0x0f);
            //byte reservedV = profileVersionNumber[2];
            //byte reservedV1 = profileVersionNumber[3];

            // Profile/Device class
            // 12 to 15
            var profileDeviceClass = GetProfileClass(IccTagsHelper.GetString(profile, 12, 4));

            // Colour space of data (possibly a derived space)
            // 16 to 19
            var colourSpaceOfData = IccTagsHelper.GetString(profile, 16, 4);

            // PCS
            // 20 to 23
            var pcs = IccTagsHelper.GetString(profile, 20, 4).Trim();

            // Date and time this profile was first created
            // 24 to 35 - dateTimeNumber
            var created = IccTagsHelper.ReadDateTimeType(profile.Skip(24).Take(12).ToArray());

            // ‘acsp’ (61637370h) profile file signature
            // 36 to 39
            // The profile file signature field shall contain the value “acsp” (61637370h) as a profile file signature.
            var profileFileSignature = IccTagsHelper.GetString(profile, 36, 4);

            // Primary platform signature
            // 40 to 43
            var primaryPlatformSignature = IccTagsHelper.GetString(profile, 40, 4);

            // Profile flags to indicate various options for the CMM such as distributed
            // processing and caching options
            // 44 to 47
            var profileFlags = profile.Skip(44).Take(4).ToArray();// TODO

            // Device manufacturer of the device for which this profile is created
            // 48 to 51
            var manufacturer = IccTagsHelper.GetString(profile, 48, 4);

            // Device model of the device for which this profile is created
            // 52 to 55
            var deviceModel = IccTagsHelper.GetString(profile, 52, 4);

            // Device attributes unique to the particular device setup such as media type
            // 56 to 63
            var deviceAttributes = profile.Skip(56).Take(8).ToArray(); // TODO

            // Rendering Intent
            // 64 to 67
            var renderingIntent = IccTagsHelper.ReadUInt32(profile.Skip(64).Take(4).ToArray());

            // The nCIEXYZ values of the illuminant of the PCS
            // 68 to 79 - XYZNumber
            // shall be X = 0,964 2, Y = 1,0 and Z = 0,824 9
            // These values are the nCIEXYZ values of CIE illuminant D50
            var nCIEXYZ = IccXyzType.Parse(profile.Skip(68).Take(12).ToArray());

            // Profile creator signature
            // 80 to 83
            var profileCreatorSignature = IccTagsHelper.GetString(profile, 80, 4);

            // Profile ID
            // 84 to 99
            byte[] profileId = profile.Skip(84).Take(16).ToArray();

            if (profileId.All(b => b == 0))
            {
                // Compute profile id
                // This field, if not zero (00h), shall hold the Profile ID. The Profile ID shall be calculated using the MD5
                // fingerprinting method as defined in Internet RFC 1321.The entire profile, whose length is given by the size field
                // in the header, with the profile flags field (bytes 44 to 47, see 7.2.11), rendering intent field (bytes 64 to 67, see
                // 7.2.15), and profile ID field (bytes 84 to 99) in the profile header temporarily set to zeros (00h), shall be used to
                // calculate the ID. A profile ID field value of zero (00h) shall indicate that a profile ID has not been calculated.
                // Profile creators should compute and record a profile ID.

                //profileId = MD5.HashData(profile);
            }

            // Bytes reserved for future expansion and shall be set to zero (00h)
            // 100 to 127
            // var reserved = header.Skip(100).Take(28).ToArray();
            // check if this is 28 zeros

            return new IccProfileHeader()
            {
                ProfileSize = profileSize,
                VersionMajor = major,
                VersionMinor = minor,
                VersionBugFix = bugFix,
                Cmm = preferredCmmType,
                ProfileClass = profileDeviceClass,
                ColourSpace = GetColourSpaceType(colourSpaceOfData),
                Pcs = pcs,
                Created = created,
                ProfileSignature = profileFileSignature,
                PrimaryPlatformSignature = GetPrimaryPlatforms(primaryPlatformSignature),
                ProfileFlags = profileFlags,
                DeviceManufacturer = manufacturer,
                DeviceModel = deviceModel,
                DeviceAttributes = deviceAttributes,
                RenderingIntent = (IccRenderingIntent)renderingIntent,
                nCIEXYZ = nCIEXYZ,
                ProfileCreatorSignature = profileCreatorSignature,
                ProfileId = profileId
                //Reserved = reserved
            };
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccTagTableItem[] ParseTagTable(byte[] bytes)
        {
            // Tag count (n)
            // 0 to 3
            uint tagCount = IccTagsHelper.ReadUInt32(bytes.Take(4).ToArray());

            IccTagTableItem[] tagTableItems = new IccTagTableItem[tagCount];

            byte[] tagTable = bytes.Skip(4).ToArray();

            for (var i = 0; i < tagCount; ++i)
            {
                byte[] input = tagTable.Skip(i * 12).Take(12).ToArray();

                // Tag Signature
                // 4 to 7
                string signature = IccTagsHelper.GetString(input, 0, 4);

                // Offset to beginning of tag data element
                // 8 to 11
                uint offset = IccTagsHelper.ReadUInt32(input.Skip(4).Take(4).ToArray());

                // Size of tag data element
                // 12 to 15
                uint size = IccTagsHelper.ReadUInt32(input.Skip(8).Take(4).ToArray());

                tagTableItems[i] = new IccTagTableItem(signature, offset, size);
            }

            return tagTableItems;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccProfile Create(byte[] bytes)
        {
            var header = ParseHeader(bytes);
            var tagTable = ParseTagTable(bytes.Skip(128).ToArray()); // Should be lazy

            return new IccProfile(header, tagTable, bytes.ToArray());
        }

        private static IccPrimaryPlatforms GetPrimaryPlatforms(string platform)
        {
            switch (platform)
            {
                case "APPL":
                    return IccPrimaryPlatforms.AppleComputer;

                case "MSFT":
                    return IccPrimaryPlatforms.MicrosoftCorporation;

                case "SGI ":
                    return IccPrimaryPlatforms.SiliconGraphics;

                case "SUNW":
                    return IccPrimaryPlatforms.SunMicrosystems;

                default:
                    return IccPrimaryPlatforms.Unidentified;
            }
        }

        private static IccColourSpaceType GetColourSpaceType(string colourSpace)
        {
            switch (colourSpace)
            {
                case "XYZ ":
                    return IccColourSpaceType.nCIEXYZorPCSXYZ;

                case "Lab ":
                    return IccColourSpaceType.CIELABorPCSLAB;

                case "Luv ":
                    return IccColourSpaceType.CIELUV;

                case "YCbr":
                    return IccColourSpaceType.YCbCr;

                case "Yxy ":
                    return IccColourSpaceType.CIEYxy;

                case "RGB ":
                    return IccColourSpaceType.RGB;

                case "GRAY":
                    return IccColourSpaceType.Gray;

                case "HSV ":
                    return IccColourSpaceType.HSV;

                case "HLS ":
                    return IccColourSpaceType.HLS;

                case "CMYK":
                    return IccColourSpaceType.CMYK;

                case "CMY ":
                    return IccColourSpaceType.CMY;

                case "2CLR":
                    return IccColourSpaceType.Colour2;

                case "3CLR":
                    return IccColourSpaceType.Colour3;

                case "4CLR":
                    return IccColourSpaceType.Colour4;

                case "5CLR":
                    return IccColourSpaceType.Colour5;

                case "6CLR":
                    return IccColourSpaceType.Colour6;

                case "7CLR":
                    return IccColourSpaceType.Colour7;

                case "8CLR":
                    return IccColourSpaceType.Colour8;

                case "9CLR":
                    return IccColourSpaceType.Colour9;

                case "ACLR":
                    return IccColourSpaceType.Colour10;

                case "BCLR":
                    return IccColourSpaceType.Colour11;

                case "CCLR":
                    return IccColourSpaceType.Colour12;

                case "DCLR":
                    return IccColourSpaceType.Colour13;

                case "ECLR":
                    return IccColourSpaceType.Colour14;

                case "FCLR":
                    return IccColourSpaceType.Colour15;

                default:
                    throw new ArgumentException($"Unknown colour space type '{colourSpace}'.");
            }
        }

        private static IccProfileClass GetProfileClass(string profile)
        {
            switch (profile)
            {
                case "scnr":
                    return IccProfileClass.InputDeviceProfile;

                case "mntr":
                    return IccProfileClass.DisplayDeviceProfile;

                case "prtr":
                    return IccProfileClass.OutputDeviceProfile;

                case "link":
                    return IccProfileClass.DeviceLinkProfile;

                case "spac":
                    return IccProfileClass.ColorSpaceProfile;

                case "abst":
                    return IccProfileClass.AbstractProfile;

                case "nmcl":
                    return IccProfileClass.NamedColorProfile;

                default:
                    throw new ArgumentException($"Unknown profile class '{profile}'.");
            }
        }
    }
}
