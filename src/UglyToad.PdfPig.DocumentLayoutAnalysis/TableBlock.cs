namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// 
    /// </summary>
    public class TableBlock : ContentBlock
    {
        /// <summary>
        /// 
        /// </summary>
        public TableCell[] Cells { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cells"></param>
        public TableBlock(IEnumerable<TableCell> cells)
        {
            Cells = cells.ToArray();
            BoundingBox = new PdfRectangle(cells.Min(x => x.BoundingBox.Left),
                                           cells.Min(x => x.BoundingBox.Bottom),
                                           cells.Max(x => x.BoundingBox.Right),
                                           cells.Max(x => x.BoundingBox.Top));
        }

        /// <summary>
        /// 
        /// </summary>
        public class TableCell : ContentBlock
        {
            /// <summary>
            /// 
            /// </summary>
            public TableCellType Type { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cell"></param>
            /// <param name="content"></param>
            public TableCell(PdfRectangle cell, TextBlock content)
            {
                BoundingBox = cell;
                Blocks = new[] { content };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (Blocks != null && Blocks.Count > 0 && Blocks.First() is TextBlock block)
                {
                    return block.Text + " " + BoundingBox.ToString();
                }
                return BoundingBox.ToString();
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
                /// defines the columns’ data. In some cases, header does not have to be in the top-most rows, however, 
                /// it still defines and categorizes columns’ data bellow it (e.g. in multi-tables).
                /// </summary>
                Header,

                /// <summary>
                /// Sub-header or super-rowcreates an additional dimension of the table and additionally, describes table 
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
}
