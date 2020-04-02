namespace UglyToad.PdfPig.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core.Graphics;
    using UglyToad.PdfPig.Core.Graphics.Colors;

    /// <summary>
    /// 
    /// </summary>
    public class PdfPath : List<PdfSubpath>
    {

        /// <summary>
        /// Rules for determining which points lie inside/outside the path.
        /// </summary>
        public FillingRule FillingRule { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IColor FillColor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IColor StrokeColor { get; set; }

        /// <summary>
        /// Thickness in user space units of path to be stroked.
        /// </summary>
        public double LineWidth { get; set; } = double.NaN;

        /// <summary>
        /// The pattern to be used for stroked lines.
        /// </summary>
        public LineDashPattern? LineDashPattern { get; set; }

        /// <summary>
        /// The cap style to be used for stroked lines.
        /// </summary>
        public LineCapStyle LineCapStyle { get; set; }

        /// <summary>
        /// The join style to be used for stroked lines.
        /// </summary>
        public LineJoinStyle LineJoinStyle { get; set; }

        /// <summary>
        /// Returns true if this is a clipping path.
        /// </summary>
        public bool IsClipping { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsFilled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsStroked { get; set; }

        /// <summary>
        /// Set the clipping mode for this path.
        /// </summary>
        public void SetClipping(FillingRule fillingRule)
        {
            IsFilled = false;
            IsStroked = false;
            IsClipping = true;
            FillingRule = fillingRule;
        }

        /// <summary>
        /// Set the filling rule for this path.
        /// </summary>
        public void SetFillingRule(FillingRule fillingRule)
        {
            FillingRule = fillingRule;
        }

        /// <summary>
        /// Create a clone with no Commands.
        /// </summary>
        public PdfPath CloneEmpty()
        {
            PdfPath newPath = new PdfPath();
            if (IsClipping)
            {
                newPath.SetClipping(FillingRule);
            }
            else
            {
                if (IsFilled)
                {
                    newPath.IsFilled = true;
                    newPath.SetFillingRule(FillingRule);
                    newPath.FillColor = FillColor;
                }

                if (IsStroked)
                {
                    newPath.IsStroked = true;
                    newPath.LineCapStyle = LineCapStyle;
                    newPath.LineDashPattern = LineDashPattern;
                    newPath.LineJoinStyle = LineJoinStyle;
                    newPath.LineWidth = LineWidth;
                    newPath.StrokeColor = StrokeColor;
                }
            }
            return newPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PdfRectangle? GetBoundingRectangle()
        {
            if (this.Count == 0)
            {
                return null;
            }

            var bboxes = this.Select(x => x.GetBoundingRectangle()).Where(x => x.HasValue).Select(x => x.Value).ToList();
            if (bboxes.Count == 0)
            {
                return null;
            }

            var minX = bboxes.Min(x => x.Left);
            var minY = bboxes.Min(x => x.Bottom);
            var maxX = bboxes.Max(x => x.Right);
            var maxY = bboxes.Max(x => x.Top);
            return new PdfRectangle(minX, minY, maxX, maxY);
        }
    }
}
