namespace UglyToad.PdfPig.Graphics.Colors
{
    using UglyToad.PdfPig.Tokens;

    /// <summary>
    /// TODO
    /// </summary>
    public class Pattern
    {
        /// <summary>
        /// 1 for tilling, 2 for shading.
        /// </summary>
        public int PatternType { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ArrayToken Matrix { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public Shading Shading { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public DictionaryToken ExtGState { get; }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="patternType"></param>
        /// <param name="matrix"></param>
        /// <param name="shading"></param>
        /// <param name="extGState"></param>
        public Pattern(int patternType, ArrayToken matrix, Shading shading, DictionaryToken extGState)
        {
            PatternType = patternType;
            Matrix = matrix;
            Shading = shading;
            ExtGState = extGState;
        }
    }
}
