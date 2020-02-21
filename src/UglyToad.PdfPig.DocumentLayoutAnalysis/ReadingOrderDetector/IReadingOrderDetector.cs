namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline;

    /// <summary>
    /// Reading order detector determines the page's blocks reading order.
    /// <para>Note: Make sure you use <see cref="TextBlock.SetReadingOrder(int)"/> to set each <see cref="TextBlock"/> reading order when implementing <see cref="IReadingOrderDetector.Get(IReadOnlyList{TextBlock})"/>.</para>
    /// </summary>
    public interface IReadingOrderDetector : ILayoutProcessor<IReadOnlyList<TextBlock>, IReadOnlyList<TextBlock>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textBlocks"></param>
        /// <returns></returns>
        IEnumerable<TextBlock> Get(IReadOnlyList<TextBlock> textBlocks);
    }
}
