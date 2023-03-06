namespace UglyToad.PdfPig.Graphics.Colors
{
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Functions;
    using UglyToad.PdfPig.Tokens;

    /// <summary>
    /// TODO
    /// </summary>
    public class Shading
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int ShadingType { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool AntiAlias { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public DictionaryToken ShadingDictionary { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ColorSpaceDetails ColorSpace { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public PdfFunction Function { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ArrayToken Coords { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ArrayToken Domain { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ArrayToken Extend { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public PdfRectangle? BBox { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ArrayToken Background { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public Shading(int shadingType, bool antiAlias, DictionaryToken shadingDictionary,
            ColorSpaceDetails colorSpace, PdfFunction function, ArrayToken coords,
             ArrayToken domain, ArrayToken extend, PdfRectangle? bbox, ArrayToken background)
        {
            ShadingType = shadingType;
            AntiAlias = antiAlias;
            ShadingDictionary = shadingDictionary;
            ColorSpace = colorSpace;
            Function = function;
            Coords = coords;
            Domain = domain;
            Extend = extend;
            BBox = bbox;
            Background = background;
        }
    }
}
