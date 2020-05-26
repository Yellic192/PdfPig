namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Graphics;

    /// <summary>
    /// A block that can contain text, images and paths.
    /// </summary>
    public class GraphicBlock
    {
        /// <summary>
        /// The rectangle completely containing the block.
        /// </summary>
        public PdfRectangle BoundingBox { get; }

        /// <summary>
        /// The text blocks contained in the block.
        /// </summary>
        public IReadOnlyList<TextBlock> TextBlocks { get; }

        /// <summary>
        /// The images contained in the block.
        /// </summary>
        public IReadOnlyList<IPdfImage> Images { get; }

        /// <summary>
        /// The paths contained in the block.
        /// </summary>
        public IReadOnlyList<PdfPath> Paths { get; }

        /// <summary>
        /// The reading order index. Starts at 0. A value of -1 means the block is not ordered.
        /// </summary>
        public int ReadingOrder { get; private set; }

        /// <summary>
        /// Create a new <see cref="GraphicBlock"/>.
        /// </summary>
        /// <param name="textBlocks"></param>
        /// <param name="images"></param>
        /// <param name="paths"></param>
        public GraphicBlock(IReadOnlyList<TextBlock> textBlocks, IReadOnlyList<IPdfImage> images, IReadOnlyList<PdfPath> paths)
        {
            if (textBlocks == null && images == null && paths == null)
            {
                throw new ArgumentNullException();
            }

            ReadingOrder = -1;

            TextBlocks = textBlocks;
            Images = images;
            Paths = paths;

            var bboxes = new List<PdfRectangle>();
            if (textBlocks?.Count > 0) bboxes.AddRange(textBlocks.Select(b => b.BoundingBox));
            if (images?.Count > 0) bboxes.AddRange(images.Select(i => i.Bounds));
            if (paths?.Count > 0) bboxes.AddRange(paths.Select(p => p.GetBoundingRectangle()).Where(b => b.HasValue).Select(b => b.Value));

            BoundingBox = new PdfRectangle(bboxes.Min(x => x.BottomLeft.X),
                                           bboxes.Min(x => x.BottomLeft.Y),
                                           bboxes.Max(x => x.TopRight.X),
                                           bboxes.Max(x => x.TopRight.Y));
        }

        /// <summary>
        /// Sets the <see cref="TextBlock"/>'s reading order.
        /// </summary>
        /// <param name="readingOrder"></param>
        public void SetReadingOrder(int readingOrder)
        {
            if (readingOrder < -1)
            {
                throw new ArgumentException("The reading order should be more or equal to -1. A value of -1 means the block is not ordered.", nameof(readingOrder));
            }
            this.ReadingOrder = readingOrder;
        }
    }
}
