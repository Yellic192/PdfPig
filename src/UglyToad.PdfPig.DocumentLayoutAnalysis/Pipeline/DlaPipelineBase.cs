namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Input"></typeparam>
    /// <typeparam name="ProcessorInput"></typeparam>
    /// <typeparam name="Output"></typeparam>
    public abstract class DlaPipelineBase<Input, ProcessorInput, Output> : IDlaPipeline<Input, Output>
    {
        /// <summary>
        /// 
        /// </summary>
        protected ILayoutTransformer<ProcessorInput, Output> currentProcessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        internal DlaPipelineBase(ILayoutTransformer<ProcessorInput, Output> processor)
        {
            currentProcessor = processor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ProcessorOutput"></typeparam>
        /// <param name="processor"></param>
        /// <returns></returns>
        public DlaPipeline<Input, Output, ProcessorOutput> Append<ProcessorOutput>(ILayoutTransformer<Output, ProcessorOutput> processor)
        {
            return new DlaPipeline<Input, Output, ProcessorOutput>(processor, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Output Get(Input input, out DLAContext context)
        {
            context = new DLAContext();
            context.Stopwatch.Start();
            var result = GetSubPipeline(input, context);
            context.Stopwatch.Stop();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract Output GetSubPipeline(Input input, DLAContext context);
    }
}
