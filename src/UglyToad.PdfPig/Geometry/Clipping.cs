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
        const int factor = 10_000;
        const int linesInCurve = 10;

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

            foreach (var subPathClipping in clipping)
            {
                if (subPathClipping.Commands.Count == 0)
                {
                    Console.WriteLine("subPathClipping empty");
                    continue;
                }
                // force close clipping polygon
                if (!subPathClipping.IsClosed())
                {
                    Console.WriteLine("force close clipping polygon");
                    subPathClipping.CloseSubpath();
                }
                var subPathClippingClipper = subPathClipping.ToClipperPolygon().ToList();
                clipper.AddPath(subPathClippingClipper, PolyType.ptClip, true);
            }

            bool subjectClose = subject.IsFilled || subject.IsClipping;
            foreach (var subPathSubject in subject)
            {
                if (subPathSubject.Commands.Count == 0)
                {
                    Console.WriteLine("subPathSubject empty");
                    continue;
                }
                // force close subject if need be
                if (subjectClose && !subPathSubject.IsClosed())
                {
                    Console.WriteLine("force close subject if need be");
                    subPathSubject.CloseSubpath();
                }
                var subPathSubjectClipper = subPathSubject.ToClipperPolygon().ToList();
                clipper.AddPath(subPathSubjectClipper, PolyType.ptSubject, subjectClose);
            }

            var clippingFillType = clipping.FillingRule == FillingRule.NonZeroWinding ? PolyFillType.pftNonZero : PolyFillType.pftEvenOdd;
            var subjectFillType = subject.FillingRule == FillingRule.NonZeroWinding ? PolyFillType.pftNonZero : PolyFillType.pftEvenOdd;

            if (!subjectClose)
            {
                PdfPath clippedPath = subject.CloneEmpty();
                // case where subject is not closed
                var solutions = new PolyTree();
                if (clipper.Execute(ClipType.ctIntersection, solutions, subjectFillType, clippingFillType))
                {
                    foreach (var solution in solutions.Childs)
                    {
                        PdfSubpath clippedSubpath = new PdfSubpath();
                        clippedSubpath.MoveTo((double)solution.Contour[0].X / factor, (double)solution.Contour[0].Y / factor);

                        for (int i = 1; i < solution.Contour.Count; i++)
                        {
                            clippedSubpath.LineTo((double)solution.Contour[i].X / factor, (double)solution.Contour[i].Y / factor);
                        }
                        clippedPath.Add(clippedSubpath);
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
                PdfPath clippedPath = subject.CloneEmpty();
                // case where subject is closed
                var solutions = new List<List<IntPoint>>();
                if (clipper.Execute(ClipType.ctIntersection, solutions, subjectFillType, clippingFillType))
                {
                    foreach (var solution in solutions)
                    {
                        PdfSubpath clippedSubpath = new PdfSubpath();
                        clippedSubpath.MoveTo((double)solution[0].X / factor, (double)solution[0].Y / factor);

                        for (int i = 1; i < solution.Count; i++)
                        {
                            clippedSubpath.LineTo((double)solution[i].X / factor, (double)solution[i].Y / factor);
                        }
                        clippedSubpath.CloseSubpath();

                        clippedPath.Add(clippedSubpath);
                    }
                    return clippedPath;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Allows duplicate points as they will be removed by Clipper.
        /// </summary>
        private static IEnumerable<IntPoint> ToClipperPolygon(this PdfSubpath pdfPath)
        {
            if (pdfPath.Commands.Count == 0)
            {
                yield break;
            }

            if (pdfPath.Commands[0] is Move currentMove)
            {
                var previous = new IntPoint(currentMove.Location.X * factor, currentMove.Location.Y * factor);
                yield return previous;
                if (pdfPath.Commands.Count == 1) yield break;
            }
            else
            {
                throw new ArgumentException("ToClipperPolygon(): First command is not a Move command. Type is '" + pdfPath.Commands[0].GetType().ToString() + "'.");
            }

            Console.WriteLine(pdfPath.Commands.Count);
            for (int i = 1; i < pdfPath.Commands.Count; i++)
            {
                var command = pdfPath.Commands[i];
                if (command is Move move)
                {
                    throw new ArgumentException("ToClipperPolygon(): another move found in subpath.");
                    //yield return new IntPoint(move.Location.X * factor, move.Location.Y * factor);
                    //currentMove = move;
                }
                else if (command is Line line)
                {
                    yield return new IntPoint(line.From.X * factor, line.From.Y * factor);
                    yield return new IntPoint(line.To.X * factor, line.To.Y * factor);
                }
                else if (command is BezierCurve curve)
                {
                    foreach (var lineB in curve.ToLines(linesInCurve))
                    {
                        yield return new IntPoint(lineB.From.X * factor, lineB.From.Y * factor);
                        yield return new IntPoint(lineB.To.X * factor, lineB.To.Y * factor);
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
