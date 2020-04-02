namespace UglyToad.PdfPig.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;
    using static UglyToad.PdfPig.Core.PdfPath;

    /// <summary>
    /// 
    /// </summary>
    public static class Clipping
    {
        const int factor = 10_000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static PdfPathFix Clip(this PdfPathFix clipping, PdfPathFix subject)
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

                // convert PdfPath to polygon
                var intClipping = subpath.ToClipperPolygon(10).ToList();
                clipper.AddPath(intClipping, PolyType.ptClip, true);
            }

            bool subjectClose = subject.IsFilled || subject.IsClipping;

            foreach (var subpath in subject)
            {      
                // force close subject if need be
                if (subjectClose && !subpath.IsClosed()) subpath.ClosePath();
                // convert PdfPath to polygon
                var intPath = subpath.ToClipperPolygon(10).ToList();
                clipper.AddPath(intPath, PolyType.ptSubject, subjectClose);
            }

            var clippingFillType = clipping.FillingRule == FillingRule.NonZeroWinding ? PolyFillType.pftNonZero : PolyFillType.pftEvenOdd;

            if (!subjectClose)
            {
                // case where subject is not closed
                var solutions = new PolyTree();
                if (clipper.Execute(ClipType.ctIntersection, solutions, clippingFillType))
                {
                    PdfPathFix clippedPath = subject.CloneEmpty();
                    foreach (var solution in solutions.Childs)
                    {
                        PdfPath clipped = new PdfPath();
                        clipped.MoveTo((double)solution.Contour[0].X / factor, (double)solution.Contour[0].Y / factor);

                        for (int i = 1; i < solution.Contour.Count; i++)
                        {
                            clipped.LineTo((double)solution.Contour[i].X / factor, (double)solution.Contour[i].Y / factor);
                        }
                        clippedPath.Add(clipped);
                    }
                    return clippedPath;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // case where subject is closed
                var subjectFillType = subject.FillingRule == FillingRule.NonZeroWinding ? PolyFillType.pftNonZero : PolyFillType.pftEvenOdd;

                var solutions = new List<List<IntPoint>>();
                if (clipper.Execute(ClipType.ctIntersection, solutions, subjectFillType, clippingFillType))
                {
                    PdfPathFix clippedPath = subject.CloneEmpty();
                    foreach (var solution in solutions)
                    {
                        PdfPath clipped = new PdfPath();
                        clipped.MoveTo((double)solution[0].X / 10000, (double)solution[0].Y / 10000);

                        for (int i = 1; i < solution.Count; i++)
                        {
                            clipped.LineTo((double)solution[i].X / 10000, (double)solution[i].Y / 10000);
                        }
                        clipped.ClosePath();

                        clippedPath.Add(clipped);
                    }
                    return clippedPath;
                }
                else
                {
                    return null;
                }
            }
        }

        private static IEnumerable<IntPoint> ToClipperPolygon(this PdfPath pdfPath, int n = 4)
        {
            if (pdfPath.Commands.Count == 0)
            {
                yield break;
            }

            IntPoint previous;
            if (pdfPath.Commands[0] is Move currentMove)
            {
                previous = new IntPoint(currentMove.Location.X * factor, currentMove.Location.Y * factor);
                yield return previous;
            }
            else
            {
                throw new ArgumentException();
            }

            for (int i = 1; i < pdfPath.Commands.Count; i++)
            {
                var command = pdfPath.Commands[i];
                if (command is Move move)
                {
                    var location = new IntPoint(move.Location.X * factor, move.Location.Y * factor);
                    if (location != previous)
                    {
                        yield return location;
                        previous = location;
                        currentMove = move;
                    }
                }
                else if (command is Line line)
                {
                    var from = new IntPoint(line.From.X * factor, line.From.Y * factor);
                    if (!previous.Equals(from))
                    {
                        yield return from;
                        previous = from;
                    }

                    var to = new IntPoint(line.To.X * factor, line.To.Y * factor);
                    if (!previous.Equals(to))
                    {
                        yield return to;
                        previous = to;
                    }
                }
                else if (command is BezierCurve curve)
                {
                    foreach (var lineB in curve.ToLines(n))
                    {
                        var from = new IntPoint(lineB.From.X * factor, lineB.From.Y * factor);
                        if (!previous.Equals(from))
                        {
                            yield return from;
                            previous = from;
                        }

                        var to = new IntPoint(lineB.To.X * factor, lineB.To.Y * factor);
                        if (!previous.Equals(to))
                        {
                            yield return to;
                            previous = to;
                        }
                    }
                }
                else if (command is Close)
                {
                    yield return new IntPoint(currentMove.Location.X * factor, currentMove.Location.Y * factor);
                }
            }
        }
    }
}
