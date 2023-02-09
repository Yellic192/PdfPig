namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/type4/InstructionSequence.java

    internal class InstructionSequence
    {
        private readonly List<Object> instructions = new List<object>();

        /**
 * Add a name (ex. an operator)
 * @param name the name
 */
        public void addName(String name)
        {
            this.instructions.Add(name);
        }

        /**
   * Adds an int value.
   * @param value the value
   */
        public void addInteger(int value)
        {
            this.instructions.Add(value);
        }

        /**
         * Adds a real value.
         * @param value the value
         */
        public void addReal(float value)
        {
            this.instructions.Add(value);
        }

        /**
         * Adds a bool value.
         * @param value the value
         */
        public void addBoolean(bool value)
        {
            this.instructions.Add(value);
        }

        /**
         * Adds a proc (sub-sequence of instructions).
         * @param child the child proc
         */
        public void addProc(InstructionSequence child)
        {
            this.instructions.Add(child);
        }

        /**
         * Executes the instruction sequence.
         * @param context the execution context
         */
        public void execute(ExecutionContext context)
        {
            Stack<Object> stack = context.getStack();
            foreach (Object o in instructions)
            {
                if (o is String name)
                {
                    Operator cmd = context.getOperators().getOperator(name);
                    if (cmd != null)
                    {
                        cmd.execute(context);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown operator or name: " + name);
                    }
                }
                else
                {
                    stack.Push(o);
                }
            }

            //Handles top-level procs that simply need to be executed
            while (stack.Any() && stack.Peek() is InstructionSequence) // !stack.isEmpty() ...
            {
                InstructionSequence nested = (InstructionSequence)stack.Pop();
                nested.execute(context);
            }
        }
    }
}
