namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;

    /**
     * Provides the arithmetic operators such as "add" and "sub".
     *
     */
    class ArithmeticOperators
    {
        private static double toRadians(double val)
        {
            return (Math.PI / 180.0) * val;
        }

        private static double toDegrees(double val)
        {
            return (180.0 / Math.PI) * val;
        }

        private ArithmeticOperators()
        {
            // Private constructor.
        }

        /**  :  the "Abs" operator. */
        internal class Abs : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                if (num is int numi)
                {
                    context.getStack().Push(Math.Abs(numi));
                }
                else
                {
                    context.getStack().Push(Math.Abs((float)num));
                }
            }
        }

        /**  :  the "add" operator. */
        internal class Add : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num2 = context.popNumber();
                var num1 = context.popNumber();
                if (num1 is int num1i && num2 is int num2i)
                {
                    long sum = num1i + num2i;
                    if (sum < int.MinValue || sum > int.MaxValue)
                    {
                        context.getStack().Push((float)sum);
                    }
                    else
                    {
                        context.getStack().Push((int)sum);
                    }
                }
                else
                {
                    float sum = (float)num1 + (float)num2;
                    context.getStack().Push(sum);
                }
            }
        }

        /**  :  the "atan" operator. */
        internal class Atan : Operator
        {
            public void execute(ExecutionContext context)
            {
                float den = context.popReal();
                float num = context.popReal();
                float atan = (float)Math.Atan2(num, den);
                atan = (float)toDegrees(atan) % 360;
                if (atan < 0)
                {
                    atan = atan + 360;
                }
                context.getStack().Push(atan);
            }
        }

        /**  :  the "ceiling" operator. */
        internal class Ceiling : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                if (num is int numi)
                {
                    context.getStack().Push(numi);
                }
                else
                {
                    context.getStack().Push((float)Math.Ceiling((double)num));
                }
            }
        }

        /**  :  the "cos" operator. */
        internal class Cos : Operator
        {
            public void execute(ExecutionContext context)
            {
                float angle = context.popReal();
                float cos = (float)Math.Cos(toRadians(angle));
                context.getStack().Push(cos);
            }
        }

        /**  :  the "cvi" operator. */
        internal class Cvi : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                context.getStack().Push((int)num);
            }
        }

        /**  :  the "cvr" operator. */
        internal class Cvr : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                context.getStack().Push((float)num);
            }
        }

        /**  :  the "div" operator. */
        internal class Div : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num2 = context.popNumber();
                var num1 = context.popNumber();
                context.getStack().Push((float)num1 / (float)num2);
            }
        }

        /**  :  the "exp" operator. */
        internal class Exp : Operator
        {
            public void execute(ExecutionContext context)
            {
                var exp = context.popNumber();
                var base_ = context.popNumber();
                double value = Math.Pow((double)base_, (double)exp);
                context.getStack().Push((float)value);
            }
        }

        /**  :  the "floor" operator. */
        internal class Floor : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                if (num is int numi)
                {
                    context.getStack().Push(numi);
                }
                else
                {
                    context.getStack().Push((float)Math.Floor((double)num));
                }
            }
        }

        /**  :  the "idiv" operator. */
        internal class IDiv : Operator
        {
            public void execute(ExecutionContext context)
            {
                int num2 = context.popInt();
                int num1 = context.popInt();
                context.getStack().Push(num1 / num2);
            }
        }

        /**  :  the "ln" operator. */
        internal class Ln : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                context.getStack().Push((float)Math.Log((double)num));
            }
        }

        /**  :  the "log" operator. */
        internal class Log : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                context.getStack().Push((float)Math.Log10((double)num));
            }
        }

        /**  :  the "mod" operator. */
        internal class Mod : Operator
        {
            public void execute(ExecutionContext context)
            {
                int int2 = context.popInt();
                int int1 = context.popInt();
                context.getStack().Push(int1 % int2);
            }
        }

        /**  :  the "mul" operator. */
        internal class Mul : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num2 = context.popNumber();
                var num1 = context.popNumber();
                if (num1 is int num1i && num2 is int num2i)
                {
                    long result = num1i * num2i;
                    if (result >= int.MinValue && result <= int.MaxValue)
                    {
                        context.getStack().Push((int)result);
                    }
                    else
                    {
                        context.getStack().Push((float)result);
                    }
                }
                else
                {
                    float result = Convert.ToSingle(num1) * Convert.ToSingle(num2);
                    context.getStack().Push(result);
                }
            }
        }

        /**  :  the "neg" operator. */
        internal class Neg : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                if (num is int v)
                {
                    if (v == int.MinValue)
                    {
                        context.getStack().Push(-(float)num);
                    }
                    else
                    {
                        context.getStack().Push(-(int)num);
                    }
                }
                else
                {
                    context.getStack().Push(-(float)num);
                }
            }
        }

        /**  :  the "round" operator. */
        internal class Round : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                if (num is int numi)
                {
                    context.getStack().Push(numi);
                }
                else
                {
                    context.getStack().Push((float)Math.Round((double)num));
                }
            }
        }

        /**  :  the "sin" operator. */
        internal class Sin : Operator
        {
            public void execute(ExecutionContext context)
            {
                float angle = context.popReal();
                float sin = (float)Math.Sin(toRadians(angle));
                context.getStack().Push(sin);
            }
        }

        /**  :  the "sqrt" operator. */
        internal class Sqrt : Operator
        {
            public void execute(ExecutionContext context)
            {
                float num = context.popReal();
                if (num < 0)
                {
                    throw new ArgumentException("argument must be nonnegative");
                }
                context.getStack().Push((float)Math.Sqrt(num));
            }
        }

        /**  :  the "sub" operator. */
        internal class Sub : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                var num2 = context.popNumber();
                var num1 = context.popNumber();
                if (num1 is int num1i && num2 is int num2i)
                {
                    long result = num1i - num2i;
                    if (result < int.MinValue || result > int.MaxValue)
                    {
                        stack.Push((float)result);
                    }
                    else
                    {
                        stack.Push((int)result);
                    }
                }
                else
                {
                    float result = (float)num1 - (float)num2;
                    stack.Push(result);
                }
            }
        }

        /**  :  the "truncate" operator. */
        internal class Truncate : Operator
        {
            public void execute(ExecutionContext context)
            {
                var num = context.popNumber();
                if (num is int numi)
                {
                    context.getStack().Push(numi);
                }
                else
                {
                    context.getStack().Push((float)(int)(num));
                }
            }
        }
    }
}