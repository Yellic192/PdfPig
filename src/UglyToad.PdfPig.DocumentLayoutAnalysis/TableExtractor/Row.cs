namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
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
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Index: {0}, {1}-{2}", Index, BeginY, EndY);
        }
    }
}