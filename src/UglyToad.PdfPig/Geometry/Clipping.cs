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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping"></param>
        /// <param name="path"></param>
        public static IEnumerable<PdfPath> Clip(this PdfPath clipping, PdfPath path)
        {
            if (clipping == null)
            {
                throw new ArgumentNullException(nameof(clipping), "Clip(): the clipping path cannot be null.");
            }

            if (!clipping.IsClipping)
            {
                throw new ArgumentException("Clip(): the clipping path does not have the IsClipping flag set to true.", nameof(clipping));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "Clip(): the path to be clipped cannot be null.");
            }

            var clippingRect = clipping.GetBoundingRectangle();
            if (!clippingRect.HasValue)
            {
                throw new ArgumentException("Clip(): clip bounding box not available.", nameof(clipping));
            }

            if (path.Commands.Count == 0)
            {
                yield return path;
                yield break;
            }

            var pathRect = path.GetBoundingRectangle();
            if (!pathRect.HasValue)
            {
                if (path.Commands.Count == 1 && path.Commands[0] is Move move)
                {
                    if (clipping.FillingRule == FillingRule.EvenOdd)
                    {
                        if (EvenOddRule(clipping.ToPolygon(10), move.Location))
                        {
                            yield return path;
                            yield break;
                        }
                        else
                        {
                            yield break;
                        }
                    }
                    else if (clipping.FillingRule == FillingRule.NonZeroWinding)
                    {
                        if (NonZeroWindingRule(clipping.ToPolygon(10), move.Location))
                        {
                            yield return path;
                            yield break;
                        }
                        else
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Clip(): path bounding box not available.", nameof(path));
                    }
                }
                else
                {
                    throw new ArgumentException("Clip(): path bounding box not available.", nameof(path));
                }
            }
            
            if (clippingRect.Value.Contains(pathRect.Value, true))
            {
                // path completly inside
                yield return path;
                yield break;
            }
            
            if (!clippingRect.Value.IntersectsWith(pathRect.Value))
            {
                // path completly outside
                yield break;
            }

            else if (clipping.IsDrawnAsRectangle) // rectangle clipping
            {
                if (path.IsDrawnAsRectangle) // rectangle clipped
                {
                    if (path.IsFilled || path.IsClipping) // rectangle w/ rectangle, filled
                    {
                        // Simplest case where both the clipping and the clipped path are axis aligned rectangles
                        var intersection = clippingRect.Value.Intersect(pathRect.Value);
                        if (intersection.HasValue)
                        {
                            PdfPath clipped = path.CloneEmpty();
                            clipped.Rectangle(intersection.Value.BottomLeft.X, intersection.Value.BottomLeft.Y,
                                              intersection.Value.Width, intersection.Value.Height);
                            yield return clipped;
                        }
                        else
                        {
                            throw new ArgumentException("They should intersect as we checked for that before.");
                        }
                    }
                    else // rectangle w/ rectangle, not filled
                    {
                        foreach (var clipped in LiangBarsky(clippingRect.Value.Left, clippingRect.Value.Right, clippingRect.Value.Bottom, clippingRect.Value.Top, path.Simplify(10)))
                        {
                            yield return clipped;
                        }
                    }
                }
                else // polyline or polygon clipped
                {
                    if (path.IsFilled || path.IsClipping) // polygon with rectangle
                    {
                        // could check for convexity to use Sutherland-Hodgman
                        foreach (var clipped in GreinerHormann(clipping, path.Simplify(10)))
                        {
                            yield return clipped;
                        }
                    }
                    else // polyline with rectangle
                    {
                        foreach (var clipped in LiangBarsky(clippingRect.Value.Left, clippingRect.Value.Right, clippingRect.Value.Bottom, clippingRect.Value.Top, path.Simplify(10)))
                        {
                            yield return clipped;
                        }
                    }
                }
            }
            else // polygon clipping
            {
                if (path.IsFilled || path.IsClipping) // polygon with polygon
                {
                    // check convex or not
                    foreach (var clipped in GreinerHormann(clipping, path.Simplify(10)))
                    {
                        yield return clipped;
                    }
                }
                else // polyline
                {
                    // check convex or not
                    // if convex -> CB

                    // we assume always convex for the moment
                    foreach (var clipped in CyrusBeck(clipping, path.Simplify(10)))
                    {
                        yield return clipped;
                    }
                }
            }
        }

        #region Winding Rules
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        public static bool EvenOddRule(IReadOnlyList<PdfPoint> polygon, PdfPoint point)
        {
            return GetWindingNumber(polygon, point) % 2 != 0; // odd=inside / even=outside
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        public static bool NonZeroWindingRule(IReadOnlyList<PdfPoint> polygon, PdfPoint point)
        {
            return GetWindingNumber(polygon, point) != 0;  // 0!=inside / 0=outside
        }

        private static int GetWindingNumber(IReadOnlyList<PdfPoint> polygon, PdfPoint point)
        {
            int count = 0;
            var previous = polygon[0];
            for (int i = 1; i < polygon.Count; i++)
            {
                var current = polygon[i];
                if (previous.Y <= point.Y)
                {
                    if (current.Y > point.Y && GeometryExtensions.ccw(previous, current, point))
                    {
                        count++;
                    }
                }
                else if (current.Y <= point.Y && !GeometryExtensions.ccw(previous, current, point))
                {
                    count--;
                }
                previous = current;
            }

            return count;
        }
        #endregion

        #region Cyrus-Beck
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clippingPath">Convex counter clockwise</param>
        /// <param name="path">polyline path.</param>
        /// <returns></returns>
        public static IEnumerable<PdfPath> CyrusBeck(PdfPath clippingPath, PdfPath path)
        {
            var lastX = double.NaN;
            var lastY = double.NaN;
            var clipped = path.CloneEmpty();

            PdfPoint previous;
            if (path.Commands[0] is Move move)
            {
                previous = move.Location;
            }
            else
            {
                throw new ArgumentException("CyrusBeck(): path's first command is not a Move command.", nameof(path));
            }

            var clipping = clippingPath.ToPolygon(10);

            for (int i = 1; i < path.Commands.Count; i++)
            {
                PdfPoint current;
                if (path.Commands[i] is Line line)
                {
                    current = line.To;
                }
                else if (path.Commands[i] is Close)
                {
                    current = move.Location;
                }
                else
                {
                    throw new ArgumentException("CyrusBeck(): path contains bezier curve.", nameof(path));
                }

                if (CyrusBeck(clipping, previous, current, out double x0clip, out double y0clip, out double x1clip, out double y1clip))
                {
                    if (clipped.Commands.Count == 0)
                    {
                        // new polygon
                        clipped.MoveTo(x0clip, y0clip);
                        clipped.LineTo(x1clip, y1clip);
                        lastX = x1clip;
                        lastY = y1clip;
                    }
                    else if (Math.Abs(lastX - x0clip) < GeometryExtensions.epsilon &&
                             Math.Abs(lastY - y0clip) < GeometryExtensions.epsilon)
                    {
                        // last point of polygon equal new start point of clipped line, we continue
                        clipped.LineTo(x1clip, y1clip);
                        lastX = x1clip;
                        lastY = y1clip;
                    }
                    else
                    {
                        // polygon and new clipped line are not connected
                        // we are done for this polygon, close polygon
                        yield return clipped;

                        clipped = path.CloneEmpty();
                        clipped.MoveTo(x0clip, y0clip);
                        clipped.LineTo(x1clip, y1clip);
                        lastX = x1clip;
                        lastY = y1clip;
                    }
                }
                else
                {
                    yield return clipped;
                    clipped = path.CloneEmpty();
                }

                previous = current;
            }

            if (clipped.Commands.Count > 0) yield return clipped;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping">Convex counter clockwise</param>
        /// <param name="polyLine"></param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyList<PdfPoint>> CyrusBeck(IReadOnlyList<PdfPoint> clipping, IReadOnlyList<PdfPoint> polyLine)
        {
            var lastX = double.NaN;
            var lastY = double.NaN;
            List<PdfPoint> clipped = new List<PdfPoint>();

            PdfPoint previous = polyLine[0];
            for (int i = 1; i < polyLine.Count; i++)
            {
                PdfPoint current = polyLine[i];

                if (CyrusBeck(clipping, previous, current, out double x0clip, out double y0clip, out double x1clip, out double y1clip))
                {
                    if (clipped.Count == 0)
                    {
                        // new polygon
                        clipped.Add(new PdfPoint(x0clip, y0clip));
                        clipped.Add(new PdfPoint(x1clip, y1clip));
                        lastX = x1clip;
                        lastY = y1clip;
                    }
                    else if (Math.Abs(lastX - x0clip) < GeometryExtensions.epsilon &&
                             Math.Abs(lastY - y0clip) < GeometryExtensions.epsilon)
                    {
                        // last point of polygon equal new start point of clipped line, we continue
                        clipped.Add(new PdfPoint(x1clip, y1clip));
                        lastX = x1clip;
                        lastY = y1clip;
                    }
                    else
                    {
                        // polygon and new clipped line are not connected
                        // we are done for this polygon, close polygon
                        yield return clipped;

                        clipped = new List<PdfPoint>
                        {
                            new PdfPoint(x0clip, y0clip),
                            new PdfPoint(x1clip, y1clip)
                        };
                        lastX = x1clip;
                        lastY = y1clip;
                    }
                }
                else
                {
                    yield return clipped;
                    clipped = new List<PdfPoint>();
                }

                previous = current;
            }

            if (clipped.Count > 0) yield return clipped;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping">Convex counter clockwise</param>
        /// <param name="p1src"></param>
        /// <param name="p2src"></param>
        /// <param name="x0clip"></param>
        /// <param name="y0clip"></param>
        /// <param name="x1clip"></param>
        /// <param name="y1clip"></param>
        /// <returns></returns>
        public static bool CyrusBeck(IReadOnlyList<PdfPoint> clipping, PdfPoint p1src, PdfPoint p2src,
            out double x0clip, out double y0clip, out double x1clip, out double y1clip)
        {
            if (!clipping.Last().Equals(clipping.First()))
            {
                throw new ArgumentException("CyrusBeck(): need a closed polygon as input.", nameof(clipping));
            }

            double tmin = 0;
            double tmax = 1.0;

            var sX = p2src.X - p1src.X;
            var sY = p2src.Y - p1src.Y;

            var current = clipping[0];

            for (int i = 1; i < clipping.Count; i++)
            {
                var siX = current.X - p1src.X;
                var siY = current.Y - p1src.Y;

                // n is a normal vector of edge ei(xi, xi+1), pointing outside of polygon
                var next = clipping[i];
                var nX = next.Y - current.Y;
                var nY = current.X - next.X;

                var k = nX * sX + nY * sY;

                if (Math.Abs(k) > GeometryExtensions.epsilon) // k != 0
                {
                    double t = (nX * siX + nY * siY) / k;
                    if (k > 0)
                    {
                        tmax = Math.Min(t, tmax);
                    }
                    else
                    {
                        tmin = Math.Max(t, tmin);
                    }
                }
                else
                {
                    // special case solution
                }
                current = next;
            }

            if (tmin > tmax)
            {
                x0clip = double.NaN;
                y0clip = double.NaN;
                x1clip = double.NaN;
                y1clip = double.NaN;
                return false;
            }

            x1clip = p1src.X + sX * tmax;
            y1clip = p1src.Y + sY * tmax;
            x0clip = p1src.X + sX * tmin;
            y0clip = p1src.Y + sY * tmin;
            return true;
        }

        #endregion

        #region Liang-Barsky
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeLeft"></param>
        /// <param name="edgeRight"></param>
        /// <param name="edgeBottom"></param>
        /// <param name="edgeTop"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<PdfPath> LiangBarsky(double edgeLeft, double edgeRight, double edgeBottom, double edgeTop, PdfPath path)
        {
            // TO DO: need to take in account if first == last (belong to same output polyline)

            var clipped = path.CloneEmpty();
            var lastX = double.NaN;
            var lastY = double.NaN;

            PdfPoint previous;
            if (path.Commands[0] is Move move)
            {
                previous = move.Location;
            }
            else
            {
                throw new ArgumentException("LiangBarsky(): path's first command is not a Move command.", nameof(path));
            }

            for (int i = 1; i < path.Commands.Count; i++)
            {
                bool drawLine = true;
                PdfPoint current;

                if (path.Commands[i] is Line line)
                {
                    current = line.To;
                }
                else if (path.Commands[i] is Close)
                {
                    current = move.Location;
                }
                else
                {
                    throw new ArgumentException("LiangBarsky(): path contains bezier curve.", nameof(path));
                }

                double t0 = 0.0;
                double t1 = 1.0;
                double xdelta = current.X - previous.X;
                double ydelta = current.Y - previous.Y;

                for (int edge = 0; edge < 4; edge++)
                {
                    double p;
                    double q;

                    // Traverse through left, right, bottom, top edges.
                    switch (edge)
                    {
                        case 0:
                            p = -xdelta;
                            q = -edgeLeft + previous.X;
                            break;

                        case 1:
                            p = xdelta;
                            q = edgeRight - previous.X;
                            break;

                        case 2:
                            p = -ydelta;
                            q = -edgeBottom + previous.Y;
                            break;

                        default: //case 3
                            p = ydelta;
                            q = edgeTop - previous.Y;
                            break;
                    }

                    if (p == 0 && q < 0)
                    {
                        // Don't draw line at all. (parallel line outside)
                        yield return clipped;
                        clipped = path.CloneEmpty();
                        drawLine = false;
                        break;
                    }
                    else
                    {
                        double r = q / p;
                        if (p < 0)
                        {
                            if (r > t1)
                            {
                                // Don't draw line at all.
                                yield return clipped;
                                clipped = path.CloneEmpty();
                                drawLine = false;
                                break;
                            }
                            else if (r > t0)
                            {
                                // Line is clipped!
                                t0 = r;
                            }
                        }
                        else if (p > 0)
                        {
                            if (r < t0)
                            {
                                // Don't draw line at all.
                                yield return clipped;
                                clipped = path.CloneEmpty();
                                drawLine = false;
                                break;
                            }
                            else if (r < t1)
                            {
                                // Line is clipped!
                                t1 = r;
                            }
                        }
                    }
                }

                if (drawLine)
                {
                    if (t0 == 0 && t1 == 1)
                    {
                        // points don't change so line is inside
                        if (clipped.Commands.Count == 0)
                        {
                            clipped.MoveTo(previous.X, previous.Y);
                        }
                        clipped.LineTo(current.X, current.Y);
                        lastX = current.X;
                        lastY = current.Y;
                    }
                    else
                    {
                        // (clipped) line is drawn
                        double x0clip = previous.X + t0 * xdelta;
                        double y0clip = previous.Y + t0 * ydelta;
                        double x1clip = previous.X + t1 * xdelta;
                        double y1clip = previous.Y + t1 * ydelta;

                        if (clipped.Commands.Count == 0)
                        {
                            // new polygon
                            clipped.MoveTo(x0clip, y0clip);
                            clipped.LineTo(x1clip, y1clip);
                            lastX = x1clip;
                            lastY = y1clip;
                        }
                        else if (Math.Abs(lastX - x0clip) < GeometryExtensions.epsilon &&
                                 Math.Abs(lastY - y0clip) < GeometryExtensions.epsilon)
                        {
                            // last point of polygon equal new start point of clipped line, we continue
                            clipped.LineTo(x1clip, y1clip);
                            lastX = x1clip;
                            lastY = y1clip;
                        }
                        else
                        {
                            // polygon and new clipped line are not connected
                            // we are done for this polygon, close polygon
                            yield return clipped;

                            clipped = path.CloneEmpty();
                            clipped.MoveTo(x0clip, y0clip);
                            clipped.LineTo(x1clip, y1clip);
                            lastX = x1clip;
                            lastY = y1clip;
                        }
                    }
                }

                previous = current;
            }

            if (clipped.Commands.Count > 0) yield return clipped;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeLeft"></param>
        /// <param name="edgeRight"></param>
        /// <param name="edgeBottom"></param>
        /// <param name="edgeTop"></param>
        /// <param name="polyLine"></param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyList<PdfPoint>> LiangBarsky(double edgeLeft, double edgeRight, double edgeBottom, double edgeTop, IReadOnlyList<PdfPoint> polyLine)
        {
            // TO DO: need to take in account if first == last (belong to same output polyline)
            var clipped = new List<PdfPoint>();
            var lastX = double.NaN;
            var lastY = double.NaN;

            PdfPoint previous = polyLine[0];
            for (int i = 1; i < polyLine.Count; i++)
            {
                bool drawLine = true;
                PdfPoint current = polyLine[i];

                double t0 = 0.0;
                double t1 = 1.0;
                double xdelta = current.X - previous.X;
                double ydelta = current.Y - previous.Y;

                for (int edge = 0; edge < 4; edge++)
                {
                    double p;
                    double q;

                    // Traverse through left, right, bottom, top edges.
                    switch (edge)
                    {
                        case 0:
                            p = -xdelta;
                            q = -edgeLeft + previous.X;
                            break;

                        case 1:
                            p = xdelta;
                            q = edgeRight - previous.X;
                            break;

                        case 2:
                            p = -ydelta;
                            q = -edgeBottom + previous.Y;
                            break;

                        default: //case 3
                            p = ydelta;
                            q = edgeTop - previous.Y;
                            break;
                    }

                    if (p == 0 && q < 0)
                    {
                        // Don't draw line at all. (parallel line outside)
                        yield return clipped; 
                        clipped = new List<PdfPoint>();
                        drawLine = false;
                        break;
                    }
                    else
                    {
                        double r = q / p;
                        if (p < 0)
                        {
                            if (r > t1)
                            {
                                // Don't draw line at all.
                                yield return clipped;
                                clipped = new List<PdfPoint>();
                                drawLine = false;
                                break;
                            }
                            else if (r > t0)
                            {
                                // Line is clipped!
                                t0 = r;
                            }
                        }
                        else if (p > 0)
                        {
                            if (r < t0)
                            {
                                // Don't draw line at all.
                                yield return clipped;
                                clipped = new List<PdfPoint>();
                                drawLine = false;
                                break;
                            }
                            else if (r < t1)
                            {
                                // Line is clipped!
                                t1 = r;
                            }
                        }
                    }
                }

                if (drawLine)
                {
                    if (t0 == 0 && t1 == 1)
                    {
                        // points don't change so line is inside
                        if (clipped.Count == 0)
                        {
                            clipped.Add(previous);
                        }
                        clipped.Add(current);
                        lastX = current.X;
                        lastY = current.Y;
                    }
                    else
                    {
                        // (clipped) line is drawn
                        double x0clip = previous.X + t0 * xdelta;
                        double y0clip = previous.Y + t0 * ydelta;
                        double x1clip = previous.X + t1 * xdelta;
                        double y1clip = previous.Y + t1 * ydelta;

                        if (clipped.Count == 0)
                        {
                            // new polygon
                            clipped.Add(new PdfPoint(x0clip, y0clip));
                            clipped.Add(new PdfPoint(x1clip, y1clip));
                            lastX = x1clip;
                            lastY = y1clip;
                        }
                        else if (Math.Abs(lastX - x0clip) < GeometryExtensions.epsilon &&
                                 Math.Abs(lastY - y0clip) < GeometryExtensions.epsilon)
                        {
                            // last point of polygon equal new start point of clipped line, we continue
                            clipped.Add(new PdfPoint(x1clip, y1clip));
                            lastX = x1clip;
                            lastY = y1clip;
                        }
                        else
                        {
                            // polygon and new clipped line are not connected
                            // we are done for this polygon, close polygon
                            yield return clipped;

                            clipped = new List<PdfPoint>
                            {
                                new PdfPoint(x0clip, y0clip),
                                new PdfPoint(x1clip, y1clip)
                            };
                            lastX = x1clip;
                            lastY = y1clip;
                        }
                    }
                }

                previous = current;
            }

            if (clipped.Count > 0 ) yield return clipped;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeLeft"></param>
        /// <param name="edgeRight"></param>
        /// <param name="edgeBottom"></param>
        /// <param name="edgeTop"></param>
        /// <param name="x0src"></param>
        /// <param name="y0src"></param>
        /// <param name="x1src"></param>
        /// <param name="y1src"></param>
        /// <param name="x0clip"></param>
        /// <param name="y0clip"></param>
        /// <param name="x1clip"></param>
        /// <param name="y1clip"></param>
        /// <returns></returns>
        public static bool LiangBarsky(double edgeLeft, double edgeRight, double edgeBottom, double edgeTop, // Define the x/y clipping values for the border.
                                  double x0src, double y0src, double x1src, double y1src,                             // Define the start and end points of the line.
                                  out double x0clip, out double y0clip, out double x1clip, out double y1clip)         // The output values, so declare these outside.
        {
            // Liang-Barsky function by Daniel White @ http://www.skytopia.com/project/articles/compsci/clipping.html
            // This function inputs 8 numbers, and outputs 4 new numbers (plus a boolean value to say whether the clipped line is drawn at all).

            x0clip = double.NaN;
            y0clip = double.NaN;
            x1clip = double.NaN;
            y1clip = double.NaN;

            double t0 = 0.0;
            double t1 = 1.0;
            double xdelta = x1src - x0src;
            double ydelta = y1src - y0src;

            for (int edge = 0; edge < 4; edge++)
            {
                double p;
                double q;

                // Traverse through left, right, bottom, top edges.
                switch (edge)
                {
                    case 0:
                        p = -xdelta;
                        q = -(edgeLeft - x0src);
                        break;

                    case 1:
                        p = xdelta;
                        q = (edgeRight - x0src);
                        break;

                    case 2:
                        p = -ydelta;
                        q = -(edgeBottom - y0src);
                        break;

                    default: //case 3
                        p = ydelta;
                        q = (edgeTop - y0src);
                        break;
                }

                if (p == 0 && q < 0) return false;  // Don't draw line at all. (parallel line outside)

                double r = q / p;
                if (p < 0)
                {
                    if (r > t1) return false;       // Don't draw line at all.
                    else if (r > t0) t0 = r;        // Line is clipped!
                }
                else if (p > 0)
                {
                    if (r < t0) return false;       // Don't draw line at all.
                    else if (r < t1) t1 = r;        // Line is clipped!
                }
            }

            x0clip = x0src + t0 * xdelta;
            y0clip = y0src + t0 * ydelta;
            x1clip = x0src + t1 * xdelta;
            y1clip = y0src + t1 * ydelta;

            return true;        // (clipped) line is drawn
        }
        #endregion

        #region Greiner-Hormann
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping"></param>
        /// <param name="polygon"></param>
        public static IEnumerable<PdfPath> GreinerHormann(PdfPath clipping, PdfPath polygon)
        {
            //if (clipping.Equals(polygon)) return new List<PdfPath>() { polygon };

            var clippingList = clipping.ToPolygon(10);
            var polygonList = polygon.ToPolygon(10);

            var clippeds = GreinerHormann(clippingList, polygonList, clipping.FillingRule);

            foreach (var clipped in clippeds)
            {
                if (clipped.Count > 0)
                {
                    PdfPath clippedPath = polygon.CloneEmpty();
                    var current = clipped[0];
                    clippedPath.MoveTo(current.Coordinates.X, current.Coordinates.Y);

                    for (int i = 1; i < clipped.Count; i++)
                    {
                        current = clipped[i];
                        clippedPath.LineTo(current.Coordinates.X, current.Coordinates.Y);
                    }
                    yield return clippedPath;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping"></param>
        /// <param name="polygon"></param>
        /// <param name="fillingRule"></param>
        /// <returns></returns>
        public static List<List<Vertex>> GreinerHormann(IReadOnlyList<PdfPoint> clipping, IReadOnlyList<PdfPoint> polygon,
            FillingRule fillingRule)
        {
            if (clipping.Count < 3)
            {
                throw new ArgumentException("GreinerHormann(): clipping should contain more that 3 points.", nameof(clipping));
            }

            if (polygon.Count == 0)
            {
                return new List<List<Vertex>>();
            }
            else if (polygon.Count == 1)
            {
                if (NonZeroWindingRule(clipping, polygon[0]))
                {
                    return new List<List<Vertex>>() { new List<Vertex>() { new Vertex() { Coordinates = polygon[0], IsClipping = false } } };
                }
                else
                {
                    return new List<List<Vertex>>();
                }
            }

            if (!clipping.Last().Equals(clipping.First()))
            {
                throw new ArgumentException("GreinerHormann(): need a closed clipping polygon as input.", nameof(clipping));
            }

            static double squaredDist(PdfPoint point1, PdfPoint point2)
            {
                double dx = point1.X - point2.X;
                double dy = point1.Y - point2.Y;
                return dx * dx + dy * dy;
            }

            Func<IReadOnlyList<PdfPoint>, PdfPoint, bool> isInside = NonZeroWindingRule;
            if (fillingRule == FillingRule.EvenOdd) isInside = EvenOddRule;

            LinkedList<Vertex> subject = new LinkedList<Vertex>();
            foreach (var point in polygon)
            {
                subject.AddLast(new Vertex() { Coordinates = point, IsClipping = false });
            }

            // force close
            if (!subject.Last.Value.Coordinates.Equals(subject.First.Value.Coordinates))
            {
                subject.AddLast(new Vertex() { Coordinates = subject.First.Value.Coordinates, IsClipping = false, IsFake = true });
            }

            LinkedList<Vertex> clip = new LinkedList<Vertex>();
            foreach (var point in clipping)
            {
                clip.AddLast(new Vertex() { Coordinates = point, IsClipping = true });
            }

            bool hasIntersection = false;

            // phase 1
            for (var Si = subject.First; Si != subject.Last; Si = Si.Next)
            {
                if (Si.Value.Intersect) continue;
                for (var Cj = clip.First; Cj != clip.Last; Cj = Cj.Next)
                {
                    if (Cj.Value.Intersect) continue;
                    var SiNext = Si.Next;
                    while (SiNext.Value.Intersect)
                    {
                        SiNext = SiNext.Next;
                    }

                    var CjNext = Cj.Next;
                    while (CjNext.Value.Intersect)
                    {
                        CjNext = CjNext.Next;
                    }

                    var intersection = GeometryExtensions.Intersect(Si.Value.Coordinates, SiNext.Value.Coordinates, Cj.Value.Coordinates, CjNext.Value.Coordinates);
                    if (intersection.HasValue)
                    {
                        hasIntersection = true;

                        bool isFake = Si.Value.IsFake || SiNext.Value.IsFake;

                        var a = squaredDist(Si.Value.Coordinates, intersection.Value) / squaredDist(Si.Value.Coordinates, SiNext.Value.Coordinates);
                        var b = squaredDist(Cj.Value.Coordinates, intersection.Value) / squaredDist(Cj.Value.Coordinates, CjNext.Value.Coordinates);

                        var i1 = new Vertex() { Coordinates = intersection.Value, Intersect = true, Alpha = (float)a, IsClipping = false, IsFake = isFake };
                        var i2 = new Vertex() { Coordinates = intersection.Value, Intersect = true, Alpha = (float)b, IsClipping = true, IsFake = isFake };

                        var tempSi = Si;
                        while (tempSi != SiNext && tempSi.Value.Alpha < i1.Alpha)
                        {
                            tempSi = tempSi.Next;
                        }
                        var neighbour2 = subject.AddBefore(tempSi, i1);

                        var tempCj = Cj;
                        while (tempCj != CjNext && tempCj.Value.Alpha < i2.Alpha)
                        {
                            tempCj = tempCj.Next;
                        }
                        var neighbour1 = clip.AddBefore(tempCj, i2);

                        i1.Neighbour = neighbour1;
                        i2.Neighbour = neighbour2;
                    }
                }
            }

            // phase 2
            var statusPoly = true;
            var polyInside = isInside(clipping, subject.First.Value.Coordinates);
            if (polyInside)
            {
                statusPoly = false;
            }

            for (var node = subject.First; node != subject.Last.Next; node = node.Next)
            {
                if (node.Value.Intersect)
                {
                    node.Value.EntryExit = statusPoly;
                    statusPoly = !statusPoly;
                }
            }

            var statusClip = true;
            var clipInside = isInside(polygon, clip.First.Value.Coordinates);
            if (clipInside)
            {
                statusClip = false;
            }

            for (var node = clip.First; node != clip.Last.Next; node = node.Next)
            {
                if (node.Value.Intersect)
                {
                    node.Value.EntryExit = statusClip;
                    statusClip = !statusClip;
                }
            }

            // phase 3
            if (!hasIntersection) // no intersection
            {
                if (polyInside)
                {
                    return new List<List<Vertex>>() { subject.ToList() };
                }
                else if (clipInside)
                {
                    return new List<List<Vertex>>() { clip.ToList() };
                }

                return new List<List<Vertex>>();
            }

            List<List<Vertex>> polygons = new List<List<Vertex>>();
            while (true)
            {
                var current = subject.Find(subject.Where(x => x.Intersect && !x.IsProcessed).First());
                List<Vertex> newPolygon = new List<Vertex>
                {
                    current.Value
                };

                while (true)
                {
                    current.Value.Processed();

                    if (current.Value.EntryExit)
                    {
                        while (true)
                        {
                            if (current.Next == null)
                            {
                                current = current.List.First;
                            }
                            else
                            {
                                current = current.Next;
                            }

                            newPolygon.Add(current.Value);
                            if (current.Value.Intersect) break;
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            if (current.Previous == null)
                            {
                                current = current.List.Last;
                            }
                            else
                            {
                                current = current.Previous;
                            }

                            newPolygon.Add(current.Value);
                            if (current.Value.Intersect) break;
                        }
                    }

                    current = current.Value.Neighbour;
                    if (current.Value.IsProcessed) break;
                }
                polygons.Add(newPolygon);

                if (!subject.Where(x => x.Intersect && !x.IsProcessed).Any()) break;
            }

            return polygons;
        }

        /// <summary>
        /// 
        /// </summary>
        public class Vertex
        {
            /// <summary>
            /// 
            /// </summary>
            public PdfPoint Coordinates { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool Intersect { get; set; }

            /// <summary>
            /// true for Entry, false for Exit.
            /// </summary>
            public bool EntryExit { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public LinkedListNode<Vertex> Neighbour { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public float Alpha { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool IsProcessed { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public bool IsClipping { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool IsFake { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public void Processed()
            {
                IsProcessed = true;
                Neighbour.Value.IsProcessed = true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Coordinates + ", " + Alpha.ToString("0.000") + ", " + Intersect + (IsClipping ? ", clipping" : "") + (IsFake ? ", fake" : "");
            }
        }
        #endregion

        #region Sutherland-Hodgman
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static PdfPath SutherlandHodgman(PdfPath clipping, PdfPath polygon)
        {
            var clippingList = clipping.ToPolygon(10);
            var polygonList = polygon.ToPolygon(10);

            var clipped = SutherlandHodgman(clippingList.Distinct().ToList(), polygonList.ToList());

            PdfPath clippedPath = polygon.CloneEmpty();

            if (clipped.Count > 0)
            {
                clippedPath.MoveTo(clipped.First().X, clipped.First().Y);
                for (int i = 1; i < clipped.Count; i++)
                {
                    var current = clipped[i];
                    clippedPath.LineTo(current.X, current.Y);
                }

                return clippedPath;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clipping">The clipping polygon. Should be convex, in counter-clockwise order.</param>
        /// <param name="polygon">The polygon to be clipped. Preferably convex.</param>
        public static IReadOnlyList<PdfPoint> SutherlandHodgman(IReadOnlyList<PdfPoint> clipping, IReadOnlyList<PdfPoint> polygon)
        {
            /*
             * This algorithm does not work if the clip window is not convex.
             * If the polygon is not also convex, there may be some dangling edges. 
             */
            if (clipping.Count < 3)
            {
                throw new ArgumentException();
            }

            if (polygon.Count == 0)
            {
                return polygon;
            }
            else if (polygon.Count == 1)
            {
                if (NonZeroWindingRule(clipping, polygon[0]))
                {
                    return polygon;
                }
                else
                {
                    return new List<PdfPoint>();
                }
            }

            if (!clipping.Last().Equals(clipping.First()))
            {
                throw new ArgumentException("SutherlandHodgman(): need a closed clipping polygon as input.", nameof(clipping));
            }

            if (!polygon.Last().Equals(polygon.First()))
            {
                throw new ArgumentException("SutherlandHodgman(): need a closed polygon to clip as input.", nameof(polygon));
            }

            List<PdfPoint> outputList = polygon.ToList();

            PdfPoint edgeP1 = clipping[0];
            for (int e = 1; e < clipping.Count; e++)
            {
                if (outputList.Count == 0) break;

                List<PdfPoint> inputList = outputList.ToList();
                outputList.Clear();

                PdfPoint edgeP2 = clipping[e];

                PdfPoint previous = inputList[inputList.Count - 1];
                for (int i = 0; i < inputList.Count; i++)
                {
                    PdfPoint current = inputList[i];
                    if (GeometryExtensions.ccw(edgeP1, edgeP2, current))
                    {
                        if (!GeometryExtensions.ccw(edgeP1, edgeP2, previous))
                        {
                            PdfPoint? intersection = GeometryExtensions.IntersectInfiniteLines(edgeP1, edgeP2, previous, current);
                            if (intersection.HasValue)
                            {
                                outputList.Add(intersection.Value);
                            }
                        }
                        outputList.Add(current);
                    }
                    else if (GeometryExtensions.ccw(edgeP1, edgeP2, previous))
                    {
                        PdfPoint? intersection = GeometryExtensions.IntersectInfiniteLines(edgeP1, edgeP2, previous, current);
                        if (intersection.HasValue)
                        {
                            outputList.Add(intersection.Value);
                        }
                    }
                    previous = current;
                }
                edgeP1 = edgeP2;
            }

            // force close 
            if (!outputList[0].Equals(outputList[outputList.Count - 1])) outputList.Add(outputList[0]);
            return outputList;
        }
        #endregion
    }
}
