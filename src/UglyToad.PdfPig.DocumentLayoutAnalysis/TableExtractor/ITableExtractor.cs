namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.Graphics;

    /// <summary>
    /// .
    /// </summary>
    public interface ITableExtractor
    {
        /// <summary>
        /// .
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<TableBlock> Get(IReadOnlyList<PdfPath> paths);
    }
}
