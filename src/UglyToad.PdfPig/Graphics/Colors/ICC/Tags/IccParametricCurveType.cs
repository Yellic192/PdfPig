namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO
    /// </summary>
    public sealed class IccParametricCurveType : BaseCurveType
    {
        private IccParametricCurveType(float[] values, byte[] rawData, int readBytes)
            : base("para", values, rawData, readBytes)
        { }

        /// <summary>
        /// TODO
        /// </summary>
        public new static IccParametricCurveType Parse(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);

            if (typeSignature != "para")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Encoded value of the function type
            // 8 to 9
            ushort functionType = IccTagsHelper.ReadUInt16(bytes.Skip(8).Take(2).ToArray());

            // Reserved, shall be set to 0
            // 10 to 11
            //var reserved2 = bytes.Skip(10).Take(2).ToArray();

            // Table 68 — parametricCurveType function type encoding
            int paramCount;
            switch (functionType)
            {
                case 0:
                    paramCount = 1;
                    break;

                case 1:
                case 2:
                case 3:
                    paramCount = functionType + 2;
                    break;

                case 4:
                    paramCount = 7;
                    break;

                default:
                    throw new InvalidOperationException($"{functionType}");
            }

            int fieldLength = paramCount * 4;

            // One or more parameters (see Table 67)
            // 12 to end
            byte[] parametersBytes = bytes.Skip(12).Take(fieldLength).ToArray();
            float[] parameters = new float[paramCount];
            for (int p = 0; p < paramCount; p++)
            {
                parameters[p] = IccTagsHelper.Reads15Fixed16Number(parametersBytes, p * 4);
            }

            int readBytes = 12 + fieldLength;
            return new IccParametricCurveType(parameters, bytes.Take(readBytes).ToArray(), readBytes);
        }
    }
}
