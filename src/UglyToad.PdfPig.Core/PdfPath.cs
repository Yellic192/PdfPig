namespace UglyToad.PdfPig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core.Graphics;
    using UglyToad.PdfPig.Core.Graphics.Colors;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    /// <summary>
    /// 
    /// </summary>
    public class PdfPath : List<PdfSubpath>
    {
        private bool isClosed;

        private PdfSubpath CurrentSubpath;

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
        public bool IsFilled { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsStroked { get; private set; }

        /// <summary>
        /// Set the clipping mode for this path.
        /// </summary>
        public void SetClipping(FillingRule fillingRule)
        {
            IsFilled = false;
            IsStroked = false;
            IsClipping = true;
            FillingRule = fillingRule;
            isClosed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetStroked()
        {
            IsStroked = true;
            isClosed = true;
        }

        /// <summary>
        /// Set the filling rule for this path.
        /// </summary>
        public void SetFillingRule(FillingRule fillingRule)
        {
            IsFilled = true;
            FillingRule = fillingRule;
            isClosed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void BeginSubpath()
        {
            if (isClosed)
            {
                throw new ArgumentException("BeginSubpath with closed path.");
            }

            if (CurrentSubpath != null)
            {
                AddCurrentSubpath();
            }

            CurrentSubpath = new PdfSubpath();
        }

        /// <summary>
        /// Add a <see cref="Move"/> command to the path.
        /// <para>Begin a new subpath by moving the current point to coordinates (x, y), omitting any 
        /// connecting line segment. If the previous path construction operator in the current path was
        /// also m, the new m overrides it; no vestige of the previous m operation remains in the path.</para>
        /// </summary>
        public void MoveTo(double x, double y)
        {
            if (isClosed)
            {
                throw new ArgumentException("MoveTo with closed path.");
            }

            // TODO: check previous command for move
            CurrentSubpath.MoveTo(x, y);
        }

        /// <summary>
        /// Add a <see cref="Line"/> command to the path.
        /// <para>Append a straight line segment from the current point to the point (x, y). The new current point shall be (x, y).</para>
        /// </summary>
        public void LineTo(double x, double y)
        {
            if (isClosed)
            {
                throw new ArgumentException("LineTo with closed path.");
            }

            CurrentSubpath.LineTo(x, y);
        }

        /// <summary>
        /// Adds 4 <see cref="Line"/>s forming a rectangle to the path.
        /// </summary>
        public void Rectangle(double x, double y, double width, double height)
        {
            if (isClosed)
            {
                throw new ArgumentException("Rectangle with closed path.");
            }

            BeginSubpath();
            CurrentSubpath.Rectangle(x, y, width, height);
            AddCurrentSubpath();
        }

        /// <summary>
        /// Add a <see cref="BezierCurve"/> to the path.
        /// </summary>
        public void BezierCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            if (isClosed)
            {
                throw new ArgumentException("BezierCurveTo with closed path.");
            }

            CurrentSubpath.BezierCurveTo(x1, y1, x2, y2, x3, y3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        public void BezierCurveTo(double x2, double y2, double x3, double y3)
        {
            if (isClosed)
            {
                throw new ArgumentException("BezierCurveTo with closed path.");
            }

            CurrentSubpath.BezierCurveTo(x2, y2, x3, y3);
        }

        /// <summary>
        /// Close the current subpath by appending a straight line segment from the current point to the starting point of the subpath. If the current subpath is already closed, h shall donothing.
        /// <para>This operator terminates the current subpath. Appending another segment to the current path shall begin a new subpath, even if the new segment begins at the endpoint reached by the h operation.</para>
        /// </summary>
        public void CloseSubpath()
        {
            if (isClosed)
            {
                Console.WriteLine("CloseSubpath with closed path.");
                throw new ArgumentException("CloseSubpath with closed path.");
            }

            if (!CurrentSubpath.Commands.Any(c => c is Close))
            {
                CurrentSubpath.CloseSubpath();
            }
            AddCurrentSubpath();
        }

        private void AddCurrentSubpath()
        {
            this.Add(CurrentSubpath);
            CurrentSubpath = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PdfRectangle? GetBoundingRectangle()
        {
            var bboxes = this.Select(p => p.GetBoundingRectangle()).Where(x => x.HasValue).Select(x => x.Value).ToList();
            if (bboxes.Count == 0)
            {
                return null;
            }
            var minX = bboxes.Min(b => b.Left);
            var maxX = bboxes.Max(b => b.Right);
            var minY = bboxes.Min(b => b.Bottom);
            var maxY = bboxes.Max(b => b.Top);
            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Create a clone with no Subpath.
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
    }
}
