namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;
    using System.Linq;
    using System.Text;

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

        private IccMeasurementType(string standardObserver, string measurementGeometry,
            string measurementFlare, string standardIlluminant, byte[] rawData)
        {
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
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4); // meas

            if (typeSignature != "meas")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Encoded value for standard observer
            // 8 to 11
            byte[] standardObserver = bytes.Skip(8).Take(4).ToArray();
            string standardObserverHex = BitConverter.ToString(standardObserver).Replace("-", string.Empty);
            /*
             * Table 50 — Standard observer encodings
             * Standard observer                            Hex encoding
             * Unknown                                      00000000h
             * CIE 1931 standard colorimetric observer      00000001h
             * CIE 1964 standard colorimetric observer      00000002h
             */

            // nCIEXYZ tristimulus values for measurement backing
            // 12 to 23
            var tristimulus = IccXyzType.Parse(bytes.Skip(12).Take(12).ToArray());

            // Encoded value for measurement geometry
            // 24 to 27
            byte[] measurementGeometry = bytes.Skip(24).Take(4).ToArray();
            string measurementGeometryHex = BitConverter.ToString(measurementGeometry).Replace("-", string.Empty);
            measurementGeometryHex = string.Concat(measurementGeometry.Select(b => b.ToString("X")).ToArray());
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
            measurementFlareHex = string.Concat(measurementFlare.Select(b => b.ToString("X")).ToArray());
            /*
             * Table 52 — Measurement flare encodings
             * Flare                 Hex encoding
             * 0 (0 %)               00000000h
             * 1,0 (or 100 %)        00010000h
             */

            // Encoded value for standard illuminant
            // 32 to 35
            byte[] standardIlluminant = bytes.Skip(32).Take(4).ToArray();
            string standardIlluminantHex = BitConverter.ToString(standardIlluminant).Replace("-", string.Empty);
            standardIlluminantHex = string.Concat(standardIlluminant.Select(b => b.ToString("X")).ToArray());

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

            return new IccMeasurementType(standardObserverHex, measurementGeometryHex, measurementFlareHex, standardIlluminantHex, bytes); // TODO bytes exact count
        }
    }
}
