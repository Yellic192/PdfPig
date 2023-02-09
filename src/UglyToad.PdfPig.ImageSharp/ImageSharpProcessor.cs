namespace UglyToad.PdfPig.ImageSharp
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using System.Collections.Generic;
    using System.IO;
    using UglyToad.PdfPig.Annotations;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Graphics.Operations;
    using UglyToad.PdfPig.PdfFonts;
    using UglyToad.PdfPig.Rendering;
    using UglyToad.PdfPig.Tokens;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    public class ImageSharpProcessor : BaseRenderStreamProcessor
    {
        private int _height;
        private int _width;
        private double _mult;
        //private SKCanvas _canvas;
        private Page _page;

        private Image<Rgba32> _canvas;

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

        public ImageSharpProcessor(Page page) : base(page)
        {
            _page = page;
        }

        public override MemoryStream GetImage(double scale)
        {
            _mult = scale;
            _height = ToInt(_page.Height);
            _width = ToInt(_page.Width);
            return Process(_page.Number, _page.Operations);
        }

        public override MemoryStream Process(int pageNumberCurrent, IReadOnlyList<IGraphicsStateOperation> operations)
        {
            var ms = new MemoryStream();

            CloneAllStates();

            using (_canvas = new Image<Rgba32>(_width, _height))
            {
                _canvas.Mutate(x => x.BackgroundColor(Color.White));

                DrawAnnotations(true);
                ProcessOperations(operations);
                DrawAnnotations(false);

                _canvas.SaveAsPng(ms);
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
            // TODO
        }

        protected override void PaintShading(DictionaryToken shading)
        {
            var clippingPath = GetCurrentState().CurrentClippingPath.ToGraphicsPath(_height, _mult);
            //image.Mutate(x => x.Fill(Color.Violet, clippingPath));
        }

        public override void ShowGlyph(IFont font, IColor color, double fontSize, double pointSize, int code, string unicode, long currentOffset, TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix, CharacterBoundingBox characterBoundingBox)
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
            var gp = new PathBuilder();
            foreach (var subpath in path)
            {
                gp.StartFigure(); // TODO - necessary?
                foreach (var c in subpath.Commands)
                {
                    if (c is Move move)
                    {
                        var loc = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, move.Location);
                        gp.MoveTo(new PointF((float)(loc.x * _mult), (float)(_height - loc.y * _mult)));
                    }
                    else if (c is Line line)
                    {
                        var to = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, line.To);
                        gp.LineTo((float)(to.x * _mult), (float)(_height - to.y * _mult));
                    }
                    else if (c is BezierCurve curve)
                    {
                        var start = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.StartPoint);
                        var first = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.FirstControlPoint);
                        var second = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.SecondControlPoint);
                        var end = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.EndPoint);
                        gp.AddCubicBezier(new PointF((float)(start.x * _mult), (float)(_height - start.y * _mult)),
                                          new PointF((float)(first.x * _mult), (float)(_height - first.y * _mult)),
                                          new PointF((float)(second.x * _mult), (float)(_height - second.y * _mult)),
                                          new PointF((float)(end.x * _mult), (float)(_height - end.y * _mult)));
                    }
                    else if (c is Close)
                    {
                        gp.CloseFigure();
                    }
                }
            }

            Color fillBrush = Color.Black;

            if (color != null)
            {
                fillBrush = color.ToColor();
            }

            _canvas.Mutate(x => x.Fill(fillBrush, gp.Build()));
        }

        private void ShowNonVectorFontGlyph(IFont font, IColor color, double pointSize, string unicode,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix,
            CharacterBoundingBox characterBoundingBox)
        {
            // TODO
        }


        public override void ShowXObjectImage(XObjectContentRecord xObjectContentRecord)
        {
            var image = GetImageFromXObject(xObjectContentRecord);
            DrawImage(image);
        }

        private PathBuilder? CurrentPath { get; set; }

        public override void BeginSubpath()
        {
            if (CurrentPath == null)
            {
                CurrentPath = new PathBuilder();
            }
            CurrentPath.StartFigure();
        }

        public override PdfPoint? CloseSubpath()
        {
            CurrentPath.CloseFigure();
            return null;
        }

        public override void StrokePath(bool close)
        {
            if (close)
            {
                CurrentPath.CloseFigure();
            }

            var currentState = GetCurrentState();

            PaintStrokePath(currentState);

            CurrentPath.Clear();
            CurrentPath = null;
        }

        private void PaintStrokePath(CurrentGraphicsState currentGraphicsState)
        {
            float lineWidth = Math.Max((float)0.5, GetScaledLineWidth()) * (float)_mult; // A guess
            float[]? strokePattern = currentGraphicsState.LineDashPattern.ToStrokePattern(_mult);
            Pen pen = strokePattern != null ? new Pen(currentGraphicsState.CurrentStrokingColor.ToColor(), lineWidth, strokePattern)
                : new Pen(currentGraphicsState.CurrentStrokingColor.ToColor(), lineWidth);
            pen.JointStyle = currentGraphicsState.JoinStyle.ToJointStyle();
            pen.EndCapStyle = currentGraphicsState.CapStyle.ToEndCapStyle();

            _canvas.Mutate(x => x.Draw(pen, CurrentPath.Build()));
        }

        public override void FillPath(FillingRule fillingRule, bool close)
        {
            if (close)
            {
                CurrentPath.CloseFigure();
            }

            var currentState = GetCurrentState();

            PaintFillPath(currentState, fillingRule);

            CurrentPath.Clear();
            CurrentPath = null;
        }

        private void PaintFillPath(CurrentGraphicsState currentGraphicsState, FillingRule fillingRule)
        {
            var drawingOptions = new DrawingOptions()
            {
                ShapeOptions = new ShapeOptions()
                {
                    IntersectionRule = fillingRule.ToIntersectionRule()
                }
            };

            _canvas.Mutate(x => x.Fill(drawingOptions, currentGraphicsState.CurrentNonStrokingColor.ToColor(), CurrentPath.Build()));
        }

        public override void FillStrokePath(FillingRule fillingRule, bool close)
        {
            if (close)
            {
                CurrentPath.CloseFigure();
            }

            var currentState = GetCurrentState();

            PaintFillPath(currentState, fillingRule);
            PaintStrokePath(currentState);

            CurrentPath.Clear();
            CurrentPath = null;
        }

        public override void MoveTo(double x, double y)
        {
            BeginSubpath();
            var point = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            CurrentPosition = point;
            float xs = (float)(point.X * _mult);
            float ys = (float)(_height - point.Y * _mult);
            CurrentPath.MoveTo(new PointF(xs, ys));
        }

        public override void BezierCurveTo(double x2, double y2, double x3, double y3)
        {
            var controlPoint2 = CurrentTransformationMatrix.Transform(new PdfPoint(x2, y2));
            var end = CurrentTransformationMatrix.Transform(new PdfPoint(x3, y3));
            float x2s = (float)(controlPoint2.X * _mult);
            float y2s = (float)(_height - controlPoint2.Y * _mult);
            float x3s = (float)(end.X * _mult);
            float y3s = (float)(_height - end.Y * _mult);

            CurrentPath.AddQuadraticBezier(CurrentPosition.ToPointF(_height, _mult), new PointF(x2s, y2s), new PointF(x3s, y3s));
            CurrentPosition = end;
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

            CurrentPath.AddCubicBezier(CurrentPosition.ToPointF(_height, _mult), new PointF(x1s, y1s), new PointF(x2s, y2s), new PointF(x3s, y3s));
            CurrentPosition = end;
        }

        public override void LineTo(double x, double y)
        {
            var endPoint = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            float xs = (float)(endPoint.X * _mult);
            float ys = (float)(_height - endPoint.Y * _mult);

            CurrentPath.LineTo(xs, ys);
            CurrentPosition = endPoint;
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

            CurrentPath.Rectangle(left, top, right - left, bottom - top);
        }

        public override void EndPath()
        {
            if (CurrentPath == null)
            {
                return;
            }

            // TODO
            CurrentPath.Clear();
            CurrentPath = null;
        }

        public override void ClosePath()
        {
            // TODO - to check, does nothing
        }

        private float GetScaledLineWidth()
        {
            var currentState = GetCurrentState();
            // https://stackoverflow.com/questions/25690496/how-does-pdf-line-width-interact-with-the-ctm-in-both-horizontal-and-vertical-di
            // TODO - a hack but works, to put in ContentStreamProcessor
            return (float)(float)(currentState.LineWidth * (decimal)currentState.CurrentTransformationMatrix.A);
        }

        public override void ModifyClippingIntersect(FillingRule clippingRule)
        {
            //AddCurrentSubpath();
            //CurrentPath.SetClipping(clippingRule);

            //var currentClipping = GetCurrentState().CurrentClippingPath;
            //currentClipping.SetClipping(clippingRule);

            //var newClippings = CurrentPath.Clip(currentClipping, parsingOptions.Logger);
            //if (newClippings == null)
            //{
            //parsingOptions.Logger.Warn("Empty clipping path found. Clipping path not updated.");
            //}
            //else
            //{
            //    GetCurrentState().CurrentClippingPath = newClippings;
            //}
        }

        public override void ShowInlineImage(InlineImage inlineImage)
        {
            DrawImage(inlineImage);
        }

        private void DrawImage(IPdfImage image)
        {
            var upperLeft = image.Bounds.TopLeft.ToPointF(_height, _mult);
            var destRect = new RectangleF(upperLeft.X, upperLeft.Y,
                             upperLeft.X + (float)(image.Bounds.Width * _mult),
                             upperLeft.Y + (float)(image.Bounds.Height * _mult));
            var size = new Size(ToInt(image.Bounds.Width), ToInt(image.Bounds.Height));

            byte[]? bytes = null;

            if (image.Bounds.Rotation != 0)
            {
                //_canvas.RotateDegrees((float)-image.Bounds.Rotation, upperLeft.X, upperLeft.Y);
                //_canvas.RotateDegrees((float)image.Bounds.Rotation, destRect.MidX, destRect.MidY);
            }

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
                    try
                    {
                        using (var bitmap = Image.Load(bytes))
                        {
                            bitmap.Mutate(x => x.Resize(size));
                            _canvas.Mutate(x => x.DrawImage(bitmap, new Point((int)upperLeft.X, (int)upperLeft.Y), 1));
                            //_canvas.DrawBitmap(bitmap, destRect);
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        // Try with raw bytes
                        using (var bitmap = Image.Load(image.RawBytes.ToArray()))
                        {
                            bitmap.Mutate(x => x.Resize(size));
                            _canvas.Mutate(x => x.DrawImage(bitmap, new Point((int)upperLeft.X, (int)upperLeft.Y), 1));
                            //_canvas.DrawBitmap(bitmap, destRect);
                        }
                    }
                }
                else
                {
                    if (image.ImageDictionary.ContainsKey(NameToken.Filter) && image.ImageDictionary.Data[NameToken.Filter.Data] is NameToken filter)
                    {
                        if (filter.Equals(NameToken.JpxDecode))
                        {
                            //sKImageInfo.ColorSpace = SKColorSpace.CreateSrgbLinear();
                            //sKImageInfo.ColorType = SKColorType.Rgb888x;
                        }
                    }

                    using (var bitmap = Image.Load(image.RawBytes.ToArray()))
                    {
                        bitmap.Mutate(x => x.Resize(size));
                        _canvas.Mutate(x => x.DrawImage(bitmap, new Point((int)upperLeft.X, (int)upperLeft.Y), 1));
                        //_canvas.DrawBitmap(bitmap, destRect);
                    }
                }
            }
            catch (Exception)
            {
#if DEBUG
                //var paint = new SKPaint
                //{
                //    Style = SKPaintStyle.Fill,
                //    Color = new SKColor(SKColors.GreenYellow.Red, SKColors.GreenYellow.Green, SKColors.GreenYellow.Blue, 40)
                //};
                //_canvas.DrawRect(destRect, paint);
                //paint.Dispose();
#endif
            }
            finally
            {
                //_canvas.ResetMatrix();
            }
        }
    }
}