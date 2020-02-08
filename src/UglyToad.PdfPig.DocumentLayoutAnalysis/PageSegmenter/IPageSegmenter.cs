namespace UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter
{
    using Content;
    using System.Collections.Generic;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline;

    /// <summary>
    /// Page segmentation divides a page into areas, each consisting of a layout structure (blocks, lines, etc.).
    /// <para> See 'Performance Comparison of Six Algorithms for Page Segmentation' by Faisal Shafait, Daniel Keysers, and Thomas M. Breuel.</para>
    /// </summary>
    public interface IPageSegmenter : ILayoutTransformer<IReadOnlyList<Word>, IReadOnlyList<TextBlock>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        IReadOnlyList<TextBlock> GetBlocks(IEnumerable<Word> words);
    }
}
