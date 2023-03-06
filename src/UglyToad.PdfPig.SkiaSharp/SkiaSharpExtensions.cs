namespace UglyToad.PdfPig.SkiaSharp
{
    using global::SkiaSharp;
    using System;
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Graphics.Core;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    internal static class SkiaSharpExtensions
    {
        public static SKPath PdfPathToGraphicsPath(this PdfPath path, int height, double scale)
        {
            var gp = PdfSubpathsToGraphicsPath(path, height, scale);
            gp.FillType = path.FillingRule == FillingRule.NonZeroWinding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
            return gp;
        }

        public static SKPath PdfSubpathsToGraphicsPath(this IReadOnlyList<PdfSubpath> pdfSubpaths, int height, double scale)
        {
            var gp = new SKPath();

            foreach (var subpath in pdfSubpaths)
            {
                foreach (var c in subpath.Commands)
                {
                    if (c is Move move)
                    {
                        gp.MoveTo(move.Location.ToSKPoint(height, scale));
                    }
                    else if (c is Line line)
                    {
                        gp.LineTo(line.To.ToSKPoint(height, scale));
                    }
                    else if (c is BezierCurve curve)
                    {
                        gp.CubicTo(curve.FirstControlPoint.ToSKPoint(height, scale),
                            curve.SecondControlPoint.ToSKPoint(height, scale),
                            curve.EndPoint.ToSKPoint(height, scale));
                    }
                    else if (c is Close)
                    {
                        gp.Close();
                    }
                }
            }
            return gp;
        }

        public static SKPoint ToSKPoint(this PdfPoint pdfPoint, int height, double scale)
        {
            return new SKPoint((float)(pdfPoint.X * scale), (float)(height - pdfPoint.Y * scale));
        }

        public static SKStrokeJoin ToSKStrokeJoin(this LineJoinStyle lineJoinStyle)
        {
            switch (lineJoinStyle)
            {
                case LineJoinStyle.Bevel:
                    return SKStrokeJoin.Bevel;

                case LineJoinStyle.Miter:
                    return SKStrokeJoin.Miter;

                case LineJoinStyle.Round:
                    return SKStrokeJoin.Round;

                default:
                    throw new NotImplementedException($"Unknown LineJoinStyle '{lineJoinStyle}'.");
            }
        }

        public static SKStrokeCap ToSKStrokeCap(this LineCapStyle lineCapStyle)
        {
            switch (lineCapStyle) // to put in helper
            {
                case LineCapStyle.Butt:
                    return SKStrokeCap.Butt;

                case LineCapStyle.ProjectingSquare:
                    return SKStrokeCap.Square;

                case LineCapStyle.Round:
                    return SKStrokeCap.Round;

                default:
                    throw new NotImplementedException($"Unknown LineCapStyle '{lineCapStyle}'.");
            }
        }

        public static SKPathEffect ToSKPathEffect(this LineDashPattern lineDashPattern, double mult)
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
                    return SKPathEffect.CreateDash(pattern.ToArray(), (float)v); // TODO
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
                    return SKPathEffect.CreateDash(pattern.ToArray(), pattern[0]); // TODO
                }
                //pen.DashOffset = path.LineDashPattern.Value.Phase; // mult?? //  // TODO
            }
            return null;
        }

        public static SKPathFillType ToSKPathFillType(this FillingRule fillingRule)
        {
            return fillingRule == FillingRule.NonZeroWinding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
        }

        /// <summary>
        /// Default to Black.
        /// </summary>
        /// <param name="pdfColor"></param>
        public static SKColor ToSKColor(this IColor pdfColor)
        {
            if (pdfColor != null)
            {
                var colorRgb = pdfColor.ToRGBValues();
                decimal r = colorRgb.r;
                decimal g = colorRgb.g;
                decimal b = colorRgb.b;

                if (pdfColor.ColorSpace == ColorSpace.DeviceCMYK)
                {
                    r *= 0.8m;
                    g *= 0.8m;
                    b *= 0.8m;
                }

                if (pdfColor is AlphaColor alphaColor)
                {
                    return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(alphaColor.A * 255));
                }
                return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));

                
            }
            return SKColors.Black;
        }

        public static SKColor GetCurrentNonStrokingColorSKColor(this CurrentGraphicsState currentGraphicsState)
        {
            if (currentGraphicsState.AlphaConstantNonStroking != 1)
            {
                return new AlphaColor(currentGraphicsState.AlphaConstantNonStroking, currentGraphicsState.CurrentNonStrokingColor).ToSKColor();
            }
            return currentGraphicsState.CurrentNonStrokingColor.ToSKColor();
        }

        public static SKColor GetCurrentStrokingColorSKColor(this CurrentGraphicsState currentGraphicsState)
        {
            if (currentGraphicsState.AlphaConstantStroking != 1)
            {
                return new AlphaColor(currentGraphicsState.AlphaConstantStroking, currentGraphicsState.CurrentStrokingColor).ToSKColor();
            }
            return currentGraphicsState.CurrentStrokingColor.ToSKColor();
        }
    }
}
