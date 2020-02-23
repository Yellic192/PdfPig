namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System.Collections.Generic;

    /// <summary>
    /// Reading order detector determines the page's blocks reading order.
    /// <para>Note: Make sure you use <see cref="BaseBlock.SetReadingOrder(int)"/> to set each <see cref="BaseBlock"/> reading order when implementing <see cref="IReadingOrderDetector.Get(IReadOnlyList{TextBlock})"/>.</para>
    /// </summary>
    public interface IReadingOrderDetector
    {
        /// <summary>
        /// Gets the blocks in reading order and sets the <see cref="BaseBlock.ReadingOrder"/>.
        /// </summary>
        /// <param name="textBlocks">The <see cref="TextBlock"/>s to order.</param>
        IEnumerable<TextBlock> Get(IReadOnlyList<TextBlock> textBlocks);
    }
}
