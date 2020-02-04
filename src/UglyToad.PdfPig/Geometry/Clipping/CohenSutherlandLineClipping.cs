using UglyToad.PdfPig.Core;

namespace UglyToad.PdfPig.Geometry.Clipping
{
    /// <summary>
    /// The Cohen–Sutherland algorithm is a computer-graphics algorithm used for line clipping.
    /// </summary>
    internal static class CohenSutherlandLineClipping
    {
        private const byte Inside = 0; // 0000
        private const int Left = 1;    // 0001
        private const int Right = 2;   // 0010
        private const int Bottom = 4;  // 0100
        private const int Top = 8;     // 1000

        /// <summary>
        /// Clip the line using the Cohen–Sutherland algorithm.
        /// </summary>
        internal static PdfLine? Clip(PdfLine line, PdfRectangle clippingRectangle)
        {
            var coords = Clip(
                (double)line.Point1.X, (double)line.Point1.Y, (double)line.Point2.X, (double)line.Point2.Y,
                (double)clippingRectangle.Left, (double)clippingRectangle.Right, (double)clippingRectangle.Bottom, (double)clippingRectangle.Top);
            if (!coords.HasValue) return null;
            return new PdfLine(coords.Value.point1, coords.Value.point2);
        }

        /// <summary>
        /// Clip the line using the Cohen–Sutherland algorithm.
        /// </summary>
        internal static PdfPath.Line Clip(PdfPath.Line line, PdfRectangle clippingRectangle)
        {
            var coords = Clip(
                (double)line.From.X, (double)line.From.Y, (double)line.To.X, (double)line.To.Y,
                (double)clippingRectangle.Left, (double)clippingRectangle.Right, (double)clippingRectangle.Bottom, (double)clippingRectangle.Top);
            if (!coords.HasValue) return null;
            return new PdfPath.Line(coords.Value.point1, coords.Value.point2);
        }

        // Cohen–Sutherland clipping algorithm clips a line from
        // P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with 
        // diagonal from (xmin, ymin) to (xmax, ymax).
        private static (PdfPoint point1, PdfPoint point2)? Clip(double x0, double y0, double x1, double y1,
            double xmin, double xmax, double ymin, double ymax)
        {
            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            int outcode0 = ComputeOutCode(x0, y0, xmin, xmax, ymin, ymax);
            int outcode1 = ComputeOutCode(x1, y1, xmin, xmax, ymin, ymax);
            bool accept = false;

            double x = 0;
            double y = 0;

            while (true)
            {
                if ((outcode0 | outcode1) == 0)
                {
                    // bitwise OR is 0: both points inside window; trivially accept and exit loop
                    accept = true;
                    break;
                }
                else if ((outcode0 & outcode1) != 0)
                {
                    // bitwise AND is not 0: both points share an outside zone (LEFT, RIGHT, TOP,
                    // or BOTTOM), so both must be outside window; exit loop (accept is false)
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge

                    // At least one endpoint is outside the clip rectangle; pick it.
                    int outcodeOut = outcode0 != Inside ? outcode0 : outcode1;

                    // Now find the intersection point;
                    // use formulas:
                    //   slope = (y1 - y0) / (x1 - x0)
                    //   x = x0 + (1 / slope) * (ym - y0), where ym is ymin or ymax
                    //   y = y0 + slope * (xm - x0), where xm is xmin or xmax
                    // No need to worry about divide-by-zero because, in each case, the
                    // outcode bit being tested guarantees the denominator is non-zero
                    if ((outcodeOut & Top) != 0)
                    {
                        // point is above the clip window
                        x = x0 + (x1 - x0) * (ymax - y0) / (y1 - y0);
                        y = ymax;
                    }
                    else if ((outcodeOut & Bottom) != 0)
                    {
                        // point is below the clip window
                        x = x0 + (x1 - x0) * (ymin - y0) / (y1 - y0);
                        y = ymin;
                    }
                    else if ((outcodeOut & Right) != 0)
                    {
                        // point is to the right of clip window
                        y = y0 + (y1 - y0) * (xmax - x0) / (x1 - x0);
                        x = xmax;
                    }
                    else if ((outcodeOut & Left) != 0)
                    {
                        // point is to the left of clip window
                        y = y0 + (y1 - y0) * (xmin - x0) / (x1 - x0);
                        x = xmin;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == outcode0)
                    {
                        x0 = x;
                        y0 = y;
                        outcode0 = ComputeOutCode(x0, y0, xmin, xmax, ymin, ymax);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        outcode1 = ComputeOutCode(x1, y1, xmin, xmax, ymin, ymax);
                    }
                }
            }

            if (accept)
            {
                return (new PdfPoint((decimal)x0, (decimal)y0), new PdfPoint((decimal)x1, (decimal)y1));
            }
            return null;
        }

        private static int ComputeOutCode(double x, double y, double xmin, double xmax, double ymin, double ymax)
        {
            int code;

            code = Inside;                      // initialised as being inside of [[clip window]]

            if (x < xmin) code |= Left;         // to the left of clip window
            else if (x > xmax) code |= Right;   // to the right of clip window

            if (y < ymin) code |= Bottom;       // below the clip window
            else if (y > ymax) code |= Top;     // above the clip window

            return code;
        }
    }
}
