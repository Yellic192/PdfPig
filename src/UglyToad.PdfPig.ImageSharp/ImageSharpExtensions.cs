namespace UglyToad.PdfPig.ImageSharp
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing;
    using System;
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Graphics.Core;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    internal static class ImageSharpExtensions
    {
        public static IPath ToGraphicsPath(this IReadOnlyList<PdfSubpath> pdfSubpaths, int height, double scale)
        {
            var gp = new PathBuilder();

            foreach (var subpath in pdfSubpaths)
            {
                foreach (var c in subpath.Commands)
                {
                    if (c is Move move)
                    {
                        gp.MoveTo(move.Location.ToPointF(height, scale));
                    }
                    else if (c is Line line)
                    {
                        gp.LineTo(line.To.ToPointF(height, scale));
                    }
                    else if (c is BezierCurve curve)
                    {
                        gp.AddCubicBezier(curve.StartPoint.ToPointF(height, scale),
                                          curve.FirstControlPoint.ToPointF(height, scale),
                                          curve.SecondControlPoint.ToPointF(height, scale),
                                          curve.EndPoint.ToPointF(height, scale));
                    }
                    else if (c is Close)
                    {
                        gp.CloseFigure(); //.Close();
                    }
                }
            }
            return gp.Build();
        }

        /// <summary>
        /// Default to Black.
        /// </summary>
        public static Color ToColor(this IColor pdfColor)
        {
            if (pdfColor != null)
            {
                var colorRgb = pdfColor.ToRGBValues();
                if (pdfColor is AlphaColor alphaColor)
                {
                    return Color.FromRgba((byte)(colorRgb.r * 255), (byte)(colorRgb.g * 255), (byte)(colorRgb.b * 255), (byte)(alphaColor.A * 255));
                }
                return Color.FromRgb((byte)(colorRgb.r * 255), (byte)(colorRgb.g * 255), (byte)(colorRgb.b * 255));
            }
            return Color.Black;
        }

        public static PointF ToPointF(this PdfPoint point, double height, double mult)
        {
            float xs = (float)(point.X * mult);
            float ys = (float)(height - point.Y * mult);
            return new PointF(xs, ys);
        }

        public static JointStyle ToJointStyle(this LineJoinStyle lineJoinStyle)
        {
            switch (lineJoinStyle)
            {
                case LineJoinStyle.Bevel:
                    return JointStyle.Square;

                case LineJoinStyle.Miter:
                    return JointStyle.Miter;

                case LineJoinStyle.Round:
                    return JointStyle.Round;

                default:
                    throw new NotImplementedException($"Unknown LineJoinStyle '{lineJoinStyle}'.");
            }
        }

        public static float[]? ToStrokePattern(this LineDashPattern lineDashPattern, double mult)
        {
            if (lineDashPattern.Phase != 0 || lineDashPattern.Array?.Count > 0) // to put in helper
            {
                //* https://docs.microsoft.com/en-us/dotnet/api/system.drawing.pen.dashpattern?view=dotnet-plat-ext-3.1
                //* The elements in the dashArray array set the length of each dash and space in the dash pattern. 
                //* The first element sets the length of a dash, the second element sets the length of a space, the
                //* third element sets the length of a dash, and so on. Consequently, each element should be a 
                //* non-zero positive number.

                if (lineDashPattern.Array.Count == 1)
                {
                    List<float> pattern = new List<float>();
                    var v = lineDashPattern.Array[0];
                    pattern.Add((float)((double)v / mult));
                    pattern.Add((float)((double)v / mult));
                    //return SKPathEffect.CreateDash(pattern.ToArray(), (float)v); // TODO
                    return pattern.ToArray();
                }
                else if (lineDashPattern.Array.Count > 0)
                {
                    List<float> pattern = new List<float>();
                    for (int i = 0; i < lineDashPattern.Array.Count; i++)
                    {
                        var v = lineDashPattern.Array[i];
                        if (v == 0)
                        {
                            pattern.Add((float)(1.0 / 72.0 * mult));
                        }
                        else
                        {
                            pattern.Add((float)((double)v / mult));
                        }
                    }
                    //pen.DashPattern = pattern.ToArray(); // TODO
                    //return SKPathEffect.CreateDash(pattern.ToArray(), pattern[0]); // TODO
                    return pattern.ToArray();
                }
                //pen.DashOffset = path.LineDashPattern.Value.Phase; // mult?? //  // TODO
            }
            return null;
        }

        public static EndCapStyle ToEndCapStyle(this LineCapStyle lineCapStyle)
        {
            switch (lineCapStyle)
            {
                case LineCapStyle.Butt:
                    return EndCapStyle.Butt;

                case LineCapStyle.Round:
                    return EndCapStyle.Round;

                case LineCapStyle.ProjectingSquare:
                    return EndCapStyle.Square;

                default:
                    throw new NotImplementedException($"Unknown LineCapStyle '{lineCapStyle}'.");
            }
        }

        public static IntersectionRule ToIntersectionRule(this FillingRule fillingRule)
        {
            return fillingRule == FillingRule.EvenOdd ? IntersectionRule.OddEven : IntersectionRule.Nonzero;
        }

        public static void Rectangle(this PathBuilder pathBuilder, float x, float y, float width, float height)
        {
            pathBuilder.StartFigure();                  // is equivalent to:
            pathBuilder.MoveTo(new PointF(x, y));       // x y m
            pathBuilder.LineTo(x + width, y);           // (x + width) y l
            pathBuilder.LineTo(x + width, y + height);  // (x + width) (y + height) l
            pathBuilder.LineTo(x, y + height);          // x (y + height) l
            pathBuilder.CloseFigure();                  // h
        }
    }
}
