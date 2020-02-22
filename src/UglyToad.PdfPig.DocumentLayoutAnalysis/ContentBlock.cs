namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// 
    /// </summary>
    public class ContentBlock
    {
        /// <summary>
        /// The rectangle completely containing the block.
        /// </summary>
        public virtual PdfRectangle BoundingBox { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<ContentBlock> Blocks { get; protected set; }
    }
}
