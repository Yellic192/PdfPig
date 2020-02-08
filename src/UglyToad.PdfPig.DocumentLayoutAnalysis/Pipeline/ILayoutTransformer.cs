namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Input"></typeparam>
    /// <typeparam name="Output"></typeparam>
    public interface ILayoutTransformer<Input, Output>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Output Get(Input input, DLAContext context);
    }
}
