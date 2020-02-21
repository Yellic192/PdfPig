namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Input"></typeparam>
    /// <typeparam name="ProcessorInput"></typeparam>
    /// <typeparam name="Output"></typeparam>
    public abstract class DlaPipelineBase<Input, ProcessorInput, Output> : IDlaPipeline<Input, Output>, ILayoutProcessor<Input, Output>
    {
        private DLAContext context;

        /// <summary>
        /// 
        /// </summary>
        protected ILayoutProcessor<ProcessorInput, Output> currentProcessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="context"></param>
        internal DlaPipelineBase(ILayoutProcessor<ProcessorInput, Output> processor, DLAContext context)
        {
            currentProcessor = processor;
            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ProcessorOutput"></typeparam>
        /// <param name="processor"></param>
        public DlaPipeline<Input, Output, ProcessorOutput> Append<ProcessorOutput>(ILayoutProcessor<Output, ProcessorOutput> processor)
        {
            return new DlaPipeline<Input, Output, ProcessorOutput>(processor, this, context);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        public Output Get(Input input)
        {
            context.Reset();
            context.Stopwatch.Reset();
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
        public Output Get(Input input, DLAContext context)
        {
            return Get(input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        public abstract Output GetSubPipeline(Input input, DLAContext context);
    }
}
