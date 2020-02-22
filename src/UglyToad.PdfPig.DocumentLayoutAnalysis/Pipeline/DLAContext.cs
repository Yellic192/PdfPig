namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    // https://stackoverflow.com/questions/50664273/how-to-create-a-generic-pipeline-in-c

    /// <summary>
    /// 
    /// </summary>
    public class DLAContext
    {
        /// <summary>
        /// Gets or sets the maximum number of concurrent tasks enabled. Default value is -1.
        /// <para>A positive property value limits the number of concurrent operations to the set value. 
        /// If it is -1, there is no limit on the number of concurrently running operations.</para>
        /// </summary>
        public int MaxDegreeOfParallelism { get; }

        internal Dictionary<string, ProcessorPerformance> ProcessorPerformances;

        internal string description;

        /// <summary>
        /// 
        /// </summary>
        public DLAContext(int maxDegreeOfParallelism = -1)
        {
            UniqueToken = new Guid();
            logs = new List<string>();
            Stopwatch = new Stopwatch();
            ProcessorPerformances = new Dictionary<string, ProcessorPerformance>();
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            PageSegmenters = new PageSegmentersCatalog(MaxDegreeOfParallelism);
            WordExtractors = new WordExtractorsCatalog(MaxDegreeOfParallelism);
            OtherExtractors = new OtherExtractorsCatalog(MaxDegreeOfParallelism);
            ReadingOrder = new ReadingOrderDetectorsCatalog(MaxDegreeOfParallelism);
            description = "DLA PipeLine:";
        }        

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            logs = new List<string>();
            Stopwatch.Reset();
            ProcessorPerformances = new Dictionary<string, ProcessorPerformance>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="InputType"></typeparam>
        /// <typeparam name="OutputType"></typeparam>
        /// <param name="processor"></param>
        /// <returns></returns>
        public DlaPipeline<InputType, InputType, OutputType> PipeLine<InputType, OutputType>(ILayoutProcessor<InputType, OutputType> processor)
        {
            return new DlaPipeline<InputType, InputType, OutputType>(processor, this);
        }

        /// <summary>
        /// 
        /// </summary>
        internal readonly Stopwatch Stopwatch;

        /// <summary>
        /// 
        /// </summary>
        internal Guid UniqueToken;

        /// <summary>
        /// 
        /// </summary>
        public long ProcessTimeInMilliseconds
        {
            get
            {
                return Stopwatch.ElapsedMilliseconds;
            }
        }

        private List<string> logs { get; set; }

        internal void AddLog(string log)
        {
            logs.Add(log);
        }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<string> Logs => logs;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string l = string.Join("\n", Logs).Trim();
            if (string.IsNullOrEmpty(l)) l = "Logs:\n" + l;

            string perf = "Performance:\n";
            foreach (var kvp in ProcessorPerformances)
            {
                perf += "\t" + kvp.Key + ": " + kvp.Value.ToString() + "\n";
            }

            return description + "\n\n"
                + perf + "\n"
                + l
                + $"Pipeline execution took {ProcessTimeInMilliseconds} milliseconds.";
        }

        /// <summary>
        /// 
        /// </summary>
        public PageSegmentersCatalog PageSegmenters { get; }

        /// <summary>
        /// 
        /// </summary>
        public WordExtractorsCatalog WordExtractors { get; }

        /// <summary>
        /// 
        /// </summary>
        public OtherExtractorsCatalog OtherExtractors { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReadingOrderDetectorsCatalog ReadingOrder { get; }

        /// <summary>
        /// 
        /// </summary>
        public BaseProcessorsCatalog BaseProcessors { get; }
    }
}
