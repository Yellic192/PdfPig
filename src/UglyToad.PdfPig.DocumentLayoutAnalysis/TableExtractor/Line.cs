namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;

    /// <summary>
    /// Class that handles lines. Comparers are based on tolerance
    /// </summary>
    public struct Line
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> struct.
        /// </summary>
        /// <param name="startX">The start x.</param>
        /// <param name="endX">The end x.</param>
        /// <param name="startY">The start y.</param>
        /// <param name="endY">The end y.</param>
        public Line(float startX, float endX, float startY, float endY)
        {
            if (startX > endX)
            {
                EndPoint = new Point(startX, startY);
                StartPoint = new Point(endX, endY);
            }
            else if (startY > endY)
            {
                EndPoint = new Point(startX, startY);
                StartPoint = new Point(endX, endY);
            }
            else
            {
                StartPoint = new Point(startX, startY);
                EndPoint = new Point(endX, endY);
            }

            if (StartPoint.Equals(EndPoint, ContentExtractor.Tolerance))
                throw new InvalidOperationException("The line is a single point");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> struct.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        public Line(Point startPoint, Point endPoint)
        {
            if (startPoint.Equals(endPoint, ContentExtractor.Tolerance))
                throw new InvalidOperationException("The line is a single point");

            if (startPoint.CompareTo(endPoint, ContentExtractor.Tolerance) == 1)
            {
                EndPoint = startPoint;
                StartPoint = endPoint;
            }
            else
            {
                StartPoint = startPoint;
                EndPoint = endPoint;
            }
        }

        /// <summary>
        /// The start point
        /// </summary>
        public readonly Point StartPoint;

        /// <summary>
        /// The end point
        /// </summary>
        public readonly Point EndPoint;


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", StartPoint, EndPoint);
        }

        /// <summary>
        /// Determines whether this line is horizontal.
        /// </summary>
        /// <returns>True if the line is horizontal; otherwise false</returns>
        public bool IsHorizontal()
        {
            return Math.Abs(StartPoint.Y - EndPoint.Y) < ContentExtractor.Tolerance;
        }


        /// <summary>
        /// Determines whether this line is vertical.
        /// </summary>
        /// <returns>True if the line is vertical; otherwise false</returns>
        public bool IsVertical()
        {
            return Math.Abs(StartPoint.X - EndPoint.X) < ContentExtractor.Tolerance;
        }


        /// <summary>
        /// Determines whether the specified line is coincident with this line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the lines are coincident; otherwise false
        /// </returns>
        public bool IsCoincident(Line line, float tolerance)
        {
            return this.Equals(line, tolerance);
        }

        /// <summary>
        /// Determines whether the specified line is consecutive to this line.
        /// One line is considered consecutive of another line if the start point of one line is the same
        /// of the end point of the other line or vice versa. Also overlapped lines can be consecutive
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the line is consecutive to this line
        /// </returns>
        public bool IsConsecutive(Line line, float tolerance)
        {
            if (this.StartPoint.Equals(line.StartPoint, tolerance))
                return true;
            if (this.StartPoint.Equals(line.EndPoint, tolerance))
                return true;
            if (this.EndPoint.Equals(line.StartPoint, tolerance))
                return true;
            if (this.EndPoint.Equals(line.EndPoint, tolerance))
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the specified line is partially or totally overlapped with this line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if this line is partially or totally overlapped with the specified line
        /// </returns>
        /// <exception cref="InvalidOperationException">IsOverlapped works only on horizontal and vertical lines</exception>
        public bool IsOverlapped(Line line, float tolerance)
        {
            if (
                !this.IsHorizontal() && !this.IsVertical() ||
                !line.IsHorizontal() && !line.IsVertical() ||
                this.IsHorizontal() != line.IsHorizontal())
                return false;

            if (!this.IsAlignedHorizontally(line, tolerance) && !this.IsAlignedVertically(line, tolerance))
                return false;

            if (IsConsecutive(line, tolerance))
                return true;
            else if (IsCoincident(line, tolerance))
                return true;
            else if (this.StartPoint.CompareTo(line.StartPoint, tolerance) <= 0 && line.StartPoint.CompareTo(this.EndPoint, tolerance) <= 0)
                return true;
            else if (this.StartPoint.CompareTo(line.EndPoint, tolerance) <= 0 && line.EndPoint.CompareTo(this.EndPoint, tolerance) <= 0)
                return true;
            else if (line.StartPoint.CompareTo(this.StartPoint, tolerance) <= 0 && this.EndPoint.CompareTo(line.EndPoint, tolerance) <= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Joins the line with the specified line and returns the joined line that could be
        /// the current line if the specified line is totally overlapped with this line,
        /// the specified line if the current line is totally overlapped with the specified line,
        /// a new line if the lines are partially overlapped
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// The joined line
        /// </returns>
        /// <exception cref="InvalidOperationException">The lines are not overlapped
        /// or
        /// The lines are not aligned</exception>
        public Line Join(Line line, float tolerance)
        {
            if (!this.IsOverlapped(line, tolerance))
                throw new InvalidOperationException("The lines are not overlapped");

            if (!this.IsAlignedHorizontally(line, tolerance) && !this.IsAlignedVertically(line, tolerance))
                throw new InvalidOperationException("The lines are not aligned");

            if (this.IsCoincident(line, tolerance))
                return this;

            if (this.StartPoint.CompareTo(line.StartPoint, tolerance) <= 0 && line.EndPoint.CompareTo(this.EndPoint, tolerance) <= 0)
                return this;
            else if (this.StartPoint.CompareTo(line.StartPoint, tolerance) <= 0 && line.StartPoint.CompareTo(this.EndPoint, tolerance) <= 0 && this.EndPoint.CompareTo(line.EndPoint, tolerance) <= 0 && !this.StartPoint.Equals(line.EndPoint, ContentExtractor.Tolerance))
                return new Line(this.StartPoint, line.EndPoint);
            else if (line.StartPoint.CompareTo(this.StartPoint, tolerance) <= 0 && this.StartPoint.CompareTo(line.EndPoint, tolerance) <= 0 && line.EndPoint.CompareTo(this.EndPoint, tolerance) <= 0 && !line.StartPoint.Equals(this.EndPoint, ContentExtractor.Tolerance))
                return new Line(line.StartPoint, this.EndPoint);
            else if (line.StartPoint.CompareTo(this.StartPoint, tolerance) <= 0 && this.EndPoint.CompareTo(line.EndPoint, tolerance) <= 0)
                return line;
            return this;
        }

        /// <summary>
        /// Determines whether this line and the specified line are aligned vertically.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the lines are aligned; otherwise false
        /// </returns>
        public bool IsAlignedVertically(Line line, float tolerance)
        {
            if (!this.IsVertical() || !line.IsVertical())
                return false;
            else if (Math.Abs(this.StartPoint.X - line.StartPoint.X) < tolerance)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines whether this line and the specified line are aligned horizontally.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the lines are aligned; otherwise false
        /// </returns>
        public bool IsAlignedHorizontally(Line line, float tolerance)
        {
            if (!this.IsHorizontal() || !line.IsHorizontal())
                return false;
            else if (Math.Abs(this.StartPoint.Y - line.StartPoint.Y) < tolerance)
                return true;
            else
                return false;
        }

        #region ==

        /// <summary>
        /// Determines whether the specified line, is equal to this line.
        /// </summary>
        /// <param name="other">The Line to compare with this instance.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the specified Line is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Line other, float tolerance)
        {
            return 
                StartPoint.Equals(other.StartPoint, tolerance) && EndPoint.Equals(other.EndPoint, tolerance) ||
                StartPoint.Equals(other.EndPoint, tolerance) && EndPoint.Equals(other.StartPoint, tolerance);

        }
        #endregion


    }
}
