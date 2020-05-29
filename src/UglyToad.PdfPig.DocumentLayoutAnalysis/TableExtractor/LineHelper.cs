using System;

namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using Core;

    static class LineHelper
    {
        /// <summary>
        /// Determines whether this line is horizontal.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the line is horizontal; otherwise false
        /// </returns>
        public static bool IsHorizontal(this PdfSubpath.Line line, float tolerance)
        {
            return Math.Abs(line.From.Y - line.To.Y) < tolerance;
        }


        /// <summary>
        /// Determines whether this line is vertical.
        /// </summary>
        /// <returns>True if the line is vertical; otherwise false</returns>
        public static bool IsVertical(this PdfSubpath.Line line, float tolerance)
        {
            return Math.Abs(line.From.X - line.To.X) < tolerance;
        }


        /// <summary>
        /// Determines whether the specified line is coincident with this line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="other">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the lines are coincident; otherwise false
        /// </returns>
        public static bool IsCoincident(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            return line.Equals(other, tolerance);
        }

        /// <summary>
        /// Determines whether the specified line is consecutive to this line.
        /// One line is considered consecutive of another line if the start point of one line is the same
        /// of the end point of the other line or vice versa. Also overlapped lines can be consecutive
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="other">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the line is consecutive to this line
        /// </returns>
        public static bool IsConsecutive(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            if (line.From.Equals(other.From, tolerance))
                return true;
            if (line.From.Equals(other.To, tolerance))
                return true;
            if (line.To.Equals(other.From, tolerance))
                return true;
            if (line.To.Equals(other.To, tolerance))
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the specified line is partially or totally overlapped with this line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="other">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if this line is partially or totally overlapped with the specified line
        /// </returns>
        /// <exception cref="InvalidOperationException">IsOverlapped works only on horizontal and vertical lines</exception>
        public static bool IsOverlapped(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            if (
                !line.IsHorizontal(tolerance) && !line.IsVertical(tolerance) ||
                !other.IsHorizontal(tolerance) && !other.IsVertical(tolerance) ||
                line.IsHorizontal(tolerance) != other.IsHorizontal(tolerance))
                return false;

            if (!line.IsAlignedHorizontally(other, tolerance) && !line.IsAlignedVertically(other, tolerance))
                return false;

            if (line.IsConsecutive(other, tolerance))
                return true;
            else if (line.IsCoincident(other, tolerance))
                return true;
            else if (line.From.CompareTo(other.From, tolerance) <= 0 && other.From.CompareTo(line.To, tolerance) <= 0)
                return true;
            else if (line.From.CompareTo(other.To, tolerance) <= 0 && other.To.CompareTo(line.To, tolerance) <= 0)
                return true;
            else if (other.From.CompareTo(line.From, tolerance) <= 0 && line.To.CompareTo(other.To, tolerance) <= 0)
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
        /// <param name="other">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// The joined line
        /// </returns>
        /// <exception cref="InvalidOperationException">The lines are not overlapped
        /// or
        /// The lines are not aligned</exception>
        public static PdfSubpath.Line Join(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            if (!line.IsOverlapped(other, tolerance))
                throw new InvalidOperationException("The lines are not overlapped");

            if (!line.IsAlignedHorizontally(other, tolerance) && !line.IsAlignedVertically(other, tolerance))
                throw new InvalidOperationException("The lines are not aligned");

            if (line.IsCoincident(other, tolerance))
                return line;

            if (line.From.CompareTo(other.From, tolerance) <= 0 && other.To.CompareTo(line.To, tolerance) <= 0)
                return line;
            else if (line.From.CompareTo(other.From, tolerance) <= 0 && other.From.CompareTo(line.To, tolerance) <= 0 && line.To.CompareTo(other.To, tolerance) <= 0 && !line.From.Equals(other.To, tolerance))
                return new PdfSubpath.Line(line.From, other.To);
            else if (other.From.CompareTo(line.From, tolerance) <= 0 && line.From.CompareTo(other.To, tolerance) <= 0 && other.To.CompareTo(line.To, tolerance) <= 0 && !other.From.Equals(line.To, tolerance))
                return new PdfSubpath.Line(other.From, line.To);
            else if (other.From.CompareTo(line.From, tolerance) <= 0 && line.To.CompareTo(other.To, tolerance) <= 0)
                return other;
            return line;
        }

        /// <summary>
        /// Determines whether this line and the specified line are aligned vertically.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="other">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the lines are aligned; otherwise false
        /// </returns>
        public static bool IsAlignedVertically(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            if (!line.IsVertical(tolerance) || !line.IsVertical(tolerance))
                return false;
            else if (Math.Abs(line.From.X - other.From.X) < tolerance)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines whether this line and the specified line are aligned horizontally.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="other">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// True if the lines are aligned; otherwise false
        /// </returns>
        public static bool IsAlignedHorizontally(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            if (!line.IsHorizontal(tolerance) || !line.IsHorizontal(tolerance))
                return false;
            else if (Math.Abs(line.From.Y - other.From.Y) < tolerance)
                return true;
            else
                return false;
        }

        #region ==

        /// <summary>
        /// Determines whether the specified line, is equal to this line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="other">The Line to compare with this instance.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the specified Line is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public static bool Equals(this PdfSubpath.Line line, PdfSubpath.Line other, float tolerance)
        {
            return
                line.From.Equals(other.From, tolerance) && line.To.Equals(other.To, tolerance) ||
                line.From.Equals(other.To, tolerance) && line.To.Equals(other.From, tolerance);

        }
        #endregion


    }
}
