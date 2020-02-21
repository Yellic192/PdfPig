namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    using System;
    using System.Collections;
    using System.Diagnostics;

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
        /// <param name="context"></param>
        internal DlaPipeline(ILayoutProcessor<ProcessorInputType, OutputType> processor, DLAContext context)
            : base(processor, context)
        {
            context.description += "\n\t" + processor.GetType().Name;
            this.previousPipeline = null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="previousPipeline"></param>
        /// <param name="context"></param>
        internal DlaPipeline(ILayoutProcessor<ProcessorInputType, OutputType> processor, 
                             IDlaPipeline<InputType, ProcessorInputType> previousPipeline, DLAContext context)
            : base(processor, context)
        {
            context.description += "\n\t" + processor.GetType().Name;
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
            Stopwatch stopwatch = new Stopwatch();

            if (previousPipeline == null)
            {
                int inputCount = CountElements(input);

                try
                {
                    stopwatch.Start();
                    var output1 = currentProcessor.Get((ProcessorInputType)(object)input, context);
                    stopwatch.Stop();

                    int outputCount1 = CountElements(output1);
                    context.ProcessorPerformances.Add(currentProcessor.GetType().Name,
                        new ProcessorPerformance(stopwatch.ElapsedMilliseconds, inputCount, outputCount1));
                    return output1;
                }
                catch (Exception ex)
                {
                    context.ProcessorPerformances.Add(currentProcessor.GetType().Name,
                        new ProcessorPerformance(stopwatch.ElapsedMilliseconds, inputCount, -1, ex));
                    return default; // ?????????
                }
            }

            var previousPipelineResult = previousPipeline.GetSubPipeline(input, context);
            int inputCountP = CountElements(previousPipelineResult);

            try
            {
                stopwatch.Start();
                var output = currentProcessor.Get(previousPipelineResult, context);
                stopwatch.Stop();

                int outputCountP = CountElements(output);
                context.ProcessorPerformances.Add(currentProcessor.GetType().Name,
                    new ProcessorPerformance(stopwatch.ElapsedMilliseconds, inputCountP, outputCountP));
                return output;
            }
            catch (Exception ex)
            {
                context.ProcessorPerformances.Add(currentProcessor.GetType().Name,
                    new ProcessorPerformance(stopwatch.ElapsedMilliseconds, inputCountP, -1, ex));
                return default; // ?????????
            }
        }

        private int CountElements(object input)
        {
            if (input is ICollection list)
            {
                return list.Count;
            }
            return 1;
        }
    }
}
