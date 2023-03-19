using IccProfile.Tags;
using System;
using System.Linq;

namespace IccProfile.Parsers
{
    internal static class IccProfileV4TagParser
    {
        /// <summary>
        /// The profile version number consistent with this ICC specification is “4.4.0.0”.
        /// <para>TODO - update with correct parsers.</para>
        /// </summary>
        public static IIccTagType Parse(byte[] profile, IccTagTableItem tag)
        {
            byte[] data = profile.Skip((int)tag.Offset).Take((int)tag.Size).ToArray();
            switch (tag.Signature)
            {
                case "A2B0": // 9.2.1 AToB0Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "A2B1": // 9.2.2 AToB1Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "A2B2": // 9.2.3 AToB2Tag
                    // Permitted tag types: lut8Type or lut16Type or lutAToBType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "bXYZ": // 9.2.4 blueMatrixColumnTag
                    // Permitted tag type: XYZType
                    return IccXyzType.Parse(data);

                case "bTRC": // 9.2.5 blueTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    return IccBaseCurveType.Parse(data);

                case "B2A0": // 9.2.6 BToA0Tag
                             // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "B2A1": // 9.2.7 BToA1Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data); ;

                case "B2A2": // 9.2.8 BToA2Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

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
                    return IccDateTimeType.Parse(data);

                case "targ": // 9.2.14 charTargetTag
                    // Permitted tag type: textType
                    return IccTextType.Parse(data);

                case "chad": // 9.2.15 chromaticAdaptationTag
                    // Permitted tag type: s15Fixed16ArrayType
                    return IccS15Fixed16ArrayType.Parse(data);

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
                    return IccSignatureType.Parse(data);

                case "cprt": // 9.2.22 copyrightTag
                case "dmnd": // 9.2.23 deviceMfgDescTag
                case "dmdd": // 9.2.24 deviceModelDescTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    return IccMultiLocalizedUnicodeType.Parse(data);

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
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "kTRC": // 9.2.30 grayTRCTag
                case "gTRC": // 9.2.32 greenTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    return IccBaseCurveType.Parse(data);

                case "gXYZ": // 9.2.31 greenMatrixColumnTag
                case "lumi": // 9.2.33 luminanceTag
                    // Permitted tag type: XYZType
                    return IccXyzType.Parse(data);

                case "meas": // 9.2.34 measurementTag
                    return IccMeasurementType.Parse(data);

                case "meta": // 9.2.35 metadataTag
                    // Allowed tag types: dictType
                    break;

                case "wtpt": // 9.2.36 mediaWhitePointTag
                    // Permitted tag type: XYZType
                    return IccXyzType.Parse(data);

                case "ncl2": // 9.2.37 namedColor2Tag
                    // Permitted tag type: namedColor2Type
                    break;

                case "resp": // 9.2.38 outputResponseTag
                    // Permitted tag type: responseCurveSet16Type
                    break;

                case "rig0": // 9.2.39 perceptualRenderingIntentGamutTag
                    // Permitted tag type: signatureType
                    return IccSignatureType.Parse(data);

                case "pre0": // 9.2.40 preview0Tag
                             // Permitted tag types: lut8Type or lut16Type or lutAToBType or lutBToAType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "pre1": // 9.2.41 preview1Tag
                    // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "pre2": // 9.2.42 preview2Tag
                             // Permitted tag types: lut8Type or lut16Type or lutBToAType
                    return Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(data);

                case "desc": // 9.2.43 profileDescriptionTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    return IccMultiLocalizedUnicodeType.Parse(data);

                case "pseq": // 9.2.44 profileSequenceDescTag
                    // Permitted tag type: profileSequenceDescType
                    break;

                case "psid": // 9.2.45 profileSequenceIdentifierTag
                    // Permitted tag type: profileSequenceIdentifierType
                    break;

                case "rXYZ": // 9.2.46 redMatrixColumnTag
                    // Permitted tag type: XYZType
                    return IccXyzType.Parse(data);

                case "rTRC": // 9.2.47 redTRCTag
                    // Permitted tag types: curveType or parametricCurveType
                    return IccBaseCurveType.Parse(data);

                case "rig2": // 9.2.48 saturationRenderingIntentGamutTag
                    // Permitted tag type: signatureType
                    return IccSignatureType.Parse(data);

                case "tech": // 9.2.49 technologyTag
                    // Permitted tag type: signatureType
                    return IccSignatureType.Parse(data);

                case "vued": // 9.2.50 viewingCondDescTag
                    // Permitted tag type: multiLocalizedUnicodeType
                    return IccMultiLocalizedUnicodeType.Parse(data);

                case "view": // 9.2.51 viewingConditionsTag
                    // Permitted tag type: viewingConditionsType
                    return IccViewingConditionsType.Parse(data);


                // bkpt should not be there according to specs
                case "bkpt": // 9.2.36 mediaWhitePointTag
                    // Permitted tag type: XYZType
                    return IccXyzType.Parse(data);

                default:
                    throw new InvalidOperationException($"Invalid tag signature '{tag.Signature}' for ICC v4 profile.");
            }

            throw new NotImplementedException($"Tag signature '{tag.Signature}' for ICC v4 profile.");
        }

        private static IIccTagType Readlut8TypeOrlut16TypeOrlutAToBTypeOrlutBToAType(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);
            switch (typeSignature)
            {
                case "mft1":
                case "mft2":
                    return IccBaseLutType.Parse(bytes);

                case "mAB ":
                case "mBA ":
                    return IccLutABType.Parse(bytes);

                default:
                    throw new InvalidOperationException($"{typeSignature}");
            }
        }
    }
}
