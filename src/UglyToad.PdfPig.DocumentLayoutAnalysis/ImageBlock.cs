namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// 
    /// </summary>
    public class ImageBlock : ContentBlock
    {
        /// <summary>
        /// 
        /// </summary>
        public IPdfImage Image { get; }

        /// <summary>
        /// 
        /// </summary>
        public override PdfRectangle BoundingBox => Image.Bounds;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdfImage"></param>
        public ImageBlock(IPdfImage pdfImage)
        {
            Image = pdfImage;
            BoundingBox = pdfImage.Bounds;
        }
    }
}
