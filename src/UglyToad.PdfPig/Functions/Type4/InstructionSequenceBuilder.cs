namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /**
     * Basic parser for Type 4 functions which is used to build up instruction sequences.
     *
     */
    internal class InstructionSequenceBuilder : Parser.AbstractSyntaxHandler
    {
        private static readonly Regex INTEGER_PATTERN = new Regex("[\\+\\-]?\\d+", RegexOptions.Compiled);
        private static readonly Regex REAL_PATTERN = new Regex("[\\-]?\\d*\\.\\d*([Ee]\\-?\\d+)?", RegexOptions.Compiled);

        private readonly InstructionSequence mainSequence = new InstructionSequence();
        private readonly Stack<InstructionSequence> seqStack = new Stack<InstructionSequence>();

        private InstructionSequenceBuilder()
        {
            this.seqStack.Push(this.mainSequence);
        }

        /**
         * Returns the instruction sequence that has been build from the syntactic elements.
         * @return the instruction sequence
         */
        public InstructionSequence getInstructionSequence()
        {
            return this.mainSequence;
        }

        /**
         * Parses the given text into an instruction sequence representing a Type 4 function
         * that can be executed.
         * @param text the Type 4 function text
         * @return the instruction sequence
         */
        public static InstructionSequence parse(string text)
        {
            InstructionSequenceBuilder builder = new InstructionSequenceBuilder();
            Parser.parse(text, builder);
            return builder.getInstructionSequence();
        }

        private InstructionSequence getCurrentSequence()
        {
            return this.seqStack.Peek();
        }

        /** {@inheritDoc} */
        public void token(char[] text)
        {
            String val = string.Concat(text);
            token(val);
        }

        public override void token(string token)
        {
            if ("{".Equals(token))
            {
                InstructionSequence child = new InstructionSequence();
                getCurrentSequence().addProc(child);
                this.seqStack.Push(child);
            }
            else if ("}".Equals(token))
            {
                this.seqStack.Pop();
            }
            else
            {
                Match m = INTEGER_PATTERN.Match(token);
                if (m.Success)
                {
                    getCurrentSequence().addInteger(parseInt(token));
                    return;
                }

                m = REAL_PATTERN.Match(token);
                if (m.Success)
                {
                    getCurrentSequence().addReal(parseReal(token));
                    return;
                }

                //TODO Maybe implement radix numbers, such as 8#1777 or 16#FFFE

                getCurrentSequence().addName(token);
            }
        }

        /**
         * Parses a value of type "int".
         * @param token the token to be parsed
         * @return the parsed value
         */
        public static int parseInt(String token)
        {
            return int.Parse(token);
        }

        /**
         * Parses a value of type "real".
         * @param token the token to be parsed
         * @return the parsed value
         */
        public static float parseReal(String token)
        {
            return float.Parse(token);
        }
    }
}
