using IccProfile.Tags;
using System;
using System.Linq;

namespace IccProfile.Parsers
{
    internal static class IccProfileV24TagParser
    {
        /// <summary>
        /// The profile version number consistent with this ICC specification is “2.4.0.0”.
        /// <para>TODO - update with correct parsers.</para>
        /// </summary>
        public static IIccTagType Parse(byte[] profile, IccTagTableItem tag)
        {
            byte[] data = profile.Skip((int)tag.Offset).Take((int)tag.Size).ToArray();
            switch (tag.Signature)
            {
                case "A2B0": // 6.4.1 AToB0Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "A2B1": // 6.4.2 AToB1Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "A2B2": // 6.4.3 AToB2Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "bXYZ": // 6.4.4 blueColorantTag
                    // Tag Type: XYZType
                    return IccXyzType.Parse(data);

                case "bTRC": // 6.4.5 blueTRCTag
                    // Tag Type: curveType
                    return IccCurveType.Parse(data);

                case "B2A0": // 6.4.6 BToA0Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "B2A1": // 6.4.7 BToA1Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "B2A2": // 6.4.8 BToA2Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "calt": // 6.4.9 calibrationDateTimeTag
                    // Tag Type: dateTimeType
                    return IccDateTimeType.Parse(data);

                case "targ": // 6.4.10 charTargetTag
                    // Tag Type: textType
                    return IccTextType.Parse(data);

                case "chad": // 6.4.11 chromaticAdaptationTag
                    // Tag Type: s15Fixed16ArrayType
                    return IccS15Fixed16ArrayType.Parse(data);

                case "chrm": // 6.4.12 chromaticityTag
                    // Tag Type: chromaticityType
                    break;

                case "cprt": // 6.4.13 copyrightTag
                    // Tag Type: textType
                    return IccTextType.Parse(data);

                case "crdi": // 6.4.14 crdInfoTag
                    // Tag Type: crdInfoType
                    break;

                case "dmnd": // 6.4.15 deviceMfgDescTag
                    // Tag Type: textDescriptionType
                    return IccTextType.Parse(data);

                case "dmdd": // 6.4.16 deviceModelDescTag
                    // Tag Type: textDescriptionType
                    return IccTextType.Parse(data);

                case "devs": // 6.4.17 deviceSettingsTag
                    // Tag Type: deviceSettingsType
                    break;

                case "gamt": // 6.4.18 gamutTag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "kTRC": // 6.4.19 grayTRCTag
                    // Tag Type: curveType
                    return IccCurveType.Parse(data);

                case "gXYZ": // 6.4.20 greenColorantTag
                    // Tag Type: XYZType
                    return IccXyzType.Parse(data);

                case "gTRC": // 6.4.21 greenTRCTag
                    // Tag Type: curveType
                    return IccCurveType.Parse(data);

                case "lumi": // 6.4.22 luminanceTag
                    // Tag Type: XYZType
                    return IccXyzType.Parse(data);

                case "meas": // 6.4.23 measurementTag
                    // Tag Type: measurementType
                    return IccMeasurementType.Parse(data);

                case "bkpt": // 6.4.24 mediaBlackPointTag
                    // Tag Type: XYZType
                    return IccXyzType.Parse(data);

                case "wtpt": // 6.4.25 mediaWhitePointTag
                    // Tag Type: XYZType
                    return IccXyzType.Parse(data);

                case "ncol": // 6.4.26 namedColorTag
                    // Tag Type: namedColorType
                    break;

                case "ncl2": // 6.4.27 namedColor2Tag
                    // Tag Type: namedColor2Type
                    break;

                case "resp": // 6.4.28 outputResponseTag
                    // Tag Type: responseCurveSet16Type
                    break;

                case "pre0": // 6.4.29 preview0Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "pre1": // 6.4.30 preview1Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "pre2": // 6.4.31 preview2Tag
                    // Tag Type: lut8Type or lut16Type
                    return IccBaseLutType.Parse(data);

                case "desc": // 6.4.32 profileDescriptionTag
                    // Tag Type: textDescriptionType
                    return IccTextType.Parse(data);

                case "pseq": // 6.4.33 profileSequenceDescTag
                    // Tag Type: profileSequenceDescType
                    break;

                case "psd0": // 6.4.34 ps2CRD0Tag
                    // Tag Type: dataType
                    break;

                case "psd1": // 6.4.35 ps2CRD1Tag
                    // Tag Type: dataType
                    break;

                case "psd2": // 6.4.36 ps2CRD2Tag
                    // Tag Type: dataType
                    break;

                case "psd3": // 6.4.37 ps2CRD3Tag
                    // Tag Type: dataType
                    break;

                case "ps2s": // 6.4.38 ps2CSATag
                    // Tag Type: dataType
                    break;

                case "ps2i": // 6.4.39 ps2RenderingIntentTag
                    // Tag Type: dataType
                    break;

                case "rXYZ": // 6.4.40 redColorantTag
                    // Tag Type: XYZType
                    return IccXyzType.Parse(data);

                case "rTRC": // 6.4.41 redTRCTag
                    // Tag Type: curveType
                    return IccCurveType.Parse(data);

                case "scrd": // 6.4.42 screeningDescTag
                    // Tag Type: textDescriptionType
                    return IccTextType.Parse(data);

                case "scrn": // 6.4.43 screeningTag
                    // Tag Type: screeningType
                    break;

                case "tech": // 6.4.44 technologyTag
                    // Tag Type: signatureType
                    return IccSignatureType.Parse(data);

                case "bfd": // 6.4.45 ucrbgTag
                    // Tag Type: ucrbgType
                    break;

                case "vued": // 6.4.46 viewingCondDescTag
                    // Tag Type: textDescriptionType
                    return IccTextType.Parse(data);

                case "view": // 6.4.47 viewingConditionsTag
                    // Tag Type: viewingConditionsType
                    return IccViewingConditionsType.Parse(data);

                default:
                    throw new InvalidOperationException($"Invalid tag signature '{tag.Signature}' for ICC v2 profile.");
            }

            throw new NotImplementedException($"Tag signature '{tag.Signature}' for ICC v2 profile to implement.");
        }
    }
}
