namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /**
     * Provides the conditional operators such as "if" and "ifelse".
     *
     */
    class ConditionalOperators
    {
        private ConditionalOperators()
        {
            // Private constructor.
        }

        /** Implements the "if" operator. */
        internal class If : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                InstructionSequence proc = (InstructionSequence)stack.Pop();
                Boolean condition = (Boolean)stack.Pop();
                if (condition)
                {
                    proc.execute(context);
                }
            }
        }

        /** Implements the "ifelse" operator. */
        internal class IfElse : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                InstructionSequence proc2 = (InstructionSequence)stack.Pop();
                InstructionSequence proc1 = (InstructionSequence)stack.Pop();
                Boolean condition = (Boolean)stack.Pop();
                if (condition)
                {
                    proc1.execute(context);
                }
                else
                {
                    proc2.execute(context);
                }
            }
        }
    }
}