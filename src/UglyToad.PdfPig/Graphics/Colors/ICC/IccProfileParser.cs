namespace UglyToad.PdfPig.Graphics.Colors.ICC
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using UglyToad.PdfPig.Graphics.Colors.ICC.Tags;

    /// <summary>
    /// TODO
    /// </summary>
    public static class IccProfileParser
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
            var preferredCmmType = Encoding.ASCII.GetString(profile, 4, 4);

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
            int bugFix = (int)((uint)(profileVersionNumber[1] & 0x0f));
            //byte reservedV = profileVersionNumber[2];
            //byte reservedV1 = profileVersionNumber[3];

            // Profile/Device class
            // 12 to 15
            var profileDeviceClass = GetProfileClass(Encoding.ASCII.GetString(profile, 12, 4));

            // Colour space of data (possibly a derived space)
            // 16 to 19
            var colourSpaceOfData = Encoding.ASCII.GetString(profile, 16, 4);

            // PCS
            // 20 to 23
            var pcs = Encoding.ASCII.GetString(profile, 20, 4);

            // Date and time this profile was first created
            // 24 to 35 - dateTimeNumber
            var created = IccTagsHelper.ReadDateTimeType(profile.Skip(24).Take(12).ToArray());

            // ‘acsp’ (61637370h) profile file signature
            // 36 to 39
            // The profile file signature field shall contain the value “acsp” (61637370h) as a profile file signature.
            var profileFileSignature = Encoding.ASCII.GetString(profile, 36, 4);

            // Primary platform signature
            // 40 to 43
            var primaryPlatformSignature = Encoding.ASCII.GetString(profile, 40, 4);

            // Profile flags to indicate various options for the CMM such as distributed
            // processing and caching options
            // 44 to 47
            var profileFlags = profile.Skip(44).Take(4).ToArray();// TODO

            // Device manufacturer of the device for which this profile is created
            // 48 to 51
            var manufacturer = Encoding.ASCII.GetString(profile, 48, 4);

            // Device model of the device for which this profile is created
            // 52 to 55
            var deviceModel = Encoding.ASCII.GetString(profile, 52, 4);

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
            var profileCreatorSignature = Encoding.ASCII.GetString(profile, 80, 4);

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

                using (MD5 mD5 = MD5.Create())
                {
                    profileId = mD5.ComputeHash(profile);
                }
            }

            // Bytes reserved for future expansion and shall be set to zero (00h)
            // 100 to 127
            //var reserved = header.Skip(100).Take(28).ToArray();
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
                ProfileId = profileId,
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
                string signature = Encoding.ASCII.GetString(input, 0, 4);

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

            //foreach (var tag in tagTable)
            //{
            //    ParseTag4400(bytes.ToArray(), tag);
            //}

            return new IccProfile(header, tagTable, bytes.ToArray());
        }

        /// <summary>
        /// The profile version number consistent with this ICC specification is “4.4.0.0”.
        /// <para>TODO - update with correct parsers.</para>
        /// </summary>
        private static void ParseTag4400(byte[] profile, IccTagTableItem tag)
        {
            byte[] data = profile.Skip((int)tag.Offset).Take((int)tag.Size).ToArray();
            switch (tag.Signature)
            {
                case "A2B0": // 9.2.1 AToB0Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "A2B1": // 9.2.2 AToB1Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "A2B2": // 9.2.3 AToB2Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "bXYZ": // 9.2.4 blueMatrixColumnTag
                    // Permitted tag type: XYZType
                    var bXYZ = IccXyzType.Parse(data);
                    break;

                case "bTRC": // 9.2.5 blueTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    var bTRC = BaseCurveType.Parse(data);
                    break;

                case "B2A0": // 9.2.6 BToA0Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "B2A1": // 9.2.7 BToA1Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "B2A2": // 9.2.8 BToA2Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "B2D0": // 9.2.9 BToD0Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "B2D1": // 9.2.10 BToD1Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "B2D2": // 9.2.11 BToD2Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "B2D3": // 9.2.12 BToD3Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "calt": // 9.2.13 calibrationDateTimeTag
                    // Permitted tag type: dateTimeType
                    var dt = IccTagsHelper.ReadDateTimeType(data);
                    break;

                case "targ": // 9.2.14 charTargetTag
                    // Permitted tag type: textType
                    var targ = IccTextType.Parse(data);
                    break;

                case "chad": // 9.2.15 chromaticAdaptationTag
                    // Permitted tag type: s15Fixed16ArrayType
                    break;

                case "chrm": // 9.2.16 chromaticityTag
                    // Permitted tag type: chromaticityType
                    break;

                case "cicp": // 9.2.17 cicpTag
                    // Permitted tag type: cicpType
                    break;

                case "clro": // 9.2.18 colorantOrderTag
                    // Permitted tag type: colorantOrderType
                    break;

                case "clrt": // 9.2.19 colorantTableTag
                    // Permitted tag type: colorantTableType
                    break;

                case "clot": // 9.2.20 colorantTableOutTag
                    // Permitted tag type: colorantTableType
                    break;

                case "ciis": // 9.2.21 colorimetricIntentImageStateTag
                    // Permitted tag type: signatureType
                    var ciis = IccSignatureType.Parse(data);
                    break;

                case "cprt": // 9.2.22 copyrightTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    var cprt = IccMultiLocalizedUnicodeType.Parse(data);
                    break;

                case "dmnd": // 9.2.23 deviceMfgDescTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    var dmnd = IccMultiLocalizedUnicodeType.Parse(data);
                    break;

                case "dmdd": // 9.2.24 deviceModelDescTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    var dmdd = IccMultiLocalizedUnicodeType.Parse(data);
                    break;

                case "D2B0": // 9.2.25 DToB0Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "D2B1": // 9.2.26 DToB1Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "D2B2": // 9.2.27 DToB2Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "D2B3": // 9.2.28 DToB3Tag
                    // Allowed tag types: multiProcessElementsType
                    break;

                case "gamt": // 9.2.29 gamutTag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "kTRC": // 9.2.30 grayTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    var kTRC = BaseCurveType.Parse(data);
                    break;

                case "gXYZ": // 9.2.31 greenMatrixColumnTag
                    // Permitted tag type: XYZType
                    var gXYZ = IccXyzType.Parse(data);
                    break;

                case "gTRC": // 9.2.32 greenTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    var gTRC = BaseCurveType.Parse(data);
                    break;

                case "lumi": // 9.2.33 luminanceTag
                    // Permitted tag type: XYZType
                    var lumi = IccXyzType.Parse(data);
                    break;

                case "meas": // 9.2.34 measurementTag
                    var meas = IccMeasurementType.Parse(data);
                    break;

                case "meta": // 9.2.35 metadataTag
                    // Allowed tag types: dictType
                    break;

                case "wtpt": // 9.2.36 mediaWhitePointTag
                    // Permitted tag type: XYZType
                    var wtpt = IccXyzType.Parse(data);
                    break;

                case "ncl2": // 9.2.37 namedColor2Tag
                    // Permitted tag type: namedColor2Type
                    break;

                case "resp": // 9.2.38 outputResponseTag
                    // Permitted tag type: responseCurveSet16Type
                    break;

                case "rig0": // 9.2.39 perceptualRenderingIntentGamutTag
                    // Permitted tag type: signatureType
                    var rig0 = IccSignatureType.Parse(data);
                    break;

                case "pre0": // 9.2.40 preview0Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "pre1": // 9.2.41 preview1Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "pre2": // 9.2.42 preview2Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);
                    break;

                case "desc": // 9.2.43 profileDescriptionTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    var desc = IccMultiLocalizedUnicodeType.Parse(data);
                    break;

                case "pseq": // 9.2.44 profileSequenceDescTag
                    // Permitted tag type: profileSequenceDescType
                    break;

                case "psid": // 9.2.45 profileSequenceIdentifierTag
                    // Permitted tag type: profileSequenceIdentifierType
                    break;

                case "rXYZ": // 9.2.46 redMatrixColumnTag
                    // Permitted tag type: XYZType
                    var rXYZ = IccXyzType.Parse(data);
                    break;

                case "rTRC": // 9.2.47 redTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    var rTRC = BaseCurveType.Parse(data);
                    break;

                case "rig2": // 9.2.48 saturationRenderingIntentGamutTag
                    // Permitted tag type: signatureType
                    var rig2 = IccSignatureType.Parse(data);
                    break;

                case "tech": // 9.2.49 technologyTag
                    // Permitted tag type: signatureType
                    var tech = IccSignatureType.Parse(data);
                    break;

                case "vued": // 9.2.50 viewingCondDescTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    var vued = IccMultiLocalizedUnicodeType.Parse(data);
                    break;

                case "view": // 9.2.51 viewingConditionsTag
                    // Permitted tag type: viewingConditionsType
                    var view = IccViewingConditionsType.Parse(data);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid tag signature '{tag.Signature}' for ICC v4 profile.");
            }
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

        private static void Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);
            switch (typeSignature)
            {
                case "mft1":
                    Readlut8Type(bytes);
                    break;

                case "mft2":
                    Readlut16Type(bytes);
                    break;

                case "mAB ":
                    ReadlutAToBType(bytes);
                    break;

                case "mBA ":
                    ReadlutBToAType(bytes);
                    break;

                default:
                    throw new InvalidOperationException($"{typeSignature}");
            }
        }

        private static void Readlut8Type(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);

            if (typeSignature != "mft1")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // TODO
        }

        private static void Readlut16Type(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);

            if (typeSignature != "mft2")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // TODO
        }

        private static void ReadlutAToBType(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);

            if (typeSignature != "mAB ")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Number of Input Channels (i)
            // 8
            byte input = bytes.Skip(8).Take(1).ToArray()[0];

            // Number of Output Channels (o)
            // 9
            byte output = bytes.Skip(9).Take(1).ToArray()[0];

            // Reserved for padding, shall be set to 0
            // 10 to 11
            byte[] padding = bytes.Skip(10).Take(2).ToArray();

            // Offset to first “B” curve
            // 12 to 15
            uint offsetFirstB = IccTagsHelper.ReadUInt32(bytes.Skip(12).Take(4).ToArray());

            // Offset to matrix
            // 16 to 19
            uint offsetMatrix = IccTagsHelper.ReadUInt32(bytes.Skip(16).Take(4).ToArray());

            // Offset to first “M” curve
            // 20 to 23
            uint offsetFirstM = IccTagsHelper.ReadUInt32(bytes.Skip(20).Take(4).ToArray());

            // Offset to CLUT
            // 24 to 27
            uint offsetClut = IccTagsHelper.ReadUInt32(bytes.Skip(24).Take(4).ToArray());

            // Offset to first “A” curve
            // 28 to 31
            uint offsetFirstA = IccTagsHelper.ReadUInt32(bytes.Skip(28).Take(4).ToArray());

            int offset = 0;
            for (byte a = 0; a < input; a++)
            {
                var curve = BaseCurveType.Parse(bytes.Skip((int)offsetFirstA + offset).ToArray());
                offset += curve.BytesRead;
            }

            offset = 0;
            for (byte m = 0; m < output; m++)
            {
                var curve = BaseCurveType.Parse(bytes.Skip((int)offsetFirstM + offset).ToArray());
                offset += curve.BytesRead;
            }

            float[] matrix = new float[3 * 4];
            byte[] matrixData = bytes.Skip((int)offsetMatrix).Take(matrix.Length * 4).ToArray();
            for (int e = 0; e < matrix.Length; e++)
            {
                matrix[e] = IccTagsHelper.Reads15Fixed16Number(matrixData, e * 4);
            }

            offset = 0;
            for (byte b = 0; b < output; b++)
            {
                var curve = BaseCurveType.Parse(bytes.Skip((int)offsetFirstB + offset).ToArray());
                offset += curve.BytesRead;
            }

            // CLUT
            byte[] clutGridPoint = bytes.Skip((int)offsetClut).Take(input).ToArray();
            byte precision = bytes.Skip((int)offsetClut + 16).Take(1).ToArray()[0];
            byte[] clutPadding = bytes.Skip((int)offsetClut + 17).Take(3).ToArray();
            byte[] clutData = bytes.Skip((int)offsetClut + 20).ToArray();
            // TODO
        }

        private static void ReadlutBToAType(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);

            if (typeSignature != "mBA ")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Number of Input Channels (i)
            // 8
            byte input = bytes.Skip(8).Take(1).ToArray()[0];

            // Number of Output Channels (o)
            // 9
            byte output = bytes.Skip(9).Take(1).ToArray()[0];

            // Reserved for padding, shall be set to 0
            // 10 to 11
            //byte[] padding = bytes.Skip(10).Take(2).ToArray();

            // Offset to first “B” curve
            // 12 to 15
            uint offsetFirstB = IccTagsHelper.ReadUInt32(bytes.Skip(12).Take(4).ToArray());

            // Offset to matrix
            // 16 to 19
            uint offsetMatrix = IccTagsHelper.ReadUInt32(bytes.Skip(16).Take(4).ToArray());

            // Offset to first “M” curve
            // 20 to 23
            uint offsetFirstM = IccTagsHelper.ReadUInt32(bytes.Skip(20).Take(4).ToArray());

            // Offset to CLUT
            // 24 to 27
            uint offsetClut = IccTagsHelper.ReadUInt32(bytes.Skip(24).Take(4).ToArray());

            // Offset to first “A” curve
            // 28 to 31
            uint offsetFirstA = IccTagsHelper.ReadUInt32(bytes.Skip(28).Take(4).ToArray());

            int offset = 0;
            for (byte a = 0; a < input; a++)
            {
                var curve = BaseCurveType.Parse(bytes.Skip((int)offsetFirstA + offset).ToArray());
                offset += curve.BytesRead;
            }

            offset = 0;
            for (byte m = 0; m < output; m++)
            {
                var curve = BaseCurveType.Parse(bytes.Skip((int)offsetFirstM + offset).ToArray());
                offset += curve.BytesRead;
            }

            float[] matrix = new float[3 * 4];
            byte[] matrixData = bytes.Skip((int)offsetMatrix).Take(matrix.Length * 4).ToArray();
            for (int e = 0; e < matrix.Length; e++)
            {
                matrix[e] = IccTagsHelper.Reads15Fixed16Number(matrixData, e * 4);
            }

            offset = 0;
            for (byte b = 0; b < output; b++)
            {
                var curve = BaseCurveType.Parse(bytes.Skip((int)offsetFirstB + offset).ToArray());
                offset += curve.BytesRead;
            }

            // CLUT
            byte[] clutGridPoint = bytes.Skip((int)offsetClut).Take(input).ToArray();
            byte precision = bytes.Skip((int)offsetClut + 16).Take(1).ToArray()[0];
            byte[] clutPadding = bytes.Skip((int)offsetClut + 17).Take(3).ToArray();
            byte[] clutData = bytes.Skip((int)offsetClut + 20).ToArray();
            // TODO
        }

        private static void ReadMultiLocalizedUnicodeTypeOrTextType(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);
            switch (typeSignature)
            {
                case "mluc":
                    IccMultiLocalizedUnicodeType.Parse(bytes);
                    break;

                case "desc":
                case "text":
                    var text = IccTextType.Parse(bytes);
                    break;
            }
        }
    }
}
