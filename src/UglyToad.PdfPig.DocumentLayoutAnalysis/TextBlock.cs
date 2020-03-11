namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using Content;
    using Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Geometry;

    /// <summary>
    /// A block of text.
    /// </summary>
    public class TextBlock
    {
        /// <summary>
        /// The text of the block.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The text direction of the block.
        /// </summary>
        public TextDirection TextDirection { get; }

        /// <summary>
        /// The rectangle completely containing the block.
        /// </summary>
        public PdfRectangle BoundingBox { get; }

        /// <summary>
        /// The text lines contained in the block.
        /// </summary>
        public IReadOnlyList<TextLine> TextLines { get; }

        /// <summary>
        /// The reading order index. Starts at 0. A value of -1 means the block is not ordered.
        /// </summary>
        public int ReadingOrder { get; private set; }

        /// <summary>
        /// Create a new <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="lines"></param>
        public TextBlock(IReadOnlyList<TextLine> lines)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (lines.Count == 0)
            {
                throw new ArgumentException("Empty lines provided.", nameof(lines));
            }

            ReadingOrder = -1;

            TextLines = lines;

            Text = string.Join(" ", lines.Select(x => x.Text));

            var tempTextDirection = lines[0].TextDirection;
            if (tempTextDirection != TextDirection.Other)
            {
                foreach (var line in lines)
                {
                    if (line.TextDirection != tempTextDirection)
                    {
                        tempTextDirection = TextDirection.Other;
                        break;
                    }
                }
            }

            switch (tempTextDirection)
            {
                case TextDirection.Horizontal:
                    BoundingBox = GetBoundingBoxH(lines);
                    break;

                case TextDirection.Rotate180:
                    BoundingBox = GetBoundingBox180(lines);
                    break;

                case TextDirection.Rotate90:
                    BoundingBox = GetBoundingBox90(lines);
                    break;

                case TextDirection.Rotate270:
                    BoundingBox = GetBoundingBox270(lines);
                    break;

                case TextDirection.Other:
                default:
                    BoundingBox = GetBoundingBoxOther(lines);
                    break;
            }

            TextDirection = lines[0].TextDirection;
        }

        /// <summary>
        /// Sets the <see cref="TextBlock"/>'s reading order.
        /// </summary>
        /// <param name="readingOrder"></param>
        public void SetReadingOrder(int readingOrder)
        {
            if (readingOrder < -1)
            {
                throw new ArgumentException("The reading order should be more or equal to -1. A value of -1 means the block is not ordered.", nameof(readingOrder));
            }
            this.ReadingOrder = readingOrder;
        }


        #region Bounding box
        private PdfRectangle GetBoundingBoxH(IReadOnlyList<TextLine> lines)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.BoundingBox.BottomLeft.X < minX)
                {
                    minX = line.BoundingBox.BottomLeft.X;
                }

                if (line.BoundingBox.BottomLeft.Y < minY)
                {
                    minY = line.BoundingBox.BottomLeft.Y;
                }

                var right = line.BoundingBox.BottomLeft.X + line.BoundingBox.Width;
                if (right > maxX)
                {
                    maxX = right;
                }

                if (line.BoundingBox.Top > maxY)
                {
                    maxY = line.BoundingBox.Top;
                }
            }

            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        private PdfRectangle GetBoundingBox180(IReadOnlyList<TextLine> lines)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var minY = double.MaxValue;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.BoundingBox.BottomLeft.X > maxX)
                {
                    maxX = line.BoundingBox.BottomLeft.X;
                }

                if (line.BoundingBox.BottomLeft.Y > maxY)
                {
                    maxY = line.BoundingBox.BottomLeft.Y;
                }

                var right = line.BoundingBox.BottomLeft.X + line.BoundingBox.Width;
                if (right < minX)
                {
                    minX = right;
                }

                if (line.BoundingBox.Top < minY)
                {
                    minY = line.BoundingBox.Top;
                }
            }

            return new PdfRectangle(maxX, maxY, minX, minY);
        }

        private PdfRectangle GetBoundingBox90(IReadOnlyList<TextLine> lines)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.BoundingBox.BottomLeft.X < minX)
                {
                    minX = line.BoundingBox.BottomLeft.X;
                }

                if (line.BoundingBox.BottomRight.Y < minY)
                {
                    minY = line.BoundingBox.BottomRight.Y;
                }

                var right = line.BoundingBox.BottomLeft.X - line.BoundingBox.Height;
                if (right > maxX)
                {
                    maxX = right;
                }

                if (line.BoundingBox.Top > maxY)
                {
                    maxY = line.BoundingBox.Top;
                }
            }

            return new PdfRectangle(new PdfPoint(maxX, maxY),
                                    new PdfPoint(maxX, minY),
                                    new PdfPoint(minX, maxY),
                                    new PdfPoint(minX, minY));
        }

        private PdfRectangle GetBoundingBox270(IReadOnlyList<TextLine> lines)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.BoundingBox.BottomLeft.X > maxX)
                {
                    maxX = line.BoundingBox.BottomLeft.X;
                }

                if (line.BoundingBox.BottomLeft.Y < minY)
                {
                    minY = line.BoundingBox.BottomLeft.Y;
                }

                var right = line.BoundingBox.BottomLeft.X - line.BoundingBox.Height;
                if (right < minX)
                {
                    minX = right;
                }

                if (line.BoundingBox.Bottom > maxY)
                {
                    maxY = line.BoundingBox.Bottom;
                }
            }

            return new PdfRectangle(new PdfPoint(minX, minY),
                                    new PdfPoint(minX, maxY),
                                    new PdfPoint(maxX, minY),
                                    new PdfPoint(maxX, maxY));
        }

        private PdfRectangle GetBoundingBoxOther(IReadOnlyList<TextLine> lines)
        {
            return GeometryExtensions.MinimumAreaBoundingBox(lines.SelectMany(r => new[]
            {
                r.BoundingBox.BottomLeft,
                r.BoundingBox.BottomRight,
                r.BoundingBox.TopLeft,
                r.BoundingBox.TopRight
            }));
        }
        #endregion


        /// <inheritdoc />
        public override string ToString()
        {
            return Text;
        }
    }
}
