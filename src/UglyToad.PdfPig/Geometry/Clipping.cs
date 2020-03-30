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

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "Clip(): the path to be clipped cannot be null.");
            }

            if (!clipping.IsClipping)
            {
                throw new ArgumentException("Clip(): the clipping path does not have the IsClipping flag set to true.", nameof(clipping));
            }

            var clippingRect = clipping.GetBoundingRectangle();
            if (!clippingRect.HasValue)
            {
                throw new ArgumentException();
            }

            var pathRect = path.GetBoundingRectangle();
            if (!pathRect.HasValue)
            {
                throw new ArgumentException();
            }

            if (clippingRect.Value.Contains(pathRect.Value, true))
            {
                // path completly inside
                yield return path;
            }

            if (!clippingRect.Value.IntersectsWith(pathRect.Value))
            {
                // path completly outside
                yield break;
            }

            // to check if polygon is filled: path.FillingRule == FillingRule.None
            if (clipping.IsDrawnAsRectangle)
            {
                if (path.IsDrawnAsRectangle)
                {
                    if (path.IsFilled || path.IsClipping)
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
                    else
                    {
                        //Console.WriteLine("Multi line Liang-Barsky clipping");
                        foreach (var clipped in LiangBarskyClipping(clippingRect.Value.Left, clippingRect.Value.Right, clippingRect.Value.Bottom, clippingRect.Value.Top, path.Simplify(10)))
                        {
                            yield return clipped;
                        }
                    }
                }
                else
                {
                    if (path.IsFilled || path.IsClipping) // path.IsClosed() ||
                    {
                        //Console.WriteLine("Greiner-Hormann clipping.");
                        // hardcore clipping
                        foreach (var clipped in GreinerHormannClipping(clipping, path))
                        {
                            yield return clipped;
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Multi line Liang-Barsky clipping");
                        foreach (var clipped in LiangBarskyClipping(clippingRect.Value.Left, clippingRect.Value.Right, clippingRect.Value.Bottom, clippingRect.Value.Top, path.Simplify(10)))
                        {
                            yield return clipped;
                        }
                    }
                }
            }
            else
            {
                //Console.WriteLine("Greiner-Hormann clipping.");
                foreach (var clipped in GreinerHormannClipping(clipping, path))
                {
                    yield return clipped;
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
        /// <param name="clipping">convex counter-clockwise</param>
        /// <param name="xA"></param>
        /// <param name="xB"></param>
        /// <returns></returns>
        public static (PdfPoint p1, PdfPoint p2)? CyrusBeckClipping(IReadOnlyList<PdfPoint> clipping, PdfPoint xA, PdfPoint xB)
        {
            if (!clipping.Last().Equals(clipping.First()))
            {
                throw new ArgumentException("CyrusBeckClipping(): need a closed polygon as input.", nameof(clipping));
            }

            int N = clipping.Count;
            double tmin = 0;
            double tmax = 1.0;

            var sX = xB.X - xA.X;
            var sY = xB.Y - xA.Y;

            var current = clipping[0];

            for (int i = 1; i < N; i++)
            {
                var siX = current.X - xA.X;
                var siY = current.Y - xA.Y;

                // n is a normal vector of edge ei(xi, xi+1), pointing outside of polygon
                var next = clipping[i];
                var nX = next.Y - current.Y;
                var nY = -(next.X - current.X);

                var k = nX * sX + nY * sY;

                if (Math.Abs(k)> GeometryExtensions.epsilon) // k != 0
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

            if (tmin > tmax) return null;
            xB = new PdfPoint(xA.X + sX * tmax, xA.Y + sY * tmax);
            xA = new PdfPoint(xA.X + sX * tmin, xA.Y + sY * tmin);

            return (xA, xB);
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
        public static IEnumerable<PdfPath> LiangBarskyClipping(double edgeLeft, double edgeRight, double edgeBottom, double edgeTop, PdfPath path)
        {
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
                throw new ArgumentException("LiangBarskyClipping(): path's first command is not a Move command.", nameof(path));
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
                    throw new ArgumentException("LiangBarskyClipping(): path contains bezier curve.", nameof(path));
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
        public static IEnumerable<IReadOnlyList<PdfPoint>> LiangBarskyClipping(double edgeLeft, double edgeRight, double edgeBottom, double edgeTop, IReadOnlyList<PdfPoint> polyLine)
        {
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

                            clipped = new List<PdfPoint>();
                            clipped.Add(new PdfPoint(x0clip, y0clip));
                            clipped.Add(new PdfPoint(x1clip, y1clip));
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
        public static bool LiangBarskyClipping(double edgeLeft, double edgeRight, double edgeBottom, double edgeTop, // Define the x/y clipping values for the border.
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
        public static IEnumerable<PdfPath> GreinerHormannClipping(PdfPath clipping, PdfPath polygon)
        {
            //if (clipping.Equals(polygon)) return new List<PdfPath>() { polygon };

            var clippingList = clipping.ToPolygon(10);
            var polygonList = polygon.ToPolygon(10);

            var clippeds = GreinerHormannClipping(clippingList, polygonList, clipping.FillingRule);

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
        public static List<List<Vertex>> GreinerHormannClipping(IReadOnlyList<PdfPoint> clipping, IReadOnlyList<PdfPoint> polygon,
            FillingRule fillingRule)
        {
            if (clipping.Count < 3)
            {
                throw new ArgumentException("GreinerHormannClipping(): clipping shoul contain more that 3 points.", nameof(clipping));
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

            // force close
            if (!clip.Last.Value.Coordinates.Equals(clip.Last.Value.Coordinates))
            {
                clip.AddLast(new Vertex() { Coordinates = clip.First.Value.Coordinates, IsClipping = false, IsFake = true });
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
                List<Vertex> newPolygon = new List<Vertex>();

                newPolygon.Add(current.Value);

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
        public static PdfPath SutherlandHodgmanClipping(PdfPath clipping, PdfPath polygon)
        {
            var clippingList = clipping.ToPolygon(10);
            var polygonList = polygon.ToPolygon(10);

            var clipped = SutherlandHodgmanClipping(clippingList.Distinct().ToList(), polygonList.ToList());

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
        /// <param name="polygon">The polygon to be clipped.</param>
        public static IReadOnlyList<PdfPoint> SutherlandHodgmanClipping(IReadOnlyList<PdfPoint> clipping, IReadOnlyList<PdfPoint> polygon)
        {
            if (clipping.Count < 3)
            {
                throw new ArgumentException();
                //return polygon;
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

            List<PdfPoint> outputList = polygon.ToList();

            PdfPoint edgeP1 = clipping[clipping.Count - 1];
            for (int e = 0; e < clipping.Count; e++)
            {
                List<PdfPoint> inputList = outputList.ToList();
                outputList.Clear();

                if (inputList.Count == 0) break;

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
            return outputList;
        }
        #endregion
    }
}
