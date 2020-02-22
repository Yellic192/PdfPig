namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public struct ProcessorPerformance
    {
        internal ProcessorPerformance(long processingTime, int inputCount, int outputCount, Exception exception = null)
        {
            ProcessingTime = processingTime;
            InputCount = inputCount;
            OutputCount = outputCount;
            Exception = exception;
        }

        /// <summary>
        /// 
        /// </summary>
        public long ProcessingTime { get; }

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
            return "input#=" + InputCount + ", output#=" + OutputCount + ", time=" + ProcessingTime + "ms";
        }
    }
}
