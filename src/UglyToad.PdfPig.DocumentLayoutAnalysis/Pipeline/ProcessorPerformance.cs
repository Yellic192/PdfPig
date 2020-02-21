namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public struct ProcessorPerformance
    {
        internal ProcessorPerformance(long processTimeInMilliseconds, int inputCount, int outputCount, Exception exception = null)
        {
            ProcessTimeInMilliseconds = processTimeInMilliseconds;
            InputCount = inputCount;
            OutputCount = outputCount;
            Exception = exception;
        }

        /// <summary>
        /// 
        /// </summary>
        public long ProcessTimeInMilliseconds { get; }

        /// <summary>
        /// 
        /// </summary>
        public int InputCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public int OutputCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool Success => Exception == null;

        /// <summary>
        /// 
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "input#=" + InputCount + ", output#=" + OutputCount + ", time=" + ProcessTimeInMilliseconds + "ms";
        }
    }
}
