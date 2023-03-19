using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public sealed class IccMeasurementType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccXyzType Tristimulus { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public string StandardObserver { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public string MeasurementGeometry { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public string MeasurementFlare { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public string StandardIlluminant { get; }

        private IccMeasurementType(IccXyzType tristimulus, string standardObserver, string measurementGeometry,
            string measurementFlare, string standardIlluminant, byte[] rawData)
        {
            Tristimulus = tristimulus;
            StandardObserver = standardObserver;
            MeasurementGeometry = measurementGeometry;
            MeasurementFlare = measurementFlare;
            StandardIlluminant = standardIlluminant;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccMeasurementType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4); // meas

            if (typeSignature != "meas")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Encoded value for standard observer
            // 8 to 11
            byte[] standardObserverBytes = bytes.Skip(8).Take(4).ToArray();
            string standardObserverHex = BitConverter.ToString(standardObserverBytes).Replace("-", string.Empty);
            string standardObserver = string.Empty;
            /*
             * Table 50 — Standard observer encodings (v4.4)
             * Standard observer                            Hex encoding
             * Unknown                                      00000000h
             * CIE 1931 standard colorimetric observer      00000001h
             * CIE 1964 standard colorimetric observer      00000002h
             */

            /*
             * Standard Observer                            Encoded Value
             * unknown                                      00000000h
             * 1931 2 degree Observer                       00000001h
             * 1964 10 degree Observer                      00000002h
             */

            switch (standardObserverHex)
            {
                case "00000000":
                    standardObserver = "Unknown";
                    break;
                case "00000001":
                    standardObserver = "1931";
                    break;
                case "00000002":
                    standardObserver = "1964";
                    break;
            }

            // nCIEXYZ tristimulus values for measurement backing
            // 12 to 23
            var tristimulus = IccXyzType.Parse(bytes.Skip(12).Take(12).ToArray());

            // Encoded value for measurement geometry
            // 24 to 27
            byte[] measurementGeometry = bytes.Skip(24).Take(4).ToArray();
            string measurementGeometryHex = BitConverter.ToString(measurementGeometry).Replace("-", string.Empty);
            /*
             * Table 51 — Measurement geometry encodings
             * Geometry             Hex encoding
             * Unknown              00000000h
             * 0°:45° or 45°:0°     00000001h
             * 0°:d or d:0°         00000002h
             */

            // Encoded value for measurement flare
            // 28 to 31
            byte[] measurementFlare = bytes.Skip(28).Take(4).ToArray();
            string measurementFlareHex = BitConverter.ToString(measurementFlare).Replace("-", string.Empty);
            /*
             * Table 52 — Measurement flare encodings
             * Flare                 Hex encoding
             * 0 (0 %)               00000000h
             * 1,0 (or 100 %)        00010000h
             */

            // Encoded value for standard illuminant
            // 32 to 35
            byte[] standardIlluminantBytes = bytes.Skip(32).Take(4).ToArray();
            string standardIlluminantHex = BitConverter.ToString(standardIlluminantBytes).Replace("-", string.Empty);
            string standardIlluminant = string.Empty;

            switch (standardIlluminantHex)
            {
                case "00000000":
                    standardIlluminant = "Standard illuminant";
                    break;
                case "00000001":
                    standardIlluminant = "D50";
                    break;
                case "00000002":
                    standardIlluminant = "D65";
                    break;
                case "00000003":
                    standardIlluminant = "D93";
                    break;
                case "00000004":
                    standardIlluminant = "F2";
                    break;
                case "00000005":
                    standardIlluminant = "D55";
                    break;
                case "00000006":
                    standardIlluminant = "A";
                    break;
                case "00000007":
                    standardIlluminant = "Equi-Power (E)";
                    break;
                case "00000008":
                    standardIlluminant = "F8";
                    break;
            }
            /*
             * Table 53 — Standard illuminant encodings
             *  Standard illuminant      Hex encoding
             *  Unknown                  00000000h
             *  D50                      00000001h
             *  D65                      00000002h
             *  D93                      00000003h
             *  F2                       00000004h
             *  D55                      00000005h
             *  A                        00000006h
             *  Equi-Power (E)           00000007h
             *  F8                       00000008h
             */

            return new IccMeasurementType(tristimulus,
                standardObserver, measurementGeometryHex,
                measurementFlareHex, standardIlluminant,
                bytes); // TODO bytes exact count
        }
    }
}
