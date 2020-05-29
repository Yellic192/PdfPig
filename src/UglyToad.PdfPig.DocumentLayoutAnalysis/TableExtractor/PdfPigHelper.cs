using System;

namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using Core;

    static class PdfPigHelper
    {
        /// <summary>
        /// Returns true if this point is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this point is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(this PdfPoint point)
        {
            if (point.X < 0 || point.Y < 0)
                return false;

            if (point.X > 10000 || point.Y > 10000)
                return false;

            return true;
        }

        public static bool Equals(this Point point, Point other, float tolerance)
        {
            return Math.Abs(point.X - other.X) < tolerance && Math.Abs(point.Y - other.Y) < tolerance;
        }

        public static int CompareTo(this Point left, Point right, float tolerance)
        {
            if (Math.Abs(left.X - right.X) < tolerance)
            {
                if (Math.Abs(left.Y - right.Y) < tolerance)
                    // Equal point
                    return 0;
                else
                    return left.Y.CompareTo(right.Y);
            }
            else
            {
                return left.X.CompareTo(right.X);
            }
        }

    }
}
