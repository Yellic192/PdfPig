namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;

    class BitwiseOperators
    {
        private BitwiseOperators()
        {
            // Private constructor.
        }

        /** Abstract base class for logical operators. */
        internal abstract class AbstractLogicalOperator : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                Object op2 = stack.Pop();
                Object op1 = stack.Pop();
                if (op1 is Boolean && op2 is Boolean)
                {
                    bool bool1 = (Boolean)op1;
                    bool bool2 = (Boolean)op2;
                    bool result = applyForBoolean(bool1, bool2);
                    stack.Push(result);
                }
                else if (op1 is int && op2 is int)
                {
                    int int1 = (int)op1;
                    int int2 = (int)op2;
                    int result = applyforint(int1, int2);
                    stack.Push(result);
                }
                else
                {
                    throw new InvalidCastException("Operands must be bool/bool or int/int");
                }
            }

            protected abstract bool applyForBoolean(bool bool1, bool bool2);

            protected abstract int applyforint(int int1, int int2);
        }

        /** Implements the "and" operator. */
        internal class And : AbstractLogicalOperator
        {
            protected override bool applyForBoolean(bool bool1, bool bool2)
            {
                return bool1 && bool2;
            }

            protected override int applyforint(int int1, int int2)
            {
                return int1 & int2;
            }
        }

        /** Implements the "bitshift" operator. */
        internal class Bitshift : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                int shift = (int)stack.Pop();
                int int1 = (int)stack.Pop();
                if (shift < 0)
                {
                    int result = int1 >> Math.Abs(shift);
                    stack.Push(result);
                }
                else
                {
                    int result = int1 << shift;
                    stack.Push(result);
                }
            }
        }

        /** Implements the "false" operator. */
        internal class False : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                stack.Push(false);
            }
        }

        /** Implements the "not" operator. */
        internal class Not : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                Object op1 = stack.Pop();
                if (op1 is Boolean)
                {
                    bool bool1 = (Boolean)op1;
                    bool result = !bool1;
                    stack.Push(result);
                }
                else if (op1 is int)
                {
                    int int1 = (int)op1;
                    int result = -int1;
                    stack.Push(result);
                }
                else
                {
                    throw new InvalidCastException("Operand must be bool or int");
                }
            }
        }

        /** Implements the "or" operator. */
        internal class Or : AbstractLogicalOperator
        {
            protected override bool applyForBoolean(bool bool1, bool bool2)
            {
                return bool1 || bool2;
            }

            protected override int applyforint(int int1, int int2)
            {
                return int1 | int2;
            }
        }

        /** Implements the "true" operator. */
        internal class True : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                stack.Push(true);
            }
        }

        /** Implements the "xor" operator. */
        internal class Xor : AbstractLogicalOperator
        {
            protected override bool applyForBoolean(bool bool1, bool bool2)
            {
                return bool1 ^ bool2;
            }

            protected override int applyforint(int int1, int int2)
            {
                return int1 ^ int2;
            }
        }
    }
}
