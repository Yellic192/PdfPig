namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using Content;
    using Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A line of text.
    /// </summary>
    public class TextLine
    {
        /// <summary>
        /// The text of the line.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The text direction of the line.
        /// </summary>
        public TextDirection TextDirection { get; }

        /// <summary>
        /// The rectangle completely containing the line.
        /// </summary>
        public PdfRectangle BoundingBox { get; }

        /// <summary>
        /// The words contained in the line.
        /// </summary>
        public IReadOnlyList<Word> Words { get; }

        /// <summary>
        /// Create a new <see cref="TextLine"/>.
        /// </summary>
        /// <param name="words">The words contained in the line.</param>
        public TextLine(IReadOnlyList<Word> words)
        {
            if (words == null)
            {
                throw new ArgumentNullException(nameof(words));
            }

            if (words.Count == 0)
            {
                throw new ArgumentException("Empty words provided.", nameof(words));
            }

            Words = words;

            Text = string.Join(" ", words.Where(s => !string.IsNullOrWhiteSpace(s.Text)).Select(x => x.Text));

            var tempTextDirection = words[0].TextDirection;
            if (tempTextDirection != TextDirection.Other)
            {
                foreach (var word in words)
                {
                    if (word.TextDirection != tempTextDirection)
                    {
                        tempTextDirection = TextDirection.Other;
                        break;
                    }
                }
            }

            switch (tempTextDirection)
            {
                case TextDirection.Horizontal:
                    BoundingBox = GetBoundingBoxH(words);
                    break;

                case TextDirection.Rotate180:
                    BoundingBox = GetBoundingBox180(words);
                    break;

                case TextDirection.Rotate90:
                    BoundingBox = GetBoundingBox90(words);
                    break;

                case TextDirection.Rotate270:
                    BoundingBox = GetBoundingBox270(words);
                    break;

                case TextDirection.Other:
                default:
                    BoundingBox = GetBoundingBoxOther(words);
                    break;
            }

            TextDirection = tempTextDirection;
        }

        #region Bounding box
        private PdfRectangle GetBoundingBoxH(IReadOnlyList<Word> words)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];

                if (word.BoundingBox.BottomLeft.X < minX)
                {
                    minX = word.BoundingBox.BottomLeft.X;
                }

                if (word.BoundingBox.BottomLeft.Y < minY)
                {
                    minY = word.BoundingBox.BottomLeft.Y;
                }

                var right = word.BoundingBox.BottomLeft.X + word.BoundingBox.Width;
                if (right > maxX)
                {
                    maxX = right;
                }

                if (word.BoundingBox.Top > maxY)
                {
                    maxY = word.BoundingBox.Top;
                }
            }

            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        private PdfRectangle GetBoundingBox180(IReadOnlyList<Word> words)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var minY = double.MaxValue;

            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];

                if (word.BoundingBox.BottomLeft.X > maxX)
                {
                    maxX = word.BoundingBox.BottomLeft.X;
                }

                if (word.BoundingBox.BottomLeft.Y > maxY)
                {
                    maxY = word.BoundingBox.BottomLeft.Y;
                }

                var right = word.BoundingBox.BottomLeft.X + word.BoundingBox.Width;
                if (right < minX)
                {
                    minX = right;
                }

                if (word.BoundingBox.Top < minY)
                {
                    minY = word.BoundingBox.Top;
                }
            }

            return new PdfRectangle(maxX, maxY, minX, minY);
        }

        private PdfRectangle GetBoundingBox90(IReadOnlyList<Word> words)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];

                if (word.BoundingBox.BottomLeft.X < minX)
                {
                    minX = word.BoundingBox.BottomLeft.X;
                }

                if (word.BoundingBox.BottomRight.Y < minY)
                {
                    minY = word.BoundingBox.BottomRight.Y;
                }

                var right = word.BoundingBox.BottomLeft.X - word.BoundingBox.Height;
                if (right > maxX)
                {
                    maxX = right;
                }

                if (word.BoundingBox.Top > maxY)
                {
                    maxY = word.BoundingBox.Top;
                }
            }

            return new PdfRectangle(new PdfPoint(maxX, maxY),
                                    new PdfPoint(maxX, minY),
                                    new PdfPoint(minX, maxY),
                                    new PdfPoint(minX, minY));
        }

        private PdfRectangle GetBoundingBox270(IReadOnlyList<Word> words)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];

                if (word.BoundingBox.BottomLeft.X > maxX)
                {
                    maxX = word.BoundingBox.BottomLeft.X;
                }

                if (word.BoundingBox.BottomLeft.Y < minY)
                {
                    minY = word.BoundingBox.BottomLeft.Y;
                }

                var right = word.BoundingBox.BottomLeft.X - word.BoundingBox.Height;
                if (right < minX)
                {
                    minX = right;
                }

                if (word.BoundingBox.Bottom > maxY)
                {
                    maxY = word.BoundingBox.Bottom;
                }
            }

            return new PdfRectangle(new PdfPoint(minX, minY),
                                    new PdfPoint(minX, maxY),
                                    new PdfPoint(maxX, minY),
                                    new PdfPoint(maxX, maxY));
        }

        private PdfRectangle GetBoundingBoxOther(IReadOnlyList<Word> words)
        {
            var baseLinePoints = words.SelectMany(r => new[]
            {
                r.BoundingBox.BottomLeft,
                r.BoundingBox.BottomRight,
            }).ToList();

            // Fitting a line through the base lines points
            // to find the orientation (slope)
            double x0 = baseLinePoints.Average(p => p.X);
            double y0 = baseLinePoints.Average(p => p.Y);
            double sumProduct = 0;
            double sumDiffSquaredX = 0;

            for (int i = 0; i < baseLinePoints.Count; i++)
            {
                var point = baseLinePoints[i];
                var x_diff = point.X - x0;
                var y_diff = point.Y - y0;
                sumProduct += x_diff * y_diff;
                sumDiffSquaredX += x_diff * x_diff;
            }

            var slope = sumProduct / sumDiffSquaredX;

            // Rotate the points to build the axis-aligned bounding box (AABB)
            var angleRad = Math.Atan(slope);
            var cos = Math.Cos(angleRad);
            var sin = Math.Sin(angleRad);

            var inverseRotation = new TransformationMatrix(
                cos, -sin, 0,
                sin, cos, 0,
                0, 0, 1);

            var transformedPoints = words.SelectMany(r => new[]
            {
                r.BoundingBox.BottomLeft,
                r.BoundingBox.BottomRight,
                r.BoundingBox.TopLeft,
                r.BoundingBox.TopRight
            }).Distinct().Select(p => inverseRotation.Transform(p));
            var aabb = new PdfRectangle(transformedPoints.Min(p => p.X),
                                        transformedPoints.Min(p => p.Y),
                                        transformedPoints.Max(p => p.X),
                                        transformedPoints.Max(p => p.Y));

            // Rotate back the AABB to obtain to oriented bounding box (OBB)
            var rotateBack = new TransformationMatrix(
                cos, sin, 0,
                -sin, cos, 0,
                0, 0, 1);

            // Candidates bounding boxes
            var obb = rotateBack.Transform(aabb);
            var obb1 = new PdfRectangle(obb.BottomLeft, obb.TopLeft, obb.BottomRight, obb.TopRight);
            var obb2 = new PdfRectangle(obb.TopRight, obb.BottomRight, obb.TopLeft, obb.BottomLeft);
            var obb3 = new PdfRectangle(obb.BottomRight, obb.BottomLeft, obb.TopRight, obb.TopLeft);

            // Find the orientation of the OBB, using the baseline angle
            var firstWord = words[0];
            var lastWord = words[words.Count - 1];
            var baseLineAngle = Math.Atan2(
                lastWord.BoundingBox.BottomRight.Y - firstWord.BoundingBox.BottomLeft.Y,
                lastWord.BoundingBox.BottomRight.X - firstWord.BoundingBox.BottomLeft.X) * 180 / Math.PI;

            var bbox = obb;
            var deltaAngle = Math.Abs(baseLineAngle - angleRad * 180 / Math.PI);

            double deltaAngle1 = Math.Abs(baseLineAngle - obb1.Rotation);
            if (deltaAngle1 < deltaAngle)
            {
                deltaAngle = deltaAngle1;
                bbox = obb1;
            }

            double deltaAngle2 = Math.Abs(baseLineAngle - obb2.Rotation);
            if (deltaAngle2 < deltaAngle)
            {
                deltaAngle = deltaAngle2;
                bbox = obb2;
            }

            double deltaAngle3 = Math.Abs(baseLineAngle - obb3.Rotation);
            if (deltaAngle3 < deltaAngle)
            {
                bbox = obb3;
            }
            return bbox;
        }
        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Text;
        }
    }
}
