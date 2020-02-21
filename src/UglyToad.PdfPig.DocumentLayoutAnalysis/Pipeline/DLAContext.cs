namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Export;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using static UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter.DocstrumBoundingBoxes;

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
            Stopwatch = new Stopwatch();
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
        internal Stopwatch Stopwatch;

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

    /// <summary>
    /// 
    /// </summary>
    public struct BaseProcessorsCatalog
    {
        /// <summary>
        /// 
        /// </summary>
        public PageToLetters PageToLetters() => new PageToLetters();
    }

    /// <summary>
    /// 
    /// </summary>
    public class PageToLetters : ILayoutProcessor<Page, IReadOnlyList<Letter>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IReadOnlyList<Letter> Get(Page input, DLAContext context)
        {
            return input.Letters;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ReadingOrderDetectorsCatalog
    {
        private int MaxDegreeOfParallelism;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        public ReadingOrderDetectorsCatalog(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DefaultReadingOrderDetector DefaultReadingOrderDetector() => new DefaultReadingOrderDetector();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RenderingReadingOrderDetector RenderingReadingOrderDetector() => new RenderingReadingOrderDetector();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UnsupervisedReadingOrderDetector UnsupervisedReadingOrderDetector() => new UnsupervisedReadingOrderDetector();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public UnsupervisedReadingOrderDetector UnsupervisedReadingOrderDetector(double T) => new UnsupervisedReadingOrderDetector(T);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct OtherExtractorsCatalog
    {
        private int MaxDegreeOfParallelism;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        public OtherExtractorsCatalog(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DecorationTextBlockClassifier DecorationTextBlockClassifier() => new DecorationTextBlockClassifier();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WhitespaceCoverExtractor WhitespaceCoverExtractor() => new WhitespaceCoverExtractor();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TextEdgesExtractor TextEdgesExtractor() => new TextEdgesExtractor();
    }

    /// <summary>
    /// 
    /// </summary>
    public struct WordExtractorsCatalog
    {
        private int MaxDegreeOfParallelism;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        public WordExtractorsCatalog(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NearestNeighbourWordExtractor NearestNeighbourWordExtractor() => new NearestNeighbourWordExtractor(MaxDegreeOfParallelism);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDistanceFunction"></param>
        /// <param name="distMeasure"></param>
        public NearestNeighbourWordExtractor NearestNeighbourWordExtractor(Func<Letter, Letter, double> maxDistanceFunction, Func<PdfPoint, PdfPoint, double> distMeasure)
            => new NearestNeighbourWordExtractor(maxDistanceFunction, distMeasure, MaxDegreeOfParallelism);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDistanceFunction"></param>
        /// <param name="distMeasure"></param>
        /// <param name="maxDistanceFunctionOther"></param>
        /// <param name="distMeasureOther"></param>
        /// <returns></returns>
        public NearestNeighbourWordExtractor NearestNeighbourWordExtractor(Func<Letter, Letter, double> maxDistanceFunction, Func<PdfPoint, PdfPoint, double> distMeasure,
                                             Func<Letter, Letter, double> maxDistanceFunctionOther, Func<PdfPoint, PdfPoint, double> distMeasureOther)
            => new NearestNeighbourWordExtractor(maxDistanceFunction, distMeasure, maxDistanceFunctionOther, distMeasureOther, MaxDegreeOfParallelism);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDistanceFunctionH"></param>
        /// <param name="distMeasureH"></param>
        /// <param name="maxDistanceFunction270"></param>
        /// <param name="distMeasure270"></param>
        /// <param name="maxDistanceFunction180"></param>
        /// <param name="distMeasure180"></param>
        /// <param name="maxDistanceFunction90"></param>
        /// <param name="distMeasure90"></param>
        /// <param name="maxDistanceFunctionOther"></param>
        /// <param name="distMeasureOther"></param>
        /// <returns></returns>
        public NearestNeighbourWordExtractor NearestNeighbourWordExtractor(Func<Letter, Letter, double> maxDistanceFunctionH, Func<PdfPoint, PdfPoint, double> distMeasureH,
                                             Func<Letter, Letter, double> maxDistanceFunction270, Func<PdfPoint, PdfPoint, double> distMeasure270,
                                             Func<Letter, Letter, double> maxDistanceFunction180, Func<PdfPoint, PdfPoint, double> distMeasure180,
                                             Func<Letter, Letter, double> maxDistanceFunction90, Func<PdfPoint, PdfPoint, double> distMeasure90,
                                             Func<Letter, Letter, double> maxDistanceFunctionOther, Func<PdfPoint, PdfPoint, double> distMeasureOther)
            => new NearestNeighbourWordExtractor(maxDistanceFunctionH, distMeasureH, maxDistanceFunction270, distMeasure270,
                maxDistanceFunction180, distMeasure180, maxDistanceFunction90, distMeasure90,
                maxDistanceFunctionOther, distMeasureOther, MaxDegreeOfParallelism);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct PageSegmentersCatalog
    {
        private int MaxDegreeOfParallelism;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        public PageSegmentersCatalog(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// 
        /// </summary>
        public DefaultPageSegmenter DefaultPageSegmenter() => new DefaultPageSegmenter();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DocstrumBoundingBoxes DocstrumBoundingBoxes() => new DocstrumBoundingBoxes(MaxDegreeOfParallelism);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="withinLine"></param>
        /// <param name="betweenLine"></param>
        /// <param name="betweenLineMultiplier"></param>
        /// <returns></returns>
        public DocstrumBoundingBoxes DocstrumBoundingBoxes(AngleBounds withinLine, AngleBounds betweenLine, double betweenLineMultiplier) => new DocstrumBoundingBoxes(withinLine, betweenLine, betweenLineMultiplier, MaxDegreeOfParallelism);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RecursiveXYCut RecursiveXYCut() => new RecursiveXYCut();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumWidth"></param>
        /// <returns></returns>
        public RecursiveXYCut RecursiveXYCut(double minimumWidth) => new RecursiveXYCut(minimumWidth);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumWidth"></param>
        /// <param name="dominantFontWidth"></param>
        /// <param name="dominantFontHeight"></param>
        /// <returns></returns>
        public RecursiveXYCut RecursiveXYCut(double minimumWidth, double dominantFontWidth, double dominantFontHeight) => new RecursiveXYCut(minimumWidth, dominantFontWidth, dominantFontHeight);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumWidth"></param>
        /// <param name="dominantFontWidthFunc"></param>
        /// <param name="dominantFontHeightFunc"></param>
        /// <returns></returns>
        public RecursiveXYCut RecursiveXYCut(double minimumWidth, Func<IEnumerable<double>, double> dominantFontWidthFunc,
            Func<IEnumerable<double>, double> dominantFontHeightFunc) => new RecursiveXYCut(minimumWidth, dominantFontWidthFunc, dominantFontHeightFunc);
    }
}
