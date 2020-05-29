namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Core;

    /// <summary>
    /// Table
    /// </summary>
    /// <seealso cref="IPageContent" />
    /// <seealso cref="IFormattable" />
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class TableBlock : IPageContent, IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableBlock"/> class.
        /// </summary>
        public TableBlock()
        {
            Rows = new List<Row>();
            Columns = new List<Column>();
        }

        /// <summary>
        /// Gets or sets the top left point.
        /// </summary>
        /// <value>
        /// The top left point.
        /// </value>
        public PdfPoint TopLeftPoint => BoundingBox.TopLeft;

        /// <summary>
        /// Gets or sets the bottom right point.
        /// </summary>
        /// <value>
        /// The bottom right point.
        /// </value>
        public PdfPoint BottomRightPoint => BoundingBox.BottomRight;

        /// <summary>
        /// the bbox.
        /// </summary>
        public PdfRectangle BoundingBox { get; set; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public List<Row> Rows { get; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public List<Column> Columns { get; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public double Width => BoundingBox.Width;

        /// <summary>
        /// Gets the heigth.
        /// </summary>
        /// <value>
        /// The heigth.
        /// </value>
        public double Heigth => BoundingBox.Height;

        private Cell[,] content;

        /// <summary>
        /// Gets or sets the <see cref="string"/> with the specified row.
        /// </summary>
        /// <value>
        /// The <see cref="string"/>.
        /// </value>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public Cell this[int row, int column]
        {
            get { return content[row, column]; }
            set { content[row, column] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="string"/> with the specified row.
        /// </summary>
        /// <value>
        /// The <see cref="string"/>.
        /// </value>
        /// <param name="row">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public Cell this[int row, string columnName]
        {
            get
            {
                return content[row, GetColumnIndex(columnName)];
            }
            set
            {
                content[row, GetColumnIndex(columnName)] = value;
            }
        }

        /// <summary>
        /// Columns the exists.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public bool ColumnExists(string columnName)
        {
            if (columnName == "<" || columnName == ">")
                return true;

            for (int i = 1; i < content.GetLength(1) - 1; i++)
            {
                if (string.Equals(content[0, i].Text, columnName, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value or null.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public Cell GetValueOrNull(int row, string columnName)
        {
            if (!ColumnExists(columnName))
                return null;
            return this[row, GetColumnIndex(columnName)];
        }

        private int GetColumnIndex(string columnName)
        {
            if (columnName == "<")
                return 0;

            if (columnName == ">")
                return content.GetLength(1) - 1;

            for (int i = 1; i < content.GetLength(1) - 1; i++)
            {
                if (string.Equals(content[0, i].Text, columnName, StringComparison.CurrentCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException(string.Format("Column '{0}' not found", columnName), nameof(columnName));
        }

        /// <summary>
        /// Determines whether this table contains the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the table contains the specified line; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(PdfSubpath.Line line, float tolerance)
        {
            return
                TopLeftPoint.Y - tolerance <= line.From.Y &&
                line.To.Y <= BottomRightPoint.Y + tolerance
                &&
                TopLeftPoint.X - tolerance <= line.From.X &&
                line.To.X <= BottomRightPoint.X + tolerance;
        }

        /// <summary>
        /// Determines whether this instance contains the y coordinate (horizontal line at y coordinate).
        /// </summary>
        /// <param name="y">The y coordinate.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the table contains the specified coordinate; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(double y, float tolerance)
        {
            return
                TopLeftPoint.Y - tolerance <= y &&
                y <= BottomRightPoint.Y + tolerance;
        }

        /// <summary>
        /// Determines whether this instance contains the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the table contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(PdfPoint point, float tolerance)
        {
            return
                TopLeftPoint.Y - tolerance <= point.Y &&
                point.Y <= BottomRightPoint.Y - tolerance
                &&
                TopLeftPoint.X - tolerance <= point.X &&
                point.X <= BottomRightPoint.X - tolerance;
        }

        internal void CreateContent()
        {
            content = new Cell[Rows.Count, Columns.Count + 2];
        }

        /// <summary>
        /// Adds the text at the specified position
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="InvalidOperationException">Content is not initialized. Please call CreateContent first
        /// or
        /// The point is outside the table</exception>
        public void AddText(PdfPoint point, float tolerance, string content)
        {
            if (this.content == null)
                throw new InvalidOperationException("Content is not initialized. Please call CreateContent first");

            // The text can be also on the left or on the right of the table
            Row row = FindRow(point.Y, tolerance);
            if (row == null)
                throw new InvalidOperationException("The point is outside the table");

            int columnIndex = FindColumnIndex(point.X);
            int rowIndex = Rows.Count - row.Index - 1;

            if (string.IsNullOrEmpty(this.content[rowIndex, columnIndex].Text))
                this.content[rowIndex, columnIndex].Text = content;
            else if (this.content[rowIndex, columnIndex].Text.EndsWith(" "))
                this.content[rowIndex, columnIndex].Text += content;
            else
                this.content[rowIndex, columnIndex].Text += " " + content;
        }

        /// <summary>
        /// Finds the index of the column of the x coordinate.
        /// If x is on the left of the table, 0 is returned
        /// If x is on the right of the table, Count is returned
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <returns>The column</returns>
        private int FindColumnIndex(double x)
        {
            if (x < TopLeftPoint.X)
                return 0;

            if (BottomRightPoint.X < x)
                return Columns.Count + 1;

            Column column = Columns.SingleOrDefault(_ => _.BeginX <= x && x <= _.EndX) ?? Columns.OrderBy(_ => _.Index).Last(_ => x <= _.EndX);

            return column.Index + 1;
        }

        /// <summary>
        /// Finds the row corresponding to the y coordinate.
        /// Null if y is outside the table.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// The row or null if y is outside the table
        /// </returns>
        private Row FindRow(double y, float tolerance)
        {
            return Rows.Find(_ => _.BeginY <= y && y <= _.EndY) ?? Rows.Find(_ => _.BeginY - tolerance <= y && y <= _.EndY + tolerance);
        }

        double IPageContent.Y { get { return TopLeftPoint.Y; } }

        #region IFormattable
        private string DebuggerDisplay { get { return ToString("d"); } }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
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
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        /// <exception cref="FormatException"></exception>
        public string ToString(string format)
        {
            switch (format)
            {
                case "s":
                case "":
                case null:
                    if (this.content == null)
                        return "";
                    string toString = "";
                    for (int i = 0; i < this.content.GetLength(0); i++)
                    {
                        for (int j = 0; j < this.content.GetLength(1); j++)
                        {
                            if (j == 0)
                                toString += this.content[i, j];
                            else
                                toString += " | " + this.content[i, j];
                        }
                        toString += "\r\n";
                    }
                    return toString;
                case "d":
                    return string.Format("{0} - {1}; Rows = {2}, Columns = {3}", TopLeftPoint, BottomRightPoint, Rows.Count, Columns.Count);
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
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format);
        }
        #endregion

        /// <summary>
        /// Table cell
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// .
            /// </summary>
            public string Text { get; set; }
        }

        /// <summary>
        /// Table row
        /// </summary>
        public class Row
        {
            /// <summary>
            /// Gets or sets the topmost y coordinate of this row.
            /// </summary>
            /// <value>
            /// The topmost y of this row.
            /// </value>
            public double BeginY { get; set; }
            /// <summary>
            /// Gets or sets the bottommost y coordinate of this row.
            /// </summary>
            /// <value>
            /// The bottommost y coordinate of this row.
            /// </value>
            public double EndY { get; set; }
            /// <summary>
            /// Gets or sets the index of the row
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index { get; set; }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("Index: {0}, {1}-{2}", Index, BeginY, EndY);
            }
        }

        /// <summary>
        /// Table column
        /// </summary>
        public class Column
        {
            /// <summary>
            /// Gets or sets the leftmost X coordinate of the column.
            /// </summary>
            /// <value>
            /// The begin x.
            /// </value>
            public double BeginX { get; set; }

            /// <summary>
            /// Gets or sets the rightmost X coordinate of the column.
            /// </summary>
            /// <value>
            /// The end x.
            /// </value>
            public double EndX { get; set; }

            /// <summary>
            /// Gets or sets the index.
            /// </summary>
            /// <value>
            /// The index.
            /// </value>
            public int Index { get; set; }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("Index: {0}, {1}-{2}", Index, BeginX, EndX);
            }
        }
    }
}