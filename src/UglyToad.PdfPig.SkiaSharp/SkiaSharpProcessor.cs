namespace UglyToad.PdfPig.SkiaSharp
{
    using global::SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig.Annotations;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Fonts.SystemFonts;
    using UglyToad.PdfPig.Geometry;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Graphics.Operations;
    using UglyToad.PdfPig.PdfFonts;
    using UglyToad.PdfPig.Rendering;
    using UglyToad.PdfPig.Tokens;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    public class SkiaSharpProcessor : BaseRenderStreamProcessor
    {
        private int _height;
        private int _width;
        private double _mult;
        private SKCanvas _canvas;
        private Page _page;

        public SkiaSharpProcessor(Page page) : base(page)
        {
            _page = page;
        }

        private int ToInt(double value)
        {
            return (int)Math.Ceiling(value * _mult);
        }

        private static string CleanFontName(string font)
        {
            if (font.Length > 7 && font[6].Equals('+'))
            {
                string subset = font.Substring(0, 6);
                if (subset.Equals(subset.ToUpper()))
                {
                    return font.Split('+')[1];
                }
            }

            return font;
        }

        public override MemoryStream GetImage(double scale)
        {
            _mult = scale;
            _height = ToInt(_page.Height);
            _width = ToInt(_page.Width);
            return Process(-1, _page.Operations);
        }

        public override MemoryStream Process(int pageNumberCurrent, IReadOnlyList<IGraphicsStateOperation> operations)
        {
            var ms = new MemoryStream();

            CloneAllStates();

            using (var bitmap = new SKBitmap(_width, _height))
            using (_canvas = new SKCanvas(bitmap))
            {
                using (var paint = new SKPaint() { Style = SKPaintStyle.Fill, Color = SKColors.White })
                {
                    _canvas.DrawRect(0, 0, _width, _height, paint);
                }

                DrawAnnotations(true);

                ProcessOperations(operations);

                DrawAnnotations(false);

                using (SKData d = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    d.SaveTo(ms);
                }
            }
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Very hackish
        /// </summary>
        private static bool IsAnnotationBelowText(Annotation annotation)
        {
            if (annotation.Type == AnnotationType.Highlight)
            {
                return true;
            }
            return false;
        }

        private void DrawAnnotations(bool isBelowText)
        {
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawer.java
            // https://github.com/apache/pdfbox/blob/c4b212ecf42a1c0a55529873b132ea338a8ba901/pdfbox/src/main/java/org/apache/pdfbox/contentstream/PDFStreamEngine.java#L312
            foreach (var annotation in _page.ExperimentalAccess.GetAnnotations().Where(a => IsAnnotationBelowText(a) == isBelowText))
            {
                // Check if visible

                // Get appearance
                var appearance = base.GetNormalAppearanceAsStream(annotation);

                PdfRectangle? bbox = null;
                PdfRectangle? rect = annotation.Rectangle;

                if (!(appearance is null))
                {
                    if (appearance.StreamDictionary.TryGet<ArrayToken>(NameToken.Bbox, out var bboxToken))
                    {
                        var points = bboxToken.Data.OfType<NumericToken>().Select(x => x.Double).ToArray();
                        bbox = new PdfRectangle(points[0], points[1], points[2], points[3]);
                    }

                    // zero-sized rectangles are not valid
                    if (rect.HasValue && rect.Value.Width > 0 && rect.Value.Height > 0 &&
                        bbox.HasValue && bbox.Value.Width > 0 && bbox.Value.Height > 0)
                    {
                        var matrix = TransformationMatrix.Identity;
                        if (appearance.StreamDictionary.TryGet<ArrayToken>(NameToken.Matrix, out var matrixToken))
                        {
                            matrix = TransformationMatrix.FromArray(matrixToken.Data.OfType<NumericToken>().Select(x => x.Double).ToArray());
                        }

                        PushState();

                        // transformed appearance box  fixme: may be an arbitrary shape
                        PdfRectangle transformedBox = matrix.Transform(bbox.Value).Normalise();

                        // Matrix a = Matrix.getTranslateInstance(rect.getLowerLeftX(), rect.getLowerLeftY());
                        TransformationMatrix a = TransformationMatrix.GetTranslationMatrix(rect.Value.TopLeft.X, rect.Value.TopLeft.Y);
                        a = scale(a, (float)(rect.Value.Width / transformedBox.Width), (float)(rect.Value.Height / transformedBox.Height));
                        a = a.Translate(-transformedBox.TopLeft.X, -transformedBox.TopLeft.Y);

                        // Matrix shall be concatenated with A to form a matrix AA that maps from the appearance's
                        // coordinate system to the annotation's rectangle in default user space
                        //
                        // HOWEVER only the opposite order works for rotated pages with 
                        // filled fields / annotations that have a matrix in the appearance stream, see PDFBOX-3083
                        //Matrix aa = Matrix.concatenate(a, matrix);
                        //TransformationMatrix aa = a.Multiply(matrix);

                        GetCurrentState().CurrentTransformationMatrix = a;

                        try
                        {
                            base.ProcessFormXObject(appearance);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DrawAnnotations: {ex}");
                        }
                        finally
                        {
                            PopState();
                        }
                    }
                }
                else
                {
                    DebugDrawRect(annotation.Rectangle);
                }
            }
        }

        public TransformationMatrix scale(TransformationMatrix matrix, float sx, float sy)
        {
            var x0 = matrix[0, 0] * sx;
            var x1 = matrix[0, 1] * sx;
            var x2 = matrix[0, 2] * sx;
            var y0 = matrix[1, 0] * sy;
            var y1 = matrix[1, 1] * sy;
            var y2 = matrix[1, 2] * sy;
            return new TransformationMatrix(x0, x1, x2, y0, y1, y2, matrix[2, 0], matrix[2, 1], matrix[2, 2]);
        }

        private void DebugDrawRect(PdfRectangle rect)
        {
            using (new SKAutoCanvasRestore(_canvas))
            {
                var upperLeft = rect.TopLeft.ToSKPoint(_height, _mult);
                var destRect = new SKRect(upperLeft.X, upperLeft.Y,
                                 upperLeft.X + (float)(rect.Width * _mult),
                                 upperLeft.Y + (float)(rect.Height * _mult)).Standardized;

                // https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/curves/effects
                SKPathEffect diagLinesPath = SKPathEffect.Create2DLine(4 * (float)_mult,
                    SKMatrix.Concat(SKMatrix.CreateScale(6 * (float)_mult, 6 * (float)_mult), SKMatrix.CreateRotationDegrees(45)));

                var fillBrush = new SKPaint()
                {
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(SKColors.Aqua.Red, SKColors.Aqua.Green, SKColors.Aqua.Blue, 30),
                    PathEffect = diagLinesPath
                };
                _canvas.DrawRect(destRect, fillBrush);

                fillBrush = new SKPaint()
                {
                    Style = SKPaintStyle.Stroke,
                    Color = new SKColor(SKColors.Red.Red, SKColors.Red.Green, SKColors.Red.Blue, 30),
                    StrokeWidth = 5
                };
                _canvas.DrawRect(destRect, fillBrush);

                diagLinesPath.Dispose();
                fillBrush.Dispose();
            }
        }

        public override void RenderGlyph(IFont font, IColor color, double fontSize, double pointSize, int code, string unicode, long currentOffset,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix, CharacterBoundingBox characterBoundingBox)
        {
            try
            {
                if (font.TryGetNormalisedPath(code, out var path))
                {
                    ShowVectorFontGlyph(path, color, renderingMatrix, textMatrix, transformationMatrix);
                }
                else
                {
                    ShowNonVectorFontGlyph(font, color, pointSize, unicode, renderingMatrix, textMatrix, transformationMatrix, characterBoundingBox);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowGlyph: {ex}");
            }
        }

        private void ShowVectorFontGlyph(IReadOnlyList<PdfSubpath> path, IColor color,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix)
        {
            // Vector based font
            using (var gp = new SKPath() { FillType = SKPathFillType.EvenOdd })
            {
                foreach (var subpath in path)
                {
                    foreach (var c in subpath.Commands)
                    {
                        if (c is Move move)
                        {
                            var loc = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, move.Location);
                            gp.MoveTo((float)(loc.x * _mult), (float)(_height - loc.y * _mult));
                        }
                        else if (c is Line line)
                        {
                            var to = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, line.To);
                            gp.LineTo((float)(to.x * _mult), (float)(_height - to.y * _mult));
                        }
                        else if (c is BezierCurve curve)
                        {
                            var first = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.FirstControlPoint);
                            var second = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.SecondControlPoint);
                            var end = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.EndPoint);
                            gp.CubicTo((float)(first.x * _mult), (float)(_height - first.y * _mult),
                                       (float)(second.x * _mult), (float)(_height - second.y * _mult),
                                       (float)(end.x * _mult), (float)(_height - end.y * _mult));
                        }
                        else if (c is Close)
                        {
                            gp.Close();
                        }
                    }
                }

                SKPaint fillBrush = new SKPaint()
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black
                };

                if (color != null)
                {
                    fillBrush.Color = color.ToSKColor();
                }

                _canvas.DrawPath(gp, fillBrush);
                fillBrush.Dispose();
            }
        }

        private void ShowNonVectorFontGlyph(IFont font, IColor color, double pointSize, string unicode,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix,
            CharacterBoundingBox characterBoundingBox)
        {
            // Not vector based font
            var transformedGlyphBounds = PerformantRectangleTransformer
                .Transform(renderingMatrix, textMatrix, transformationMatrix, characterBoundingBox.GlyphBounds);

            var transformedPdfBounds = PerformantRectangleTransformer
                .Transform(renderingMatrix, textMatrix, transformationMatrix, new PdfRectangle(0, 0, characterBoundingBox.Width, 0));

            var startBaseLine = transformedPdfBounds.BottomLeft.ToSKPoint(_height, _mult);
            if (transformedGlyphBounds.Rotation != 0)
            {
                _canvas.RotateDegrees((float)-transformedGlyphBounds.Rotation, startBaseLine.X, startBaseLine.Y);
            }

#if DEBUG
            /*
            var glyphRectangleNormalise = transformedGlyphBounds.Normalise();

            var upperLeftNorm = glyphRectangleNormalise.TopLeft.ToSKPoint(_height, _mult);
            var bottomLeftNorm = glyphRectangleNormalise.BottomLeft.ToSKPoint(_height, _mult);
            SKRect rect = new SKRect(upperLeftNorm.X, upperLeftNorm.Y,
                upperLeftNorm.X + (float)(glyphRectangleNormalise.Width * _mult),
                upperLeftNorm.Y + (float)(glyphRectangleNormalise.Height * _mult));
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(SKColors.Red.Red, SKColors.Red.Green, SKColors.Red.Blue, 40)
            };

            _canvas.DrawRect(rect, paint);

            paint.Style = SKPaintStyle.Stroke;

            _canvas.DrawRect(rect, paint);
            paint.Dispose();
            */
#endif

            var style = SKFontStyle.Normal;
            // TODO - Use font weight instead
            if (font.Details.IsBold && font.Details.IsItalic)
            {
                style = SKFontStyle.BoldItalic;
            }
            else if (font.Details.IsBold)
            {
                style = SKFontStyle.Bold;
            }
            else if (font.Details.IsItalic)
            {
                style = SKFontStyle.Italic;
            }

            SKTypeface drawFont = null;

            if (drawFont is null)
            {
                string cleanName = CleanFontName(font.Name);
                drawFont = SKTypeface.FromFamilyName(cleanName, style);

                if (!drawFont.FamilyName.Equals(cleanName, StringComparison.OrdinalIgnoreCase) &&
                    SystemFontFinder.NameSubstitutes.TryGetValue(font.Name, out string[] subs) &&
                    subs != null)
                {
                    foreach (var sub in subs)
                    {
                        drawFont = SKTypeface.FromFamilyName(sub, style);
                        if (drawFont.FamilyName.Equals(sub))
                        {
                            break;
                        }
                    }
                }

                // https://superuser.com/questions/679753/what-is-this-zapfdingbats-font-that-isnt-in-the-fonts-folder
                if (font.Name.Data.Equals("ZapfDingbats"))
                {
                    // Wingdings 
                    //drawFont = SKTypeface.FromFamilyName("Wingdings", style);
                }
            }

            var fontPaint = new SKPaint(drawFont.ToFont((float)(pointSize * _mult)))
            {
                Color = color.ToSKColor()
            };

            //System.Diagnostics.Debug.WriteLine($"DrawLetter: '{font.Name}'\t{fontSize} -> Font name used: '{drawFont.FamilyName}'");

            _canvas.DrawText(unicode, startBaseLine, fontPaint);
            _canvas.ResetMatrix();

            fontPaint.Dispose();
            drawFont.Dispose();
            style.Dispose();
        }

        private SKPath CurrentPath { get; set; }

        public override void BeginSubpath()
        {
            if (CurrentPath == null)
            {
                CurrentPath = new SKPath();
            }
        }

        public override PdfPoint? CloseSubpath()
        {
            CurrentPath.Close();
            return null;
        }

        public override void MoveTo(double x, double y)
        {
            BeginSubpath();
            var point = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            float xs = (float)(point.X * _mult);
            float ys = (float)(_height - point.Y * _mult);

            CurrentPath.MoveTo(xs, ys);
        }

        public override void LineTo(double x, double y)
        {
            var point = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            float xs = (float)(point.X * _mult);
            float ys = (float)(_height - point.Y * _mult);

            CurrentPath.LineTo(xs, ys);
        }

        public override void BezierCurveTo(double x2, double y2, double x3, double y3)
        {
            var controlPoint2 = CurrentTransformationMatrix.Transform(new PdfPoint(x2, y2));
            var end = CurrentTransformationMatrix.Transform(new PdfPoint(x3, y3));
            float x2s = (float)(controlPoint2.X * _mult);
            float y2s = (float)(_height - controlPoint2.Y * _mult);
            float x3s = (float)(end.X * _mult);
            float y3s = (float)(_height - end.Y * _mult);

            CurrentPath.QuadTo(x2s, y2s, x3s, y3s);
        }

        public override void BezierCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            var controlPoint1 = CurrentTransformationMatrix.Transform(new PdfPoint(x1, y1));
            var controlPoint2 = CurrentTransformationMatrix.Transform(new PdfPoint(x2, y2));
            var end = CurrentTransformationMatrix.Transform(new PdfPoint(x3, y3));
            float x1s = (float)(controlPoint1.X * _mult);
            float y1s = (float)(_height - controlPoint1.Y * _mult);
            float x2s = (float)(controlPoint2.X * _mult);
            float y2s = (float)(_height - controlPoint2.Y * _mult);
            float x3s = (float)(end.X * _mult);
            float y3s = (float)(_height - end.Y * _mult);

            CurrentPath.CubicTo(x1s, y1s, x2s, y2s, x3s, y3s);
        }

        public override void ClosePath()
        {
            // TODO - to check, does nothing
        }

        public override void EndPath()
        {
            if (CurrentPath == null)
            {
                return;
            }

            // TODO
            CurrentPath.Dispose();
            CurrentPath = null;
        }

        public override void Rectangle(double x, double y, double width, double height)
        {
            BeginSubpath();
            var lowerLeft = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            var upperRight = CurrentTransformationMatrix.Transform(new PdfPoint(x + width, y + height));
            float left = (float)(lowerLeft.X * _mult);
            float top = (float)(_height - upperRight.Y * _mult);
            float right = (float)(upperRight.X * _mult);
            float bottom = (float)(_height - lowerLeft.Y * _mult);
            SKRect rect = new SKRect(left, top, right, bottom);
            CurrentPath.AddRect(rect);
        }

        private float GetScaledLineWidth()
        {
            var currentState = GetCurrentState();
            // https://stackoverflow.com/questions/25690496/how-does-pdf-line-width-interact-with-the-ctm-in-both-horizontal-and-vertical-di
            // TODO - a hack but works, to put in ContentStreamProcessor
            return (float)(float)(currentState.LineWidth * (decimal)currentState.CurrentTransformationMatrix.A);
        }

        public override void StrokePath(bool close)
        {
            if (close)
            {
                CurrentPath.Close();
            }

            var currentState = GetCurrentState();

            PaintStrokePath(currentState);

            CurrentPath.Dispose();
            CurrentPath = null;
        }

        private void PaintStrokePath(CurrentGraphicsState currentGraphicsState)
        {
            using (SKPaint paint = new SKPaint())
            {
                float lineWidth = Math.Max((float)0.5, GetScaledLineWidth()) * (float)_mult; // A guess

                paint.Color = currentGraphicsState.CurrentStrokingColor.ToSKColor();
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = lineWidth;
                paint.StrokeJoin = currentGraphicsState.JoinStyle.ToSKStrokeJoin();
                paint.StrokeCap = currentGraphicsState.CapStyle.ToSKStrokeCap();

                var pathEffect = currentGraphicsState.LineDashPattern.ToSKPathEffect(_mult);
                if (pathEffect != null)
                {
                    paint.PathEffect = pathEffect;
                }

                _canvas.DrawPath(CurrentPath, paint);
            }
        }

        public override void FillPath(FillingRule fillingRule, bool close)
        {
            if (close)
            {
                CurrentPath.Close();
            }

            var currentState = GetCurrentState();

            PaintFillPath(currentState, fillingRule);

            CurrentPath.Dispose();
            CurrentPath = null;
        }

        private void PaintFillPath(CurrentGraphicsState currentGraphicsState, FillingRule fillingRule)
        {
            CurrentPath.FillType = fillingRule.ToSKPathFillType();

            using (SKPaint paint = new SKPaint())
            {
                paint.Color = currentGraphicsState.CurrentNonStrokingColor.ToSKColor();
                paint.Style = SKPaintStyle.Fill;
                _canvas.DrawPath(CurrentPath, paint);
            }
        }

        public override void FillStrokePath(FillingRule fillingRule, bool close)
        {
            if (close)
            {
                CurrentPath.Close();
            }

            var currentState = GetCurrentState();

            PaintFillPath(currentState, fillingRule);
            PaintStrokePath(currentState);

            CurrentPath.Dispose();
            CurrentPath = null;
        }

        public override void PopState()
        {
            base.PopState();
            _canvas.Restore();
        }

        public override void PushState()
        {
            base.PushState();
            _canvas.Save();
        }

        public override void ModifyClippingIntersect(FillingRule clippingRule)
        {
            CurrentPath.FillType = clippingRule.ToSKPathFillType();
            _canvas.ClipPath(CurrentPath, SKClipOperation.Intersect);
        }

        public override void RenderXObjectImage(XObjectContentRecord xObjectContentRecord)
        {
            var image = GetImageFromXObject(xObjectContentRecord);
            RenderImage(image);
        }

        public override void RenderInlineImage(InlineImage inlineImage)
        {
            RenderImage(inlineImage);
        }

        private void RenderImage(IPdfImage image)
        {
            // see issue_484Test, Pig production p15
            // need better handling for images where rotation is not 180
            float left = (float)(image.Bounds.Left * _mult);
            float top = (float)(_height - image.Bounds.Top * _mult);
            float right = left + (float)(image.Bounds.Width * _mult);
            float bottom = top + (float)(image.Bounds.Height * _mult);
            var destRect = new SKRect(left, top, right, bottom);

            byte[] bytes = null;

            try
            {
                if (!image.TryGetPng(out bytes))
                {
                    if (image.TryGetBytes(out var bytesL))
                    {
                        bytes = bytesL.ToArray();
                    }
                }

                if (bytes?.Length > 0)
                {
                    //using (Stream s = new FileStream($"{DateTime.UtcNow.Ticks}_skia-sharp.png", FileMode.Create))
                    //using (var bitmap = SKBitmap.Decode(bytes, skImageInfo))
                    //{
                    //    SKData d = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
                    //    d.SaveTo(s);
                    //}

                    try
                    {
                        using (var bitmap = SKBitmap.Decode(bytes))
                        {
                            _canvas.DrawBitmap(bitmap, destRect);
                        }
                    }
                    catch (Exception)
                    {
                        // Try with raw bytes
                        using (var bitmap = SKBitmap.Decode(image.RawBytes.ToArray()))
                        {
                            _canvas.DrawBitmap(bitmap, destRect);
                        }
                    }
                }
                else
                {
                    bytes = image.RawBytes.ToArray();
                    using (var bitmap = SKBitmap.Decode(bytes))
                    {
                        _canvas.DrawBitmap(bitmap, destRect);
                    }
                }
            }
            catch (Exception)
            {
#if DEBUG
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(SKColors.GreenYellow.Red, SKColors.GreenYellow.Green, SKColors.GreenYellow.Blue, 40)
                })
                {
                    _canvas.DrawRect(destRect, paint);
                }
#endif
            }
            finally
            {
                _canvas.ResetMatrix();
            }
        }

        protected override void RenderShading(DictionaryToken shading)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = SKColors.Violet;
                paint.Style = SKPaintStyle.Fill;
                _canvas.DrawPath(GetCurrentState().CurrentClippingPath.PdfSubpathsToGraphicsPath(_height, _mult), paint); // Not correct
            }
        }
    }
}
