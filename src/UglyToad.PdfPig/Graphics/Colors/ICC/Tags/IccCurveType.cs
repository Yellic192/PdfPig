using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public sealed class IccCurveType : IccBaseCurveType
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string CurveType { get; } // Should be enum

        private readonly Func<double, double> _func;
        private readonly float _gamma;

        private IccCurveType(float[] values, byte[] rawData, int bytesRead)
            : base("curv", values, rawData, bytesRead)
        {
            switch (values.Length)
            {
                case 0:
                    CurveType = "Identity";
                    _func = new Func<double, double>(x => x);
                    break;

                case 1:
                    CurveType = "Gamma";
                    _gamma = Values[0];
                    _func = new Func<double, double>(x => Math.Pow(x, _gamma));
                    break;

                default:
                    CurveType = "LinearInterpolation";
                    _func = new Func<double, double>(x =>
                    {
                        // Interpolate
                        double index = (Values.Length - 1.0) * x;

                        bool hasDecimal = Math.Abs(index % 1) > (double.Epsilon * 100);
                        if (!hasDecimal)
                        {
                            return Values[(int)index];
                        }

                        int indexInt = (int)Math.Floor(index);
                        double w = index - indexInt;
                        double y1 = Values[indexInt];
                        double y2 = Values[indexInt + 1];
                        return y1 + w * (y2 - y1);
                    });
                    break;
            }
        }

        /// <inheritdoc/>
        public override double Compute(double values)
        {
            return _func(values);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            switch (CurveType)
            {
                case "Identity":
                    return CurveType;

                default:
                    if (Values.Length > 1)
                    {
                        return $"{CurveType} ({Values.Length} points)";
                    }

                    return $"{CurveType} ({Math.Round(Values[0], 4)})";
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public new static IccCurveType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);

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
