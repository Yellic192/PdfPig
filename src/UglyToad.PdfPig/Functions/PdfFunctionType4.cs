namespace UglyToad.PdfPig.Functions
{
    using System;
    using System.Linq;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Functions.Type4;
    using UglyToad.PdfPig.Tokens;

    internal class PdfFunctionType4 : PdfFunction
    {
        private readonly Operators operators = new Operators();
        private readonly InstructionSequence instructions; // TODO

        public PdfFunctionType4(StreamToken function) : base(function)
        {
            byte[] bytes = getPDStream().Data.ToArray(); //.toByteArray();
            //String string = new String(bytes, StandardCharsets.ISO_8859_1);
            //this.instructions = InstructionSequenceBuilder.parse(string);

            string str = OtherEncodings.Iso88591.GetString(bytes);
            this.instructions = InstructionSequenceBuilder.parse(str); // TODO
        }

        public override float[] eval(float[] input)
        {
            //Setup the input values
            ExecutionContext context = new ExecutionContext(operators);
            for (int i = 0; i < input.Length; i++)
            {
                PDRange domain = getDomainForInput(i);
                float value = clipToRange(input[i], domain.getMin(), domain.getMax());
                context.getStack().Push(value);
            }

            //Execute the type 4 function.
            instructions.execute(context);

            //Extract the output values
            int numberOfOutputValues = getNumberOfOutputParameters();
            int numberOfActualOutputValues = context.getStack().Count;
            if (numberOfActualOutputValues < numberOfOutputValues)
            {
                throw new ArgumentOutOfRangeException("The type 4 function returned "
                        + numberOfActualOutputValues
                        + " values but the Range entry indicates that "
                        + numberOfOutputValues + " values be returned.");
            }
            float[] outputValues = new float[numberOfOutputValues];
            for (int i = numberOfOutputValues - 1; i >= 0; i--)
            {
                PDRange range = getRangeForOutput(i);
                outputValues[i] = context.popReal();
                outputValues[i] = clipToRange(outputValues[i], range.getMin(), range.getMax());
            }

            //Return the resulting array
            return outputValues;
        }

        public override int getFunctionType()
        {
            return 4;
        }
    }
}
