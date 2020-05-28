namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
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
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Index: {0}, {1}-{2}", Index, BeginX, EndX);
        }

    }
}