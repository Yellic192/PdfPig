namespace UglyToad.PdfPig.Graphics.Colors
{
    using System;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Functions;
    using UglyToad.PdfPig.Tokens;

    // TODO - implement : IEquatable<PatternColor> and override Equals

    /// <summary>
    /// TODO
    /// </summary>
    public class Shading // : IEquatable<PatternColor>
    {
        /// <summary>
        /// TODO
        /// </summary>
        public ShadingTypes ShadingType { get; }

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
        public Shading(ShadingTypes shadingType, bool antiAlias, DictionaryToken shadingDictionary,
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

    /// <summary>
    /// SHading types.
    /// </summary>
    public enum ShadingTypes : byte
    {
        /// <summary>
        /// Function-based shading.
        /// </summary>
        FunctionBased = 1,

        /// <summary>
        /// Axial shading.
        /// </summary>
        Axial = 2,

        /// <summary>
        /// Radial shading.
        /// </summary>
        Radial = 3,

        /// <summary>
        /// Free-form Gouraud-shaded triangle mesh.
        /// </summary>
        FreeFormGouraud = 4,

        /// <summary>
        /// Lattice-form Gouraud-shaded triangle mesh.
        /// </summary>
        LatticeFormGouraud = 5,

        /// <summary>
        /// Coons patch mesh.
        /// </summary>
        CoonsPatch = 6,

        /// <summary>
        /// Tensor-product patch mesh
        /// </summary>
        TensorProductPatch = 7
    }
}
