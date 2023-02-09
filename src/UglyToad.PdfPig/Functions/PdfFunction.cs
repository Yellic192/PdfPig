namespace UglyToad.PdfPig.Functions
{
    using System;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig.Tokens;

    internal abstract class PdfFunction
    {
        public DictionaryToken FunctionDictionary { get; }

        public StreamToken FunctionStream { get; }

        private ArrayToken domain = null;
        private ArrayToken range = null;
        private int numberOfInputValues = -1;
        private int numberOfOutputValues = -1;

        public PdfFunction(DictionaryToken function)
        {
            FunctionDictionary = function;
        }

        public PdfFunction(StreamToken function)
        {
            // functionStream = new PDStream( (COSStream)function );
            // functionStream.getCOSObject().setItem(COSName.TYPE, COSName.FUNCTION);
            FunctionStream = function;
        }

        /**
         * Returns the function type.
         * 
         * Possible values are:
         * 
         * 0 - Sampled function
         * 2 - Exponential interpolation function
         * 3 - Stitching function
         * 4 - PostScript calculator function
         * 
         * @return the function type.
         */
        public abstract int getFunctionType();

        /**
         * Returns the stream.
         * @return The stream for this object.
         */
        public DictionaryToken getCOSObject()
        {
            if (FunctionStream != null)
            {
                return FunctionStream.StreamDictionary;
            }
            else
            {
                return FunctionDictionary;
            }
        }

        /**
         * Returns the underlying PDStream.
         * @return The stream.
         */
        protected StreamToken getPDStream()
        {
            return FunctionStream;
        }

        public static PdfFunction Create(IToken function)
        {
            if (function is NameToken identity && identity == NameToken.Identity) //COSName.IDENTITY)
            {
                return new PdfFunctionTypeIdentity(null);
            }

            IToken _baseDic = function;

            if (_baseDic is StreamToken _baseStream)
            {
                _baseDic = _baseStream.StreamDictionary;
            }

            if (_baseDic is not DictionaryToken functionDictionary)
            {
                throw new IOException("Error: Function must be a Dictionary, but is " +
                        (_baseDic == null ? "(null)" : _baseDic.GetType().Name)); //.getClass().getSimpleName())); ;
            }

            int functionType = (functionDictionary.Data[NameToken.FunctionType] as NumericToken).Int;
            switch (functionType)
            {
                case 0:
                    if (function is StreamToken function0Stream)
                    {
                        return new PdfFunctionType0(function0Stream);
                    }
                    else
                    {
                        throw new NotImplementedException("PdfFunctionType0 not stream");
                    }
                case 2:
                    return new PdfFunctionType2(functionDictionary);
                case 3:
                    throw new NotImplementedException("PdfFunctionType3");
                //return new PdfFunctionType3(functionDictionary);
                case 4:
                    if (function is StreamToken function4Stream)
                    {
                        return new PdfFunctionType4(function4Stream);
                    }
                    else
                    {
                        throw new NotImplementedException("PdfFunctionType4 not stream");
                    }
                default:
                    throw new IOException("Error: Unknown function type " + functionType);
            }
        }

        /**
 * This will get the number of output parameters that
 * have a range specified.  A range for output parameters
 * is optional so this may return zero for a function
 * that does have output parameters, this will simply return the
 * number that have the range specified.
 *
 * @return The number of output parameters that have a range
 * specified.
 */
        public int getNumberOfOutputParameters()
        {
            if (numberOfOutputValues == -1)
            {
                ArrayToken rangeValues = getRangeValues();
                if (rangeValues == null)
                {
                    numberOfOutputValues = 0;
                }
                else
                {
                    numberOfOutputValues = rangeValues.Length / 2;
                }
            }
            return numberOfOutputValues;
        }

        /**
  * This will get the range for a certain output parameters.  This is will never
  * return null.  If it is not present then the range 0 to 0 will
  * be returned.
  *
  * @param n The output parameter number to get the range for.
  *
  * @return The range for this component.
  */
        public PDRange getRangeForOutput(int n)
        {
            ArrayToken rangeValues = getRangeValues();
            return new PDRange(rangeValues, n);
        }

        /**
  * This will set the range values.
  *
  * @param rangeValues The new range values.
  */
        public void setRangeValues(ArrayToken rangeValues)
        {
            range = rangeValues;
            //getCOSObject().setItem(COSName.RANGE, rangeValues);
            throw new NotImplementedException();
        }

        /**
         * This will get the number of input parameters that
         * have a domain specified.
         *
         * @return The number of input parameters that have a domain
         * specified.
         */
        public int getNumberOfInputParameters()
        {
            if (numberOfInputValues == -1)
            {
                ArrayToken array = getDomainValues();
                numberOfInputValues = array.Length / 2;
            }
            return numberOfInputValues;
        }

        /**
  * This will get the range for a certain input parameter.  This is will never
  * return null.  If it is not present then the range 0 to 0 will
  * be returned.
  *
  * @param n The parameter number to get the domain for.
  *
  * @return The domain range for this component.
  */
        public PDRange getDomainForInput(int n)
        {
            ArrayToken domainValues = getDomainValues();
            return new PDRange(domainValues, n);
        }

        /**
    * This will set the domain values.
    *
    * @param domainValues The new domain values.
    */
        public void setDomainValues(ArrayToken domainValues)
        {
            domain = domainValues;
            throw new NotImplementedException();
            //getCOSObject().setItem(COSName.DOMAIN, domainValues);
        }


        /**
         * Evaluates the function at the given input.
         * ReturnValue = f(input)
         *
         * @param input The array of input values for the function. 
         * In many cases will be an array of a single value, but not always.
         * 
         * @return The of outputs the function returns based on those inputs. 
         * In many cases will be an array of a single value, but not always.
         * 
         * @throws IOException if something went wrong processing the function.  
         */
        public abstract float[] eval(float[] input);

        /**
   * Returns all ranges for the output values as COSArray .
   * Required for type 0 and type 4 functions
   * @return the ranges array. 
   */
        protected virtual ArrayToken getRangeValues()
        {
            if (range == null)
            {
                if (getCOSObject().TryGet(NameToken.Range, out ArrayToken rangeToken))
                {
                    range = rangeToken;
                }
                else
                {
                    throw new NotImplementedException();
                }
                //range = getCOSObject().getCOSArray(NameToken.Range);//COSName.RANGE);
            }
            return range;
        }

        /**
    * Returns all domains for the input values as COSArray.
    * Required for all function types.
    * @return the domains array. 
    */
        private ArrayToken getDomainValues()
        {
            if (domain == null)
            {
                if (getCOSObject().TryGet(NameToken.Domain, out ArrayToken domainToken))
                {
                    domain = domainToken;
                }
                else
                {
                    throw new NotImplementedException();
                }
                //domain = getCOSObject().getCOSArray(NameToken.Domain); //COSName.DOMAIN);
            }
            return domain;
        }

        /**
 * Clip the given input values to the ranges.
 * 
 * @param inputValues the input values
 * @return the clipped values
 */
        protected float[] clipToRange(float[] inputValues)
        {
            ArrayToken rangesArray = getRangeValues();
            float[] result;
            if (rangesArray != null && rangesArray.Length > 0)
            {
                float[] rangeValues = rangesArray.Data.OfType<NumericToken>().Select(t => (float)t.Double).ToArray(); //.toFloatArray();
                int numberOfRanges = rangeValues.Length / 2;
                result = new float[numberOfRanges];
                for (int i = 0; i < numberOfRanges; i++)
                {
                    int index = i << 1;
                    result[i] = clipToRange(inputValues[i], rangeValues[index], rangeValues[index + 1]);
                }
            }
            else
            {
                result = inputValues;
            }
            return result;
        }

        /**
 * Clip the given input value to the given range.
 * 
 * @param x the input value
 * @param rangeMin the min value of the range
 * @param rangeMax the max value of the range
 * @return the clipped value
 */
        protected float clipToRange(float x, float rangeMin, float rangeMax)
        {
            if (x < rangeMin)
            {
                return rangeMin;
            }
            else if (x > rangeMax)
            {
                return rangeMax;
            }
            return x;
        }

        /**
 * For a given value of x, interpolate calculates the y value 
 * on the line defined by the two points (xRangeMin , xRangeMax ) 
 * and (yRangeMin , yRangeMax ).
 * 
 * @param x the to be interpolated value.
 * @param xRangeMin the min value of the x range
 * @param xRangeMax the max value of the x range
 * @param yRangeMin the min value of the y range
 * @param yRangeMax the max value of the y range
 * @return the interpolated y value
 */
        protected static float interpolate(float x, float xRangeMin, float xRangeMax, float yRangeMin, float yRangeMax)
        {
            return yRangeMin + ((x - xRangeMin) * (yRangeMax - yRangeMin) / (xRangeMax - xRangeMin));
        }
    }
}
