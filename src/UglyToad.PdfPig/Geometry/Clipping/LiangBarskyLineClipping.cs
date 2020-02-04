using System;
using UglyToad.PdfPig.Core;

namespace UglyToad.PdfPig.Geometry.Clipping
{
    /// <summary>
    /// The Liang-Barsky algorithm is a computer-graphics algorithm used for line clipping.
    /// </summary>
    internal static class LiangBarskyLineClipping
    {
        /// <summary>
        /// Clip the line using the Liang-Barsky algorithm.
        /// </summary>
        internal static PdfLine? Clip(PdfLine line, PdfRectangle clippingRectangle)
        {
            var coords = Clip(
                (float)line.Point1.X, (float)line.Point1.Y, (float)line.Point2.X, (float)line.Point2.Y,
                (float)clippingRectangle.Left, (float)clippingRectangle.Right, (float)clippingRectangle.Bottom, (float)clippingRectangle.Top);
            if (!coords.HasValue) return null;
            return new PdfLine(coords.Value.point1, coords.Value.point2);
        }

        /// <summary>
        /// Clip the line using the Liang-Barsky algorithm.
        /// </summary>
        internal static PdfPath.Line Clip(PdfPath.Line line, PdfRectangle clippingRectangle)
        {
            var coords = Clip(
                (float)line.From.X, (float)line.From.Y, (float)line.To.X, (float)line.To.Y,
                (float)clippingRectangle.Left, (float)clippingRectangle.Right, (float)clippingRectangle.Bottom, (float)clippingRectangle.Top);
            if (!coords.HasValue) return null;
            return new PdfPath.Line(coords.Value.point1, coords.Value.point2);
        }

        private static (PdfPoint point1, PdfPoint point2)? Clip(float x1, float y1, float x2, float y2,
            float xmin, float xmax, float ymin, float ymax)
        {
            float p1 = -(x2 - x1);
            float p2 = -p1;
            float p3 = -(y2 - y1);
            float p4 = -p3;

            float q1 = x1 - xmin;
            float q2 = xmax - x1;
            float q3 = y1 - ymin;
            float q4 = ymax - y1;

            if ((Math.Abs(p1) <= double.Epsilon && q1 < 0) ||
                (Math.Abs(p3) <= double.Epsilon && q3 < 0) ||
                (Math.Abs(p2) <= double.Epsilon && q2 < 0) ||
                (Math.Abs(p4) <= double.Epsilon && q4 < 0))
            {
                return null; // Line is parallel and outside of clipping window
            }

            float[] posarr = new float[5];
            float[] negarr = new float[5];
            int posind = 1;
            int negind = 1;
            posarr[0] = 1;
            negarr[0] = 0;

            if (p1 != 0)
            {
                float r1 = q1 / p1;
                float r2 = q2 / p2;
                if (p1 < 0)
                {
                    negarr[negind++] = r1;
                    posarr[posind++] = r2;
                }
                else
                {
                    negarr[negind++] = r2;
                    posarr[posind++] = r1;
                }
            }

            if (p3 != 0)
            {
                float r3 = q3 / p3;
                float r4 = q4 / p4;
                if (p3 < 0)
                {
                    negarr[negind++] = r3;
                    posarr[posind++] = r4;
                }
                else
                {
                    negarr[negind++] = r4;
                    posarr[posind++] = r3;
                }
            }

            float xn1, yn1, xn2, yn2;
            float rn1, rn2;
            rn1 = Maxi(negarr, negind);
            rn2 = Mini(posarr, posind);

            if (rn1 > rn2)
            {
                return null;
            }

            xn1 = x1 + p2 * rn1;
            yn1 = y1 + p4 * rn1;

            xn2 = x1 + p2 * rn2;
            yn2 = y1 + p4 * rn2;

            return (new PdfPoint(Math.Round(xn1, 5), Math.Round(yn1, 5)),
                    new PdfPoint(Math.Round(xn2, 5), Math.Round(yn2, 5)));
        }

        private static float Maxi(float[] arr, int n)
        {
            float m = 0;
            for (int i = 0; i < n; ++i)
                if (m < arr[i])
                    m = arr[i];
            return m;
        }

        private static float Mini(float[] arr, int n)
        {
            float m = 1;
            for (int i = 0; i < n; ++i)
                if (m > arr[i])
                    m = arr[i];
            return m;
        }
    }
}
