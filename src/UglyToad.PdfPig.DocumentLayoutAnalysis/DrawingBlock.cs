namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// 
    /// </summary>
    public class DrawingBlock : ContentBlock
    {
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<PdfPath> Paths { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdfPaths"></param>
        public DrawingBlock(IReadOnlyList<PdfPath> pdfPaths)
        {
            Paths = pdfPaths;
            var boxes = Paths.Select(p => p.GetBoundingRectangle())
                             .Where(b => b.HasValue)
                             .Select(b => b.Value).ToList();

            BoundingBox = new PdfRectangle(boxes.Min(x => x.Left),
                                           boxes.Min(x => x.Bottom),
                                           boxes.Max(x => x.Right),
                                           boxes.Max(x => x.Top));
        }
    }
}
