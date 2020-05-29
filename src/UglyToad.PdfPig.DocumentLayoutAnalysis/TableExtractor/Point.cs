namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Globalization;

    /// <summary>
    /// A point
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// The origin
        /// </summary>
        public static readonly Point Origin = new Point(0, 0);

        /*
        #region ==

        /// <summary>
        /// Check if the specified point is equal to this
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>true if the other point is equal to this point; otherwise false</returns>
        public bool Equals(Point other)
        {
            return Math.Abs(X - other.X) < ContentExtractor.Tolerance && Math.Abs(Y - other.Y) < ContentExtractor.Tolerance;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Point && Equals((Point) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Point left, Point right)
        {
            return !left.Equals(right);
        }

        #endregion
        */

        /*
        #region >, >=, <, <=

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(Point left, Point right)
        {
            if (Math.Abs(left.X - right.X) < ContentExtractor.Tolerance)
            {
                if (Math.Abs(left.Y - right.Y) < ContentExtractor.Tolerance)
                    // Equal point
                    return false;
                else
                    return left.Y > right.Y;
            }
            else
            {
                return left.X > right.X;
            }
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >=(Point left, Point right)
        {
            if (Math.Abs(left.X - right.X) < ContentExtractor.Tolerance)
            {
                if (Math.Abs(left.Y - right.Y) < ContentExtractor.Tolerance)
                    // Equal point
                    return true;
                else
                    return left.Y > right.Y;
            }
            else
            {
                return left.X > right.X;
            }
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <=(Point left, Point right)
        {
            if (Math.Abs(left.X - right.X) < ContentExtractor.Tolerance)
            {
                if (Math.Abs(left.Y - right.Y) < ContentExtractor.Tolerance)
                    // Equal point
                    return true;
                else
                    return left.Y < right.Y;
            }
            else
            {
                return left.X < right.X;
            }
        }


        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(Point left, Point right)
        {
            if (Math.Abs(left.X - right.X) < ContentExtractor.Tolerance)
            {
                if (Math.Abs(left.Y - right.Y) < ContentExtractor.Tolerance)
                    // Equal point
                    return false;
                else
                    return left.Y < right.Y;
            }
            else
            {
                return left.X < right.X;
            }
        }

        #endregion
        */

        /// <summary>
        /// The x coordinate of the point
        /// </summary>
        public readonly double X;
        /// <summary>
        /// The y coordinate of the point
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        /// <summary>
        /// Calculate the distance between this point and the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public float Distance(Point point)
        {
            return (float) Math.Sqrt((X - point.X) * (X - point.X) + (Y - point.Y) * (Y - point.Y));
        }

        /// <summary>
        /// Returns true if this point is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this point is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            if (X < 0 || Y < 0)
                return false;

            if (X > 10000 || Y > 10000)
                return false;

            return true;
        }

        /// <summary>
        /// Rotates this point using the specified page rotation.
        /// </summary>
        /// <param name="pageRotation">The page rotation.</param>
        /// <returns>A new point rotated</returns>
        public Point Rotate(int pageRotation)
        {
            switch (pageRotation)
            {
                case 0:
                    return new Point(X, 800 - Y);
                case 90:
                    return new Point(Y, X);
                default:
                    return this;
            }
        }
    }
}
