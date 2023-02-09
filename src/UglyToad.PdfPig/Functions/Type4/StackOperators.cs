namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /**
   * Provides the stack operators such as "Pop" and "dup".
   *
   */
    class StackOperators
    {
        static Stack<T> AddAll<T>(Stack<T> stack, IEnumerable<T> values)
        {
            var all = stack.ToList();
            all.AddRange(values);
            return new Stack<T>(all);
        }

        private StackOperators()
        {
            // Private constructor.
        }

        /** Implements the "copy" operator. */
        internal class Copy : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                int n = ((int)stack.Pop());
                if (n > 0)
                {
                    int size = stack.Count;
                    //Need to copy to a new list to avoid ConcurrentModificationException
                    //List<Object> copy = new java.util.ArrayList<>(stack.subList(size - n, size));
                    List<object> copy = stack.ToList().GetRange(size - n, size);
                    context.SetStack(AddAll(stack, copy)); //stack.addAll(copy); // TODO - check
                }
            }
        }

        /** Implements the "dup" operator. */
        internal class Dup : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                stack.Push(stack.Peek());
            }
        }

        /** Implements the "exch" operator. */
        internal class Exch : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                object any2 = stack.Pop();
                object any1 = stack.Pop();
                stack.Push(any2);
                stack.Push(any1);
            }
        }

        /** Implements the "index" operator. */
        internal class Index : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                int n = ((int)stack.Pop());
                if (n < 0)
                {
                    throw new ArgumentException("rangecheck: " + n);
                }
                int size = stack.Count;
                stack.Push(stack.ElementAt(size - n - 1));
            }
        }

        /** Implements the "Pop" operator. */
        internal class Pop : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
                stack.Pop();
            }
        }

        /** Implements the "roll" operator. */
        internal class Roll : Operator
        {
            public void execute(ExecutionContext context)
            {
                Stack<object> stack = context.getStack();
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

                LinkedList<object> rolled = new LinkedList<object>();
                LinkedList<object> moved = new LinkedList<object>();
                if (j < 0)
                {
                    //negative roll
                    int n1 = n + j;
                    for (int i = 0; i < n1; i++)
                    {
                        moved.AddFirst(stack.Pop());
                    }
                    for (int i = j; i < 0; i++)
                    {
                        rolled.AddFirst(stack.Pop());
                    }
                    context.SetStack(AddAll(stack, moved)); //stack.addAll(moved); // TODO - check
                    context.SetStack(AddAll(stack, rolled)); // stack.addAll(rolled); // TODO - check
                }
                else
                {
                    //positive roll
                    int n1 = n - j;
                    for (int i = j; i > 0; i--)
                    {
                        rolled.AddFirst(stack.Pop());
                    }
                    for (int i = 0; i < n1; i++)
                    {
                        moved.AddFirst(stack.Pop());
                    }
                    context.SetStack(AddAll(stack, rolled)); // stack.addAll(rolled); // TODO - check
                    context.SetStack(AddAll(stack, moved)); // stack.addAll(moved); // TODO - check
                }
            }
        }
    }
}
