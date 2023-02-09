namespace UglyToad.PdfPig.Functions
{
    using System;
    using UglyToad.PdfPig.Tokens;

    internal class PdfFunctionTypeIdentity : PdfFunction
    {
        public PdfFunctionTypeIdentity(DictionaryToken function) : base((DictionaryToken)null)
        {
            //TODO passing null is not good because getCOSObject() can result in an NPE in the base class
        }

        public override int getFunctionType()
        {
            // shouldn't be called
            throw new NotSupportedException();
            //TODO this is a violation of the interface segregation principle
        }

        public override float[] eval(float[] input)
        {
            return input;
        }

        protected override ArrayToken getRangeValues()
        {
            return null;
        }
    }
}
