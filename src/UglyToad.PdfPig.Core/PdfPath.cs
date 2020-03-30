namespace UglyToad.PdfPig.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UglyToad.PdfPig.Core.Graphics;
    using UglyToad.PdfPig.Core.Graphics.Colors;

    /// <summary>
    /// A path in a PDF document, used by glyphs and page content. Can contain multiple sub-paths.
    /// </summary>
    public class PdfPath
    {
        private readonly List<IPathCommand> commands = new List<IPathCommand>();

        /// <summary>
        /// The sequence of sub-paths which form this <see cref="PdfPath"/>.
        /// </summary>
        public IReadOnlyList<IPathCommand> Commands => commands;

        /// <summary>
        /// True if the <see cref="PdfPath"/> was originaly draw as an axis aligned rectangle.
        /// </summary>
        public bool IsDrawnAsRectangle { get; internal set; }

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

        private PdfPoint? currentPosition;

        private double shoeLaceSum;

        /// <summary>
        /// Return true if points are organised in a clockwise order. Works only with closed paths.
        /// </summary>
        /// <returns></returns>
        public bool IsClockwise
        {
            get
            {
                if (!IsClosed()) return false;
                return shoeLaceSum > 0;
            }
        }

        /// <summary>
        /// Return true if points are organised in a counterclockwise order. Works only with closed paths.
        /// </summary>
        /// <returns></returns>
        public bool IsCounterClockwise
        {
            get
            {
                if (!IsClosed()) return false;
                return shoeLaceSum < 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsConvex()
        {
            // https://math.stackexchange.com/questions/1743995/determine-whether-a-polygon-is-convex-based-on-its-vertices
            var vertexlist = GetPoints().ToList();

            if (vertexlist.Count < 3) return false;
            double wSign = 0;                                       // First nonzero orientation (positive or negative)

            var xSign = 0;
            var xFirstSign = 0;                                     // Sign of first nonzero edge vector x
            var xFlips = 0;                                         // Number of sign changes in x

            var ySign = 0;
            var yFirstSign = 0;                                     // Sign of first nonzero edge vector y
            var yFlips = 0;                                         // Number of sign changes in y

            PdfPoint curr = vertexlist[vertexlist.Count - 2];       // Second-to-last vertex
            PdfPoint next = vertexlist[vertexlist.Count - 1];       // Last vertex

            foreach (var v in vertexlist)                           // Each vertex, in order
            {
                var prev = curr;                                    // Previous vertex
                curr = next;                                        // Current vertex
                next = v;                                           // Next vertex

                // Previous edge vector ("before"):
                var bx = curr.X - prev.X;
                var by = curr.Y - prev.Y;

                // Next edge vector ("after"):
                var ax = next.X - curr.X;
                var ay = next.Y - curr.Y;

                // Calculate sign flips using the next edge vector ("after"),
                // recording the first sign.
                if (ax > 0)
                {
                    if (xSign == 0)
                    {
                        xFirstSign = +1;
                    }
                    else if (xSign < 0)
                    {
                        xFlips++;
                    }
                    xSign = +1;
                }
                else if (ax < 0)
                {
                    if (xSign == 0)
                    {
                        xFirstSign = -1;
                    }
                    else if (xSign > 0)
                    {
                        xFlips++;
                    }
                    xSign = -1;
                }

                if (xFlips > 2)
                {
                    return false;
                }

                if (ay > 0)
                {
                    if (ySign == 0)
                    {
                        yFirstSign = +1;
                    }
                    else if (ySign < 0)
                    {
                        yFlips++;
                    }
                    ySign = +1;
                }
                else if (ay < 0)
                {
                    if (ySign == 0)
                    {
                        yFirstSign = -1;
                    }
                    else if (ySign > 0)
                    {
                        yFlips++;
                    }
                    ySign = -1;
                }

                if (yFlips > 2)
                {
                    return false;
                }

                // Find out the orientation of this pair of edges,
                // and ensure it does not differ from previous ones.
                var w = bx * ay - ax * by;
                if (wSign == 0 && w != 0)
                {
                    wSign = w;
                }
                else if (wSign > 0 && w < 0)
                {
                    return false;
                }
                else if (wSign < 0 && w > 0)
                {
                    return false;
                }
            }

            // Final/wraparound sign flips:
            if (xSign != 0 && xFirstSign != 0 && xSign != xFirstSign)
            {
                xFlips++;
            }

            if (ySign != 0 && yFirstSign != 0 && ySign != yFirstSign)
            {
                yFlips++;
            }

            // Concave polygons have two sign flips along each axis.
            if (xFlips != 2 || yFlips != 2)
            {
                return false;
            }

            // This is a convex polygon.
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consecutiveDuplicates">true to allow, false to remove.</param>
        /// <returns></returns>
        public IEnumerable<PdfPoint> GetPoints(bool consecutiveDuplicates = false)
        {
            PdfPoint firstPoint;
            PdfPoint previous;
            if (Commands[0] is Move move)
            {
                firstPoint = move.Location;
                previous = move.Location;
                yield return previous;
            }
            else
            {
                throw new ArgumentException();
            }

            for (int i = 1; i < Commands.Count; i++)
            {
                var command = Commands[i];
                if (command is Move)
                {
                    throw new ArgumentException();
                }
                else if (command is Line line)
                {
                    if (!previous.Equals(line.From))
                    {
                        previous = line.From;
                        yield return line.From;
                    }

                    if (!previous.Equals(line.To))
                    {
                        previous = line.To;
                        yield return line.To;
                    }
                }
                else if (command is BezierCurve curve)
                {
                    if (!curve.StartPoint.Equals(previous))
                    {
                        previous = curve.StartPoint;
                        yield return curve.StartPoint;
                    }

                    if (!curve.FirstControlPoint.Equals(previous))
                    {
                        previous = curve.FirstControlPoint;
                        yield return curve.FirstControlPoint;
                    }

                    if (!curve.SecondControlPoint.Equals(previous))
                    {
                        previous = curve.SecondControlPoint;
                        yield return curve.SecondControlPoint;
                    }

                    if (!curve.EndPoint.Equals(previous))
                    {
                        previous = curve.EndPoint;
                        yield return curve.EndPoint;
                    }
                }
                else // close
                {
                    if (!previous.Equals(firstPoint))
                    {
                        previous = firstPoint;
                        yield return firstPoint;
                    }
                }
            }
        }

        /// <summary>
        /// Get the <see cref="PdfPath"/>'s centroid point.
        /// </summary>
        public PdfPoint GetCentroid()
        {
            var filtered = commands.Where(c => c is Line || c is BezierCurve).ToList();
            if (filtered.Count == 0) return new PdfPoint();
            var points = filtered.Select(GetStartPoint).ToList();
            points.AddRange(filtered.Select(GetEndPoint));
            return new PdfPoint(points.Average(p => p.X), points.Average(p => p.Y));
        }

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
        
        internal static PdfPoint GetStartPoint(IPathCommand command)
        {
            if (command is Line line)
            {
                return line.From;
            }

            if (command is BezierCurve curve)
            {
                return curve.StartPoint;
            }

            if (command is Move move)
            {
                return move.Location;
            }

            throw new ArgumentException();
        }

        internal static PdfPoint GetEndPoint(IPathCommand command)
        {
            if (command is Line line)
            {
                return line.To;
            }

            if (command is BezierCurve curve)
            {
                return curve.EndPoint;
            }

            if (command is Move move)
            {
                return move.Location;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Simplify this <see cref="PdfPath"/> by converting bezier curves in to <see cref="Line"/>s.
        /// </summary>
        /// <param name="n">Number of lines required (minimum is 1).</param>
        public PdfPath Simplify(int n = 4)
        {
            if (Commands.Count == 0)
            {
                return this;
            }

            if (IsDrawnAsRectangle)
            {
                return this;
            }

            if (!Commands.Any(c => c is BezierCurve))
            {
                return this;
            }

            PdfPath simplifiedPath = this.CloneEmpty();

            foreach (var command in Commands)
            {
                if (command is Move move)
                {
                    simplifiedPath.MoveTo(move.Location.X, move.Location.Y);
                }
                else if (command is Line line)
                {
                    simplifiedPath.LineTo(line.To.X, line.To.Y);
                }
                else if (command is BezierCurve curve)
                {
                    foreach (var lineB in curve.ToLines(n))
                    {
                        simplifiedPath.LineTo(lineB.To.X, lineB.To.Y);
                    }
                }
                else if (command is Close close)
                {
                    simplifiedPath.ClosePath();
                }
            }

            return simplifiedPath;
        }

        /// <summary>
        /// Add a <see cref="Move"/> command to the path.
        /// </summary>
        public void MoveTo(double x, double y)
        {
            currentPosition = new PdfPoint(x, y);
            commands.Add(new Move(currentPosition.Value));
        }

        /// <summary>
        /// Add a <see cref="Line"/> command to the path.
        /// </summary>
        public void LineTo(double x, double y)
        {
            if (currentPosition.HasValue)
            {
                shoeLaceSum += ((x - currentPosition.Value.X) * (y + currentPosition.Value.Y));

                var to = new PdfPoint(x, y);
                commands.Add(new Line(currentPosition.Value, to));
                currentPosition = to;
            }
            else
            {
                // TODO: probably the wrong behaviour here, maybe line starts from (0, 0)?
                //MoveTo(x, y);
                // PDF Reference 1.7 p226
                throw new ArgumentNullException("LineTo(): currentPosition is null.");
            }
        }

        /// <summary>
        /// Adds 4 <see cref="Line"/>s forming a rectangle to the path.
        /// </summary>
        public void Rectangle(double x, double y, double width, double height)
        {
            MoveTo(x, y);
            LineTo(x + width, y);
            LineTo(x + width, y + height);
            LineTo(x, y + height);
            ClosePath();
            IsDrawnAsRectangle = true;
        }

        internal void QuadraticCurveTo(double x1, double y1, double x2, double y2) { }

        /// <summary>
        /// Add a <see cref="BezierCurve"/> to the path.
        /// </summary>
        public void BezierCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            if (currentPosition.HasValue)
            {
                shoeLaceSum += (x1 - currentPosition.Value.X) * (y1 + currentPosition.Value.Y);
                shoeLaceSum += (x2 - x1) * (y2 + y1);
                shoeLaceSum += (x3 - x2) * (y3 + y2);

                var to = new PdfPoint(x3, y3);
                commands.Add(new BezierCurve(currentPosition.Value, new PdfPoint(x1, y1), new PdfPoint(x2, y2), to));
                currentPosition = to;
            }
            else
            {
                //MoveTo(x3, y3);
                // PDF Reference 1.7 p226
                throw new ArgumentNullException("BezierCurveTo(): currentPosition is null.");
            }
        }

        /// <summary>
        /// Close the path.
        /// </summary>
        public void ClosePath()
        {
            if (currentPosition.HasValue)
            {
                var startPoint = GetStartPoint(commands.First());
                if (!startPoint.Equals(currentPosition.Value))
                {
                    shoeLaceSum += (startPoint.X - currentPosition.Value.X) * (startPoint.Y + currentPosition.Value.Y);
                }
            }
            commands.Add(new Close());
        }

        /// <summary>
        /// Determines if the path is currently closed.
        /// </summary>
        public bool IsClosed()
        {
            // need to check if filled -> true if filled
            if (Commands.Any(c => c is Close)) return true;
            var filtered = Commands.Where(c => c is Line || c is BezierCurve).ToList();
            if (filtered.Count < 2) return false;
            if (!GetStartPoint(filtered.First()).Equals(GetEndPoint(filtered.Last()))) return false;
            return true;
        }

        /// <summary>
        /// Gets a <see cref="PdfRectangle"/> which entirely contains the geometry of the defined path.
        /// </summary>
        /// <returns>For paths which don't define any geometry this returns <see langword="null"/>.</returns>
        public PdfRectangle? GetBoundingRectangle()
        {
            if (commands.Count == 0)
            {
                return null;
            }

            var minX = double.MaxValue;
            var maxX = double.MinValue;

            var minY = double.MaxValue;
            var maxY = double.MinValue;

            foreach (var command in commands)
            {
                var rect = command.GetBoundingRectangle();
                if (rect == null)
                {
                    continue;
                }

                if (rect.Value.Left < minX)
                {
                    minX = rect.Value.Left;
                }

                if (rect.Value.Right > maxX)
                {
                    maxX = rect.Value.Right;
                }

                if (rect.Value.Bottom < minY)
                {
                    minY = rect.Value.Bottom;
                }

                if (rect.Value.Top > maxY)
                {
                    maxY = rect.Value.Top;
                }
            }

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (minX == double.MaxValue ||
                maxX == double.MinValue ||
                minY == double.MaxValue ||
                maxY == double.MinValue)
            {
                return null;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// A command in a <see cref="PdfPath"/>.
        /// </summary>
        public interface IPathCommand
        {
            /// <summary>
            /// Returns the smallest rectangle which contains the path region given by this command.
            /// </summary>
            /// <returns></returns>
            PdfRectangle? GetBoundingRectangle();

            /// <summary>
            /// Converts from the path command to an SVG string representing the path operation.
            /// </summary>
            void WriteSvg(StringBuilder builder, double height);
        }

        /// <summary>
        /// Close the current <see cref="PdfPath"/>.
        /// </summary>
        public class Close : IPathCommand
        {
            /// <inheritdoc />
            public PdfRectangle? GetBoundingRectangle()
            {
                return null;
            }

            /// <inheritdoc />
            public void WriteSvg(StringBuilder builder, double height)
            {
                builder.Append("Z ");
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                return (obj is Close);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Move drawing of the current <see cref="PdfPath"/> to the specified location.
        /// </summary>
        public class Move : IPathCommand
        {
            /// <summary>
            /// The location to move to.
            /// </summary>
            public PdfPoint Location { get; }

            /// <summary>
            /// Create a new <see cref="Move"/> path command.
            /// </summary>
            /// <param name="location"></param>
            public Move(PdfPoint location)
            {
                Location = location;
            }

            /// <summary>
            /// Returns <see langword="null"/> since this generates no visible path.
            /// </summary>
            public PdfRectangle? GetBoundingRectangle()
            {
                return null;
            }

            /// <inheritdoc />
            public void WriteSvg(StringBuilder builder, double height)
            {
                //builder.Append("M ").Append(Location.X).Append(' ').Append(height - Location.Y).Append(' ');
                builder.Append($"M {Location.X} {height - Location.Y} ");
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj is Move move)
                {
                    return Location.Equals(move.Location);
                }
                return false;
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (Location).GetHashCode();
            }
        }

        /// <summary>
        /// Draw a straight line between two points.
        /// </summary>
        public class Line : IPathCommand
        {
            /// <summary>
            /// The start of the line.
            /// </summary>
            public PdfPoint From { get; }

            /// <summary>
            /// The end of the line.
            /// </summary>
            public PdfPoint To { get; }

            /// <summary>
            /// Length of the line.
            /// </summary>
            public double Length
            {
                get
                {
                    var dx = From.X - To.X;
                    var dy = From.Y - To.Y;
                    return Math.Sqrt(dx * dx + dy * dy);
                }
            }

            /// <summary>
            /// Create a new <see cref="Line"/>.
            /// </summary>
            public Line(PdfPoint from, PdfPoint to)
            {
                From = from;
                To = to;
            }

            /// <inheritdoc />
            public PdfRectangle? GetBoundingRectangle()
            {
                return new PdfRectangle(From, To);
            }

            /// <inheritdoc />
            public void WriteSvg(StringBuilder builder, double height)
            {
                //builder.AppendFormat($"L {0} {1} ", To.X, height - To.Y);
                builder.Append($"L {To.X} {height - To.Y} ");
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj is Line line)
                {
                    return From.Equals(line.From) && To.Equals(line.To);
                }
                return false;
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (From, To).GetHashCode();
            }
        }

        /// <summary>
        /// Draw a Bezier curve given by the start, control and end points.
        /// </summary>
        public class BezierCurve : IPathCommand
        {
            /// <summary>
            /// The start point of the Bezier curve.
            /// </summary>
            public PdfPoint StartPoint { get; }

            /// <summary>
            /// The first control point of the curve.
            /// </summary>
            public PdfPoint FirstControlPoint { get; }

            /// <summary>
            /// The second control point of the curve.
            /// </summary>
            public PdfPoint SecondControlPoint { get; }

            /// <summary>
            /// The end point of the curve.
            /// </summary>
            public PdfPoint EndPoint { get; }

            /// <summary>
            /// Create a Bezier curve at the provided points.
            /// </summary>
            public BezierCurve(PdfPoint startPoint, PdfPoint firstControlPoint, PdfPoint secondControlPoint, PdfPoint endPoint)
            {
                StartPoint = startPoint;
                FirstControlPoint = firstControlPoint;
                SecondControlPoint = secondControlPoint;
                EndPoint = endPoint;
            }

            /// <inheritdoc />
            public PdfRectangle? GetBoundingRectangle()
            {
                // Optimised
                double minX;
                double maxX;
                if (StartPoint.X <= EndPoint.X)
                {
                    minX = StartPoint.X;
                    maxX = EndPoint.X;
                }
                else
                {
                    minX = EndPoint.X;
                    maxX = StartPoint.X;
                }

                double minY;
                double maxY;
                if (StartPoint.Y <= EndPoint.Y)
                {
                    minY = StartPoint.Y;
                    maxY = EndPoint.Y;
                }
                else
                {
                    minY = EndPoint.Y;
                    maxY = StartPoint.Y;
                }

                if (TrySolveQuadratic(true, minX, maxX, out var xSolutions))
                {
                    minX = xSolutions.min;
                    maxX = xSolutions.max;
                }

                if (TrySolveQuadratic(false, minY, maxY, out var ySolutions))
                {
                    minY = ySolutions.min;
                    maxY = ySolutions.max;
                }

                return new PdfRectangle(minX, minY, maxX, maxY);
            }

            /// <inheritdoc />
            public void WriteSvg(StringBuilder builder, double height)
            {
                builder.Append($"C {FirstControlPoint.X} { height - FirstControlPoint.Y}, { SecondControlPoint.X} {height - SecondControlPoint.Y}, {EndPoint.X} {height - EndPoint.Y} ");
            }

            private bool TrySolveQuadratic(bool isX, double currentMin, double currentMax, out (double min, double max) solutions)
            {
                solutions = default((double, double));

                // This method has been optimised for performance by eliminating calls to Math.

                // Given k points the general form is:
                // P = (1-t)^(k - i - 1)*t^(i)*P_i
                // 
                // For 4 points this gives:
                // P = (1−t)^3*P_1 + 3(1−t)^2*t*P_2 + 3(1−t)*t^2*P_3 + t^3*P_4
                // The differential is:
                // P' = 3(1-t)^2(P_2 - P_1) + 6(1-t)^t(P_3 - P_2) + 3t^2(P_4 - P_3)

                // P' = 3da(1-t)^2 + 6db(1-t)t + 3dct^2
                // P' = 3da - 3dat - 3dat + 3dat^2 + 6dbt - 6dbt^2 + 3dct^2
                // P' = (3da - 6db + 3dc)t^2 + (6db - 3da - 3da)t + 3da
                var p1 = isX ? StartPoint.X : StartPoint.Y;
                var p2 = isX ? FirstControlPoint.X : FirstControlPoint.Y;
                var p3 = isX ? SecondControlPoint.X : SecondControlPoint.Y;
                var p4 = isX ? EndPoint.X : EndPoint.Y;

                var threeda = 3 * (p2 - p1);
                var sixdb = 6 * (p3 - p2);
                var threedc = 3 * (p4 - p3);

                var a = threeda - sixdb + threedc;
                var b = sixdb - threeda - threeda;
                var c = threeda;

                // P' = at^2 + bt + c
                // t = (-b (+/-) sqrt(b ^ 2 - 4ac))/2a

                var sqrtable = b * b - 4 * a * c;

                if (sqrtable < 0)
                {
                    return false;
                }

                var sqrt = Math.Sqrt(sqrtable);
                var divisor = 2 * a;

                var t1 = (-b + sqrt) / divisor;
                var t2 = (-b - sqrt) / divisor;

                if (t1 >= 0 && t1 <= 1)
                {
                    var sol1 = ValueWithT(p1, p2, p3, p4, t1);
                    if (sol1 < currentMin)
                    {
                        currentMin = sol1;
                    }

                    if (sol1 > currentMax)
                    {
                        currentMax = sol1;
                    }
                }

                if (t2 >= 0 && t2 <= 1)
                {
                    var sol2 = ValueWithT(p1, p2, p3, p4, t2);
                    if (sol2 < currentMin)
                    {
                        currentMin = sol2;
                    }

                    if (sol2 > currentMax)
                    {
                        currentMax = sol2;
                    }
                }

                solutions = (currentMin, currentMax);

                return true;
            }

            /// <summary>
            /// Calculate the value of the Bezier curve at t.
            /// </summary>
            public static double ValueWithT(double p1, double p2, double p3, double p4, double t)
            {
                // P = (1−t)^3*P_1 + 3(1−t)^2*t*P_2 + 3(1−t)*t^2*P_3 + t^3*P_4
                var oneMinusT = 1 - t;
                var p = ((oneMinusT * oneMinusT * oneMinusT) * p1)
                        + (3 * (oneMinusT * oneMinusT) * t * p2)
                        + (3 * oneMinusT * (t * t) * p3)
                        + ((t * t * t) * p4);

                return p;
            }

            /// <summary>
            /// Converts the bezier curve into approximated lines.
            /// </summary>
            /// <param name="n">Number of lines required (minimum is 1).</param>
            /// <returns></returns>
            public IReadOnlyList<Line> ToLines(int n)
            {
                if (n < 1)
                {
                    throw new ArgumentException("BezierCurve.ToLines(): n must be greater than 0.");
                }

                List<Line> lines = new List<Line>();
                var previousPoint = StartPoint;

                for (int p = 1; p <= n; p++)
                {
                    double t = p / (double)n;
                    var currentPoint = new PdfPoint(ValueWithT(StartPoint.X, FirstControlPoint.X, SecondControlPoint.X, EndPoint.X, t),
                                                    ValueWithT(StartPoint.Y, FirstControlPoint.Y, SecondControlPoint.Y, EndPoint.Y, t));
                    lines.Add(new Line(previousPoint, currentPoint));
                    previousPoint = currentPoint;
                }
                return lines;
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj is BezierCurve curve)
                {
                    return StartPoint.Equals(curve.StartPoint) &&
                           FirstControlPoint.Equals(curve.FirstControlPoint) &&
                           SecondControlPoint.Equals(curve.SecondControlPoint) &&
                           EndPoint.Equals(curve.EndPoint);
                }
                return false;
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (StartPoint, FirstControlPoint, SecondControlPoint, EndPoint).GetHashCode();
            }
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
        /// Compares two <see cref="PdfPath"/>s for equality. Paths will only be considered equal if the commands which construct the paths are in the same order.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is PdfPath path)
            {
                if (Commands.Count != path.Commands.Count) return false;

                for (int i = 0; i < Commands.Count; i++)
                {
                    if (!Commands[i].Equals(path.Commands[i])) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the hash code. Paths will only have the same hash code if the commands which construct the paths are in the same order.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = Commands.Count + 1;
            for (int i = 0; i < Commands.Count; i++)
            {
                hash = hash * (i + 1) * 17 + Commands[i].GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var command in Commands)
            {
                command.WriteSvg(builder, 0);
            }
            return builder.ToString();
        }

    }
}
