namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides the stack operators such as "Pop" and "dup".
    /// </summary>
    internal sealed class StackOperators
    {
        private static Stack<T> AddAll<T>(Stack<T> stack, IEnumerable<T> values)
        {
            var valuesList = values.ToList();
            valuesList.AddRange(stack);
            valuesList.Reverse();
            return new Stack<T>(valuesList);
        }

        private StackOperators()
        {
            // Private constructor.
        }

        /// <summary>
        /// Implements the "copy" operator.
        /// </summary>
        internal sealed class Copy : Operator
        {
            public void Execute(ExecutionContext context)
            {
                Stack<object> stack = context.GetStack();
                int n = ((int)stack.Pop());
                if (n > 0)
                {
                    int size = stack.Count;
                    //Need to copy to a new list to avoid ConcurrentModificationException
                    List<object> copy = stack.ToList().GetRange(size - n - 1, n);
                    stack = context.SetStack(AddAll(stack, copy));
                }
            }
        }

        /// <summary>
        /// Implements the "dup" operator.
        /// </summary>
        internal sealed class Dup : Operator
        {
            public void Execute(ExecutionContext context)
            {
                Stack<object> stack = context.GetStack();
                stack.Push(stack.Peek());
            }
        }

        /// <summary>
        /// Implements the "exch" operator.
        /// </summary>
        internal sealed class Exch : Operator
        {
            public void Execute(ExecutionContext context)
            {
                Stack<object> stack = context.GetStack();
                object any2 = stack.Pop();
                object any1 = stack.Pop();
                stack.Push(any2);
                stack.Push(any1);
            }
        }

        /// <summary>
        /// Implements the "index" operator.
        /// </summary>
        internal sealed class Index : Operator
        {
            public void Execute(ExecutionContext context)
            {
                Stack<object> stack = context.GetStack();
                int n = Convert.ToInt32(stack.Pop());
                if (n < 0)
                {
                    throw new ArgumentException("rangecheck: " + n);
                }
                int size = stack.Count;
                stack.Push(stack.ElementAt(n));
            }
        }

        /// <summary>
        /// Implements the "Pop" operator.
        /// </summary>
        internal sealed class Pop : Operator
        {
            public void Execute(ExecutionContext context)
            {
                Stack<object> stack = context.GetStack();
                stack.Pop();
            }
        }

        /// <summary>
        /// Implements the "roll" operator.
        /// </summary>
        internal sealed class Roll : Operator
        {
            public void Execute(ExecutionContext context)
            {
                Stack<object> stack = context.GetStack();
                int j = ((int)stack.Pop());
                int n = ((int)stack.Pop());
                if (j == 0)
                {
                    return; //Nothing to do
                }
                if (n < 0)
                {
                    throw new ArgumentException("rangecheck: " + n);
                }

                var rolled = new List<object>();
                var moved = new List<object>();
                if (j < 0)
                {
                    //negative roll
                    int n1 = n + j;
                    for (int i = 0; i < n1; i++)
                    {
                        moved.Add(stack.Pop());
                    }
                    for (int i = j; i < 0; i++)
                    {
                        rolled.Add(stack.Pop());
                    }

                    stack = context.SetStack(AddAll(stack, moved));
                    stack = context.SetStack(AddAll(stack, rolled));
                }
                else
                {
                    //positive roll
                    int n1 = n - j;
                    for (int i = j; i > 0; i--)
                    {
                        rolled.Add(stack.Pop());
                    }
                    for (int i = 0; i < n1; i++)
                    {
                        moved.Add(stack.Pop());
                    }

                    stack = context.SetStack(AddAll(stack, rolled));
                    stack = context.SetStack(AddAll(stack, moved));
                }
            }
        }
    }
}
