namespace UglyToad.PdfPig.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UglyToad.PdfPig.Tokens;

    internal class PDRange
    {
        private ArrayToken rangeArray;
        private int startingIndex;

        //private List<NumericToken> rawData;

        /*
        public PDRange()
        {
            rangeArray = new ArrayToken();
            rawData.Add(new NumericToken(0m)); //rangeArray.add(new COSFloat(0.0f));
            rawData.Add(new NumericToken(1m)); //rangeArray.add(new COSFloat(1.0f));
            startingIndex = 0;
        }
        */

        public PDRange(ArrayToken range)
        {
            rangeArray = range;
        }

        /**
  * Constructor assumes a starting index of 0.
  *
  * @param range The array that describes the range.
  */
        public PDRange(IReadOnlyList<decimal> range)
            : this(new ArrayToken(range.Select(d => new NumericToken(d)).ToArray()))
        {
        }

        public PDRange(ArrayToken range, int index)
        {
            rangeArray = range;
            startingIndex = index;
        }

        /**
 * Constructor with an index into an array.  Because some arrays specify
 * multiple ranges ie [ 0,1,  0,2,  2,3 ] It is convenient for this
 * class to take an index into an array.  So if you want this range to
 * represent 0,2 in the above example then you would say <code>new PDRange( array, 1 )</code>.
 *
 * @param range The array that describes the index
 * @param index The range index into the array for the start of the range.
 */
        public PDRange(IReadOnlyList<decimal> range, int index)
            : this(new ArrayToken(range.Select(d => new NumericToken(d)).ToArray()), index)
        {
        }

        /**
    * Convert this standard java object to a COS object.
    *
    * @return The cos object that matches this Java object.
    */
        //@Override
        public IToken getCOSObject()
        {
            return rangeArray;
        }

        /**
         * This will get the underlying array value.
         *
         * @return The cos object that this object wraps.
         */
        public ArrayToken getCOSArray()
        {
            return rangeArray;
        }

        /**
         * This will get the minimum value of the range.
         *
         * @return The min value.
         */
        public float getMin()
        {
            NumericToken min = rangeArray[startingIndex * 2] as NumericToken; //getObject(startingIndex * 2);
            return (float)min.Double; //.floatValue();
        }

 
        /**
         * This will get the maximum value of the range.
         *
         * @return The max value.
         */
        public float getMax()
        {
            NumericToken max = rangeArray[startingIndex * 2 + 1] as NumericToken; //.getObject(startingIndex * 2 + 1);
            return (float)max.Double; //.floatValue();
        }



    }
}
