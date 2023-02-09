namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal class ExecutionContext
    {
        private readonly Operators operators;
        private Stack<object> stack = new Stack<object>();

        /**
         * Creates a new execution context.
         * @param operatorSet the operator set
         */
        public ExecutionContext(Operators operatorSet)
        {
            this.operators = operatorSet;
        }

        /**
         * Returns the stack used by this execution context.
         * @return the stack
         */
        public Stack<object> getStack()
        {
            return this.stack;
        }

        public void SetStack(Stack<object> stack)
        {
            this.stack = stack;
        }

        /**
         * Returns the operator set used by this execution context.
         * @return the operator set
         */
        public Operators getOperators()
        {
            return this.operators;
        }

        /**
         * Pops a number (int or real) from the stack. If it's neither data type, a
         * ClassCastException is thrown.
         * @return the number
         */
        public object popNumber()
        {
            return stack.Pop();
        }

        /**
         * Pops a value of type int from the stack. If the value is not of type int, a
         * ClassCastException is thrown.
         * @return the int value
         */
        public int popInt()
        {
            return (int)stack.Pop();
        }

        /**
         * Pops a number from the stack and returns it as a real value. If the value is not of a
         * numeric type, a ClassCastException is thrown.
         * @return the real value
         */
        public float popReal()
        {
            return (float)stack.Pop();
        }
    }
}
