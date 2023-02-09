namespace UglyToad.PdfPig.Functions.Type4
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /**
    * Interface for PostScript operators.
    *
    */
    internal interface Operator
    {

        /**
         * Executes the operator. The method can inspect and manipulate the stack.
         * @param context the execution context
         */
        void execute(ExecutionContext context);

    }
}
