using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Core;

namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    /// <summary>
    /// 
    /// </summary>
    public class TableBlock
    {
        /// <summary>
        /// The rectangle completely containing the block.
        /// </summary>
        public PdfRectangle BoundingBox { get; }

        /// <summary>
        /// 
        /// </summary>
        public TableCell[] Cells { get; }

        /// <summary>
        /// 
        /// </summary>
        public PdfPoint[] GridPoints => Cells.SelectMany(c => new[] { c.Cell.BottomLeft, c.Cell.BottomRight, c.Cell.TopLeft, c.Cell.TopRight }).Distinct().ToArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cells"></param>
        public TableBlock(IEnumerable<TableCell> cells)
        {
            Cells = cells.ToArray();
            BoundingBox = new PdfRectangle(cells.Min(c => c.Cell.BottomLeft.X), cells.Min(c => c.Cell.BottomLeft.Y),
                                           cells.Max(c => c.Cell.TopRight.X), cells.Max(c => c.Cell.TopRight.Y));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TableCell
    {
        /// <summary>
        /// 
        /// </summary>
        public PdfRectangle Cell { get; }

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
        /// <param name="cell"></param>
        /// <param name="content"></param>
        public TableCell(PdfRectangle cell, TextBlock content)
        {
            Cell = cell;
            Content = content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Content == null ? "" : Content.Text.Trim() + " ") + Cell.ToString();
        }
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
