using System;

namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    public static class DlaPipeline
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="InputType"></typeparam>
        /// <typeparam name="OutputType"></typeparam>
        /// <param name="processor"></param>
        /// <returns></returns>
        public static DlaPipeline<InputType, InputType, OutputType> Create<InputType, OutputType>(ILayoutTransformer<InputType, OutputType> processor)
        {
            return new DlaPipeline<InputType, InputType, OutputType>(processor);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="InputType"></typeparam>
    /// <typeparam name="ProcessorInputType"></typeparam>
    /// <typeparam name="OutputType"></typeparam>
    public class DlaPipeline<InputType, ProcessorInputType, OutputType> : DlaPipelineBase<InputType, ProcessorInputType, OutputType>
    {
        IDlaPipeline<InputType, ProcessorInputType> previousPipeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        internal DlaPipeline(ILayoutTransformer<ProcessorInputType, OutputType> processor) : base(processor)
        {
            this.previousPipeline = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="previousPipeline"></param>
        internal DlaPipeline(ILayoutTransformer<ProcessorInputType, OutputType> processor, IDlaPipeline<InputType, ProcessorInputType> previousPipeline)
            : base(processor)
        {
            this.previousPipeline = previousPipeline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override OutputType GetSubPipeline(InputType input, DLAContext context)
        {
            if (previousPipeline == null)
            {
                return currentProcessor.Get((ProcessorInputType)(object)input, context);
            }

            var previousPipelineResult = previousPipeline.GetSubPipeline(input, context);
            return currentProcessor.Get(previousPipelineResult, context);
        }
    }
}
