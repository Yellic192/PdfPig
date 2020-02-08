namespace UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter
{
    using Content;
    using Core;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline;

    /// <summary>
    /// Default Page Segmenter. All words are included in one block.
    /// </summary>
    public class DefaultPageSegmenter : IPageSegmenter
    {
        /// <summary>
        /// Create an instance of default page segmenter, <see cref="DefaultPageSegmenter"/>.
        /// </summary>
        public static IPageSegmenter Instance { get; } = new DefaultPageSegmenter();

        /// <summary>
        /// Get the blocks.
        /// </summary>
        /// <param name="pageWords">The words in the page.</param>
        public IReadOnlyList<TextBlock> GetBlocks(IEnumerable<Word> pageWords)
        {
            if (pageWords.Count() == 0) return EmptyArray<TextBlock>.Instance;

            return new List<TextBlock>() { new TextBlock(new XYLeaf(pageWords).GetLines()) };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IReadOnlyList<TextBlock> Get(IReadOnlyList<Word> input, DLAContext context)
        {
            return GetBlocks(input);
        }
    }
}
