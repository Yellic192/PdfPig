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
        /// 
        /// </summary>
        public PdfPoint[] GridPoints => Cells.SelectMany(c => new[] 
        {
            c.BoundingBox.BottomLeft,
            c.BoundingBox.BottomRight,
            c.BoundingBox.TopLeft,
            c.BoundingBox.TopRight
        }).Distinct().ToArray();

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
        public TextBlock Content { get; }

        /// <summary>
        /// 
        /// </summary>
        public TableCellType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsMerged { get; }

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
        public TableCell(PdfRectangle boundingBox, TextBlock content)
        {
            BoundingBox = boundingBox;
            Content = content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Content == null ? "" : Content.Text.Trim() + " ") + BoundingBox.ToString();
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
            /// Header (column header)is usually top-most row (or set of multiple top-most rows) of a table and 
            /// defines the columns’ data. In some cases, headerdoes not have to be in the top-most rows, however, 
            /// it still defines and cate-gorizes columns’ data bellow it (e.g. in multi-tables).
            /// </summary>
            Header,

            /// <summary>
            /// Sub-headerorsuper-rowcreates an additional dimension of the table andadditionally, describes table 
            /// data. The sub-header row is usually placedbetween data rows, separating them by some dimension or 
            /// concept.
            /// </summary>
            SuperRow,

            /// <summary>
            /// The stub (row header) is typically the left-most column of the table, usu-ally containing the list 
            /// of subjects or instances to which the values in thetable body apply.
            /// </summary>
            Stub,

            /// <summary>
            /// Table body (data cells) contains the table’s data. Data cells are placed in thebody of the table.
            /// Cells in the body represent the value of things (variables) orthe value of relationship defined in 
            /// headers, sub-headers and stub.
            /// </summary>
            Body
        }
    }
}
