namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// 
    /// </summary>
    public class TableBlock : BaseBlock
    {
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyList<TableCell> Cells { get; }

        /// <summary>
        /// Gets the number of rows in the table.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Gets the number of columns in the table.
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// From left to right and top to bottom.
        /// </summary>
        /// <param name="r">The row index, starting at 0.</param>
        /// <param name="c">The column index, starting at 0.</param>
        public TableCell this[int r, int c]
        {
            get
            {
                if (r >= Rows || c >= Columns)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var candidates = Cells.Where(cell => cell.RowSpan.Contains(r) && cell.ColumnSpan.Contains(c));
                if (candidates.Count() > 1)
                {
                    throw new ArgumentException();
                }
                return candidates.FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cells"></param>
        public TableBlock(IEnumerable<TableCell> cells)
        {
            Cells = cells.ToList();
            BoundingBox = new PdfRectangle(cells.Min(c => c.BoundingBox.BottomLeft.X), cells.Min(c => c.BoundingBox.BottomLeft.Y),
                                           cells.Max(c => c.BoundingBox.TopRight.X), cells.Max(c => c.BoundingBox.TopRight.Y));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TableCell : BaseBlock
    {
        /// <summary>
        /// 
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 
        /// </summary>
        public TableCellType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsMerged => RowSpan.Length > 0 || ColumnSpan.Length > 0;

        /// <summary>
        /// 
        /// </summary>
        public int[] RowSpan { get; }

        /// <summary>
        /// 
        /// </summary>
        public int[] ColumnSpan { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="content"></param>
        /// <param name="index"></param>
        public TableCell(PdfRectangle boundingBox, TextBlock content, int index) // int[] rowSpan, int[] columnSpan)
        {
            BoundingBox = boundingBox;
            Children = new[] { content };
            Index = index;
            //RowSpan = rowSpan;
            //ColumnSpan = columnSpan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TableCell #" + Index;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum TableCellType
        {
            /// <summary>
            /// 
            /// </summary>
            Unknown,

            /// <summary>
            /// Header (column header) is usually top-most row (or set of multiple top-most rows) of a table and 
            /// defines the columns’ data. In some cases, header does not have to be in the top-most rows, however, 
            /// it still defines and categorizes columns’ data bellow it (e.g. in multi-tables).
            /// </summary>
            Header,

            /// <summary>
            /// Sub-header or super-row creates an additional dimension of the table and additionally, describes table 
            /// data. The sub-header row is usually placed between data rows, separating them by some dimension or 
            /// concept.
            /// </summary>
            SuperRow,

            /// <summary>
            /// The stub (row header) is typically the left-most column of the table, usually containing the list 
            /// of subjects or instances to which the values in the table body apply.
            /// </summary>
            Stub,

            /// <summary>
            /// Table body (data cells) contains the table’s data. Data cells are placed in the body of the table.
            /// Cells in the body represent the value of things (variables) orthe value of relationship defined in 
            /// headers, sub-headers and stub.
            /// </summary>
            Body
        }
    }
}
