using System;

namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static UglyToad.PdfPig.Functions.Type4.ArithmeticOperators;

    /**
 * Provides the relational operators such as "eq" and "le".
 *
 */
    class RelationalOperators
    {
        private RelationalOperators()
        {
            // Private constructor.
        }

        /** Implements the "eq" operator. */
        internal class Eq : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                Object op2 = stack.Pop();
                Object op1 = stack.Pop();
                bool result = isEqual(op1, op2);
                stack.Push(result);
            }

            protected virtual bool isEqual(Object op1, Object op2)
            {
                bool result;
                /*
                if (op1 is Number && op2 is Number)
                {
                    Number num1 = (Number)op1;
                    Number num2 = (Number)op2;
                    result = Float.compare(num1.floatValue(), num2.floatValue()) == 0;
                }
                */
                if (op1 is float num1 && op2 is float num2)
                {
                    result = num1.Equals(num2);
                }
                else
                {
                    result = op1.Equals(op2);
                }
                return result;
            }
        }

        /** Abstract base class for number comparison operators. */
        internal abstract class AbstractNumberComparisonOperator : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<Object> stack = context.getStack();
                Object op2 = stack.Pop();
                Object op1 = stack.Pop();
                var num1 = (float)op1; // Number
                var num2 = (float)op2; // Number
                bool result = compare(num1, num2);
                stack.Push(result);
            }

            protected abstract bool compare(float num1, float num2);
        }

        /** Implements the "ge" operator. */
        internal class Ge : AbstractNumberComparisonOperator
        {
            protected override bool compare(float num1, float num2)
            {
                return num1 >= num2;
            }
        }

        /** Implements the "gt" operator. */
        internal class Gt : AbstractNumberComparisonOperator
        {
            protected override bool compare(float num1, float num2)
            {
                return num1 > num2;
            }
        }

        /** Implements the "le" operator. */
        internal class Le : AbstractNumberComparisonOperator
        {
            protected override bool compare(float num1, float num2)
            {
                return num1 <= num2;
            }
        }

        /** Implements the "lt" operator. */
        internal class Lt : AbstractNumberComparisonOperator
        {
            protected override bool compare(float num1, float num2)
            {
                return num1 < num2;
            }
        }

        /** Implements the "ne" operator. */
        internal class Ne : Eq
        {
            protected override bool isEqual(Object op1, Object op2)
            {
                bool result = base.isEqual(op1, op2);
                return !result;
            }
        }
    }
}
