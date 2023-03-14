namespace UglyToad.PdfPig.Graphics.Colors.ICC.Tags
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO
    /// </summary>
    public sealed class IccCurveType : BaseCurveType
    {
        private IccCurveType(float[] values, byte[] rawData, int bytesRead)
            : base("curv", values, rawData, bytesRead)
        { }

        /// <summary>
        /// TODO
        /// </summary>
        public new static IccCurveType Parse(byte[] bytes)
        {
            string typeSignature = Encoding.ASCII.GetString(bytes, 0, 4);

            if (typeSignature != "curv")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Count value specifying the number of entries (n) that follow
            // 8 to 11
            uint count = IccTagsHelper.ReadUInt32(bytes.Skip(8).Take(4).ToArray());

            float[] values = new float[count]; // or double?

            int readBytes;

            // Actual curve values starting with the zeroth entry and ending with the entry n 1
            // 12 to end
            // The curveType embodies a one-dimensional function which maps an input value in the domain of the function
            // to an output value in the range of the function.The domain and range values are in the range of 0,0 to 1,0.
            // - When n is equal to 0, an identity response is assumed.
            // - When n is equal to 1, then the curve value shall be interpreted as a gamma value, encoded as
            // u8Fixed8Number. Gamma shall be interpreted as the exponent in the equation y = x^g and not as an inverse.
            // - When n is greater than 1, the curve values(which embody a sampled one - dimensional function) shall be
            // defined as follows:
            //      - The first entry represents the input value 0,0, the last entry represents the input value 1,0, and intermediate
            //      entries are uniformly spaced using an increment of 1,0 / (n-1). These entries are encoded as uInt16Numbers
            //      (i.e. the values represented by the entries, which are in the range 0,0 to 1,0 are encoded in the range 0 to
            //      65 535). Function values between the entries shall be obtained through linear interpolation.
            if (count == 0)
            {
                // When n is equal to 0, an identity response is assumed.
                readBytes = 12;
            }
            else if (count == 1)
            {
                // When n is equal to 1, then the curve value shall be interpreted as a gamma value, encoded as
                // u8Fixed8Number. Gamma shall be interpreted as the exponent in the equation y = x^g and not as an inverse.
                // * If n = 1, the field length is 2 bytes and the value is encoded as a u8Fixed8Number
                readBytes = 12 + 2;
            }
            else
            {
                // When n is greater than 1, the curve values(which embody a sampled one - dimensional function) shall be
                // defined as follows:
                // The first entry represents the input value 0,0, the last entry represents the input value 1,0, and intermediate
                // entries are uniformly spaced using an increment of 1,0 / (n-1). These entries are encoded as uInt16Numbers
                // (i.e. the values represented by the entries, which are in the range 0,0 to 1,0 are encoded in the range 0 to
                // 65 535). Function values between the entries shall be obtained through linear interpolation.                
                for (int c = 0; c < count; c++)
                {
                    values[c] = IccTagsHelper.ReadUInt16(bytes.Skip(12 + (2 * c)).Take(2).ToArray()) / 65_535f;
                }

                readBytes = 12 + 2 * (int)count;
            }

            return new IccCurveType(values, bytes.Take(readBytes).ToArray(), readBytes);
        }
    }
}
