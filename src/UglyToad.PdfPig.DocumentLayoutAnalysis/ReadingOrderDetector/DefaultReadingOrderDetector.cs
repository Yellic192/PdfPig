namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline;

    /// <summary>
    /// This detector does nothing, no ordering takes place.
    /// </summary>
    public class DefaultReadingOrderDetector : IReadingOrderDetector
    {
        /// <summary>
        /// Create an instance of default reading order detector, <see cref="DefaultReadingOrderDetector"/>.
        /// <para>This detector does nothing, no ordering takes place.</para>
        /// </summary>
        public static DefaultReadingOrderDetector Instance { get; } = new DefaultReadingOrderDetector();

        /// <summary>
        /// Gets the blocks in reading order and sets the <see cref="TextBlock.ReadingOrder"/>.
        /// </summary>
        /// <param name="textBlocks">The <see cref="TextBlock"/>s to order.</param>
        public IEnumerable<TextBlock> Get(IReadOnlyList<TextBlock> textBlocks)
        {
            return textBlocks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IReadOnlyList<TextBlock> Get(IReadOnlyList<TextBlock> input, DLAContext context)
        {
            return Get(input).ToList();
        }
    }
}
