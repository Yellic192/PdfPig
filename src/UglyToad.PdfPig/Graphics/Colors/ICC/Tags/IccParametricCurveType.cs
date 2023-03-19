using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public sealed class IccParametricCurveType : IccBaseCurveType
    {
        /// <summary>
        /// TODO
        /// </summary>
        public ushort FunctionType { get; }

        private readonly Func<double, double> _func;
        private readonly double _g;
        private readonly double _a;
        private readonly double _b;
        private readonly double _c;
        private readonly double _d;
        private readonly double _e;
        private readonly double _f;

        private IccParametricCurveType(ushort functionType, float[] values,
            byte[] rawData, int readBytes)
            : base("para", values, rawData, readBytes)
        {
            FunctionType = functionType;

            switch (FunctionType)
            {
                case 0:
                    _g = Values[0];
                    _func = new Func<double, double>(x => Math.Pow(x, _g));
                    break;

                case 1:
                    _g = Values[0];
                    _a = Values[1];
                    _b = Values[2];
                    _func = new Func<double, double>(x =>
                    {
                        if (x >= -_b / _a)
                        {
                            return Math.Pow(_a * x + _b, _g);
                        }
                        return 0.0;
                    });
                    break;

                case 2:
                    _g = Values[0];
                    _a = Values[1];
                    _b = Values[2];
                    _c = Values[3];
                    _func = new Func<double, double>(x =>
                    {
                        if (x >= -_b / _a)
                        {
                            return Math.Pow(_a * x + _b, _g) + _c;
                        }
                        return _c;
                    });
                    break;

                case 3:
                    _g = Values[0];
                    _a = Values[1];
                    _b = Values[2];
                    _c = Values[3];
                    _d = Values[4];
                    _func = new Func<double, double>(x =>
                    {
                        if (x >= _d)
                        {
                            return Math.Pow(_a * x + _b, _g);
                        }
                        return _c * x;
                    });
                    break;

                case 4:
                    _g = Values[0];
                    _a = Values[1];
                    _b = Values[2];
                    _c = Values[3];
                    _d = Values[4];
                    _e = Values[5];
                    _f = Values[6];
                    _func = new Func<double, double>(x =>
                    {
                        if (x >= _d)
                        {
                            return Math.Pow(_a * x + _b, _g) + _e;
                        }
                        return _c * x + _f;
                    });
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
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
            string[] names = new string[] { "g", "a", "b", "c", "d", "e", "f" };
            string str = "(";

            int i = 0;
            for (i = 0; i < Values.Length - 1; i++)
            {
                str += $"{names[i]}={Math.Round(Values[i], 4)},";
            }

            str += $"{names[i]}={Math.Round(Values[i], 4)}";

            return str + ")";
        }

        /// <summary>
        /// TODO
        /// </summary>
        public new static IccParametricCurveType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);

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
            float[] parameters = IccTagsHelper.Reads15Fixed16Array(parametersBytes);
            //float[] parameters = new float[paramCount];
            //for (int p = 0; p < paramCount; p++)
            //{
            //    byte[] localBytes = parametersBytes.Skip(p * 4).Take(4).ToArray();
            //    parameters[p] = IccTagsHelper.Reads15Fixed16Number(localBytes);
            //}

            int readBytes = 12 + fieldLength;
            return new IccParametricCurveType(functionType, parameters, bytes.Take(readBytes).ToArray(), readBytes);
        }
    }
}
