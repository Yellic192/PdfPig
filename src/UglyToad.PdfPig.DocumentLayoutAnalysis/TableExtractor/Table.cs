namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Table
    /// </summary>
    /// <seealso cref="UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor.IPageContent" />
    /// <seealso cref="System.IFormattable" />
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class Table : IPageContent, IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        public Table()
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
        public Point TopLeftPoint { get; set; }
        /// <summary>
        /// Gets or sets the bottom right point.
        /// </summary>
        /// <value>
        /// The bottom right point.
        /// </value>
        public Point BottomRightPoint { get; set; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public List<Row> Rows { get; private set; }
        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public List<Column> Columns { get; private set; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public double Width { get { return BottomRightPoint.X - TopLeftPoint.X; } }
        /// <summary>
        /// Gets the heigth.
        /// </summary>
        /// <value>
        /// The heigth.
        /// </value>
        public double Heigth { get { return BottomRightPoint.Y - TopLeftPoint.Y; } }

        private string[,] content;

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified row.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public string this[int row, int column]
        {
            get { return content[row, column]; }
            set { content[row, column] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified row.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="row">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string this[int row, string columnName]
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
                if (String.Equals(content[0, i].Trim(), columnName, StringComparison.CurrentCultureIgnoreCase))
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
        public string GetValueOrNull(int row, string columnName)
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
                if (String.Equals(content[0, i].Trim(), columnName, StringComparison.CurrentCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException(string.Format("Column '{0}' not found", columnName), "columnName");

        }


        /// <summary>
        /// Determines whether this table contains the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>
        ///   <c>true</c> if the table contains the specified line; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Line line)
        {
            return
                TopLeftPoint.Y - ContentExtractor.Tolerance <= line.StartPoint.Y &&
                line.EndPoint.Y <= BottomRightPoint.Y + ContentExtractor.Tolerance
                &&
                TopLeftPoint.X - ContentExtractor.Tolerance <= line.StartPoint.X &&
                line.EndPoint.X <= BottomRightPoint.X + ContentExtractor.Tolerance;
        }

        /// <summary>
        /// Determines whether this instance contains the y coordinate (horizontal line at y coordinate).
        /// </summary>
        /// <param name="y">The y coordinate.</param>
        /// <returns>
        ///   <c>true</c> if the table contains the specified coordinate; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(double y)
        {
            return
                TopLeftPoint.Y - ContentExtractor.Tolerance <= y &&
                y <= BottomRightPoint.Y + ContentExtractor.Tolerance;
        }

        /// <summary>
        /// Determines whether this instance contains the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///   <c>true</c> if the table contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Point point)
        {
            return
                TopLeftPoint.Y - ContentExtractor.Tolerance <= point.Y &&
                point.Y <= BottomRightPoint.Y - ContentExtractor.Tolerance
                &&
                TopLeftPoint.X - ContentExtractor.Tolerance <= point.X &&
                point.X <= BottomRightPoint.X - ContentExtractor.Tolerance;
        }

        internal void CreateContent()
        {
            content = new string[Rows.Count, Columns.Count + 2];
        }

        /// <summary>
        /// Adds the text at the specified position
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="InvalidOperationException">
        /// Content is not initialized. Please call CreateContent first
        /// or
        /// The point is outside the table
        /// </exception>
        public void AddText(Point point, string content)
        {
            if (this.content == null)
                throw new InvalidOperationException("Content is not initialized. Please call CreateContent first");

            // The text can be also on the left or on the right of the table
            Row row = FindRow(point.Y);
            if (row == null)
                throw new InvalidOperationException("The point is outside the table");

            int columnIndex = FindColumnIndex(point.X);
            int rowIndex = Rows.Count - row.Index - 1;

            if (string.IsNullOrEmpty(this.content[rowIndex, columnIndex]))
                this.content[rowIndex, columnIndex] = content;
            else if (this.content[rowIndex, columnIndex].EndsWith(" "))
                this.content[rowIndex, columnIndex] += content;
            else
                this.content[rowIndex, columnIndex] += " " + content;
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

            Column column = Columns.SingleOrDefault(_ => _.BeginX <= x && x <= _.EndX);

            if (column == null)
                column = Columns.OrderBy(_ => _.Index).Last(_ => x <= _.EndX);

            return column.Index + 1;
        }

        /// <summary>
        /// Finds the row corresponding to the y coordinate.
        /// Null if y is outside the table.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <returns>The row or null if y is outside the table</returns>
        private Row FindRow(double y)
        {
            Row row = Rows.FirstOrDefault(_ => _.BeginY <= y && y <= _.EndY);
            if (row == null)
                row = Rows.FirstOrDefault(_ => _.BeginY - ContentExtractor.Tolerance <= y && y <= _.EndY + ContentExtractor.Tolerance);
            return row;
        }

        double IPageContent.Y { get { return TopLeftPoint.Y; } }

        #region IFormattable

        private string DebuggerDisplay { get { return ToString("d"); } }

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
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format);
        }


        #endregion

    }
}