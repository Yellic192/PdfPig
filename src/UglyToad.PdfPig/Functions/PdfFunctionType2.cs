namespace UglyToad.PdfPig.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UglyToad.PdfPig.Tokens;

    internal class PdfFunctionType2 : PdfFunction
    {
        /**
 * The C0 values of the exponential function.
 */
        private readonly ArrayToken c0;
        /**
         * The C1 values of the exponential function.
         */

        private readonly ArrayToken c1;
        /**
         * The exponent value of the exponential function.
         */
        private readonly float exponent;

        public PdfFunctionType2(DictionaryToken function) : base(function)
        {
            if (getCOSObject().TryGet(NameToken.C0, out ArrayToken cosArray0))
            {
                c0 = cosArray0;
            }
            else
            {
                c0 = new ArrayToken(new List<IToken>());
            }
            if (c0.Length == 0)
            {
                c0 = new ArrayToken(new List<NumericToken>() { new NumericToken(0) });
            }

            if (getCOSObject().TryGet(NameToken.C1, out ArrayToken cosArray1))
            {
                c1 = cosArray1;
            }
            else
            {
                c1 = new ArrayToken(new List<IToken>());
            }
            if (c0.Length == 0)
            {
                c1 = new ArrayToken(new List<NumericToken>() { new NumericToken(1) });
            }

            if (getCOSObject().TryGet(NameToken.N, out NumericToken exp))
            {
                exponent = (float)exp.Double;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public PdfFunctionType2(StreamToken function) : base(function)
        {
            if (getCOSObject().TryGet(NameToken.C0, out ArrayToken cosArray0))
            {
                c0 = cosArray0;
            }
            else
            {
                c0 = new ArrayToken(new List<IToken>());
            }
            if (c0.Length == 0)
            {
                c0 = new ArrayToken(new List<NumericToken>() { new NumericToken(0) });
            }

            if (getCOSObject().TryGet(NameToken.C1, out ArrayToken cosArray1))
            {
                c1 = cosArray1;
            }
            else
            {
                c1 = new ArrayToken(new List<IToken>());
            }
            if (c0.Length == 0)
            {
                c1 = new ArrayToken(new List<NumericToken>() { new NumericToken(1) });
            }

            if (getCOSObject().TryGet(NameToken.N, out NumericToken exp))
            {
                exponent = (float)exp.Double;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override int getFunctionType()
        {
            return 2;
        }

        public override float[] eval(float[] input)
        {
            // exponential interpolation
            float xToN = (float)Math.Pow(input[0], exponent); // x^exponent

            float[] result = new float[Math.Min(c0.Length, c1.Length)];
            for (int j = 0; j < result.Length; j++)
            {
                float c0j = (float)((NumericToken)c0[j]).Double;
                float c1j = (float)((NumericToken)c1[j]).Double;
                result[j] = c0j + xToN * (c1j - c0j);
            }

            return clipToRange(result);
        }

        /**
    * Returns the C0 values of the function, 0 if empty.
    *
    * @return a COSArray with the C0 values
    */
        public ArrayToken getC0()
        {
            return c0;
        }

        /**
    * Returns the C1 values of the function, 1 if empty.
    *
    * @return a COSArray with the C1 values
    */
        public ArrayToken getC1()
        {
            return c1;
        }

        /**
 * Returns the exponent of the function.
 *
 * @return the float value of the exponent
 */
        public float getN()
        {
            return exponent;
        }

        public override string ToString()
        {
            return "FunctionType2{"
                + "C0: " + getC0() + " "
                + "C1: " + getC1() + " "
                + "N: " + getN() + "}";
        }
    }
}
