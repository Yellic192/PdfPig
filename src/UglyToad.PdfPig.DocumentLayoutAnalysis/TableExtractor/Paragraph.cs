namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A paragraph (text not contained in tables)
    /// </summary>
    /// <seealso cref="UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor.IPageContent" />
    /// <seealso cref="System.IFormattable" />
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class Paragraph : IPageContent, IFormattable
    {
        private List<ParagraphContent> _Contents = new List<ParagraphContent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Paragraph"/> class.
        /// </summary>
        /// <param name="y">The y.</param>
        public Paragraph(double y)
        {

            Y = y;
        }

        /// <summary>
        /// Gets the y coordinate of the paragraph.
        /// </summary>
        /// <value>
        /// The y coordinate.
        /// </value>
        public double Y { get; private set; }

        /// <summary>
        /// Gets the content of the paragraph.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content
        {
            get
            {
                string result = null;
                foreach (ParagraphContent content in _Contents.OrderBy(_ => _.Point.X))
                {
                    if (result == null)
                        result = content.Content;
                    else
                        result = result + " " + content.Content;
                }

                return result;
            }

        }

        /// <summary>
        /// Adds the text at the specified position
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="InvalidOperationException">The point is not on the paragraph</exception>
        public void AddText(Point point, string content)
        {
            if (!Contains(point))
                throw new InvalidOperationException("The point is not on the paragraph");

            _Contents.Add(new ParagraphContent(point, content));
        }

        /// <summary>
        /// Determines whether this instance contains the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///   <c>true</c> if the paragraph content contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Point point)
        {
            return Y - ContentExtractor.Tolerance < point.Y && point.Y < Y + ContentExtractor.Tolerance * 3;
        }

        /// <summary>
        /// Determines whether this paragraph contains the y coordinate.
        /// </summary>
        /// <param name="y">The y coordinate.</param>
        /// <returns>
        ///   <c>true</c> if the paragraph contains the specified y coordinate; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(double y)
        {
            return Y - ContentExtractor.Tolerance < y && y < Y + ContentExtractor.Tolerance * 3;
        }

        #region IFormattable

        // ReSharper disable once UnusedMember.Local
        private string DebuggerDisplay
        {
            get { return ToString("d"); }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString("");
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <exception cref="FormatException"></exception>
        public string ToString(string format)
        {
            switch (format)
            {
                case "s":
                case "":
                case null:
                    return Content;
                case "d":
                    return string.Format("{0} {1}", Y, Content);
                default:
                    throw new FormatException();
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format);
        }

        #endregion


        private class ParagraphContent
        {
            public ParagraphContent(Point point, string content)
            {
                Point = point;
                Content = content;
            }

            public Point Point { get; private set; }
            public string Content { get; private set; }
        }

    }

}
