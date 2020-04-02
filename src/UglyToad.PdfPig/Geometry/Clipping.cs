namespace UglyToad.PdfPig.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    /// <summary>
    /// 
    /// </summary>
    public static class Clipping
    {
        const double factor = 10_000.0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static PdfPath Clip(this PdfPath clipping, PdfPath subject)
        {
            if (clipping == null)
            {
                throw new ArgumentNullException(nameof(clipping), "Clip(): the clipping path cannot be null.");
            }

            if (!clipping.IsClipping)
            {
                throw new ArgumentException("Clip(): the clipping path does not have the IsClipping flag set to true.", nameof(clipping));
            }

            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject), "Clip(): the subject path cannot be null.");
            }

            if (subject.Count == 0)
            {
                return subject;
            }

            Clipper clipper = new Clipper();
            foreach (var subpath in clipping)
            {
                // force close clipping polygon
                if (!subpath.IsClosed()) subpath.ClosePath();
                clipper.AddPath(subpath.ToClipperPolygon(10).ToList(), PolyType.ptClip, true);
            }

            bool subjectClose = subject.IsFilled || subject.IsClipping;
            foreach (var subpath in subject)
            {      
                // force close subject if need be
                if (subjectClose && !subpath.IsClosed()) subpath.ClosePath();
                clipper.AddPath(subpath.ToClipperPolygon(10).ToList(), PolyType.ptSubject, subjectClose);
            }

            var clippingFillType = clipping.FillingRule == FillingRule.NonZeroWinding ? PolyFillType.pftNonZero : PolyFillType.pftEvenOdd;
            var subjectFillType = subject.FillingRule == FillingRule.NonZeroWinding ? PolyFillType.pftNonZero : PolyFillType.pftEvenOdd;

            if (!subjectClose)
            {
                // case where subject is not closed
                var solutions = new PolyTree();
                if (clipper.Execute(ClipType.ctIntersection, solutions, subjectFillType, clippingFillType))
                {
                    PdfPath clippedPath = subject.CloneEmpty();
                    foreach (var solution in solutions.Childs)
                    {
                        if (solution.Contour.Count > 0)
                        {
                            PdfSubpath clipped = new PdfSubpath();
                            clipped.MoveTo(solution.Contour[0].X / factor, solution.Contour[0].Y / factor);

                            for (int i = 1; i < solution.Contour.Count; i++)
                            {
                                clipped.LineTo(solution.Contour[i].X / factor, solution.Contour[i].Y / factor);
                            }
                            clippedPath.Add(clipped);
                        }
                    }
                    if (clippedPath.Count > 0) return clippedPath;
                }
                return null;
            }
            else
            {
                // case where subject is closed
                var solutions = new List<List<IntPoint>>();
                if (clipper.Execute(ClipType.ctIntersection, solutions, subjectFillType, clippingFillType))
                {
                    PdfPath clippedPath = subject.CloneEmpty();
                    foreach (var solution in solutions)
                    {
                        PdfSubpath clipped = new PdfSubpath();
                        clipped.MoveTo(solution[0].X / factor, solution[0].Y / factor);

                        for (int i = 1; i < solution.Count; i++)
                        {
                            clipped.LineTo(solution[i].X / factor, solution[i].Y / factor);
                        }
                        if (!clipped.IsClosed()) clipped.ClosePath();
                        if (clipped.Commands.Count > 0) clippedPath.Add(clipped);
                    }
                    if (clippedPath.Count > 0) return clippedPath;
                }
                return null;
            }
        }

        private static IEnumerable<IntPoint> ToClipperPolygon(this PdfSubpath pdfPath, int n = 4)
        {
            if (pdfPath.Commands.Count == 0)
            {
                yield break;
            }

            if (pdfPath.Commands[0] is Move currentMove)
            {
                yield return new IntPoint(currentMove.Location.X * factor, currentMove.Location.Y * factor);
            }
            else
            {
                throw new ArgumentException("ToClipperPolygon");
            }

            for (int i = 1; i < pdfPath.Commands.Count; i++)
            {
                var command = pdfPath.Commands[i];
                if (command is Move)
                {
                    throw new ArgumentException("ToClipperPolygon");
                }
                else if (command is Line line)
                {
                    yield return new IntPoint(line.From.X * factor, line.From.Y * factor);
                    yield return new IntPoint(line.To.X * factor, line.To.Y * factor);
                }
                else if (command is BezierCurve curve)
                {
                    foreach (var lineB in curve.ToLines(n))
                    {
                        yield return new IntPoint(lineB.From.X * factor, lineB.From.Y * factor);
                        yield return new IntPoint(lineB.To.X * factor, lineB.To.Y * factor);
                    }
                }
                else if (command is Close)
                {
                    yield return new IntPoint(currentMove.Location.X * factor, currentMove.Location.Y * factor);
                }
                else
                {
                    throw new ArgumentException("ToClipperPolygon - unknown command");
                }
            }
        }
    }
}
