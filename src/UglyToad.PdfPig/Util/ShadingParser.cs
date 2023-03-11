namespace UglyToad.PdfPig.Util
{
    using System;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Filters;
    using UglyToad.PdfPig.Functions;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Tokenization.Scanner;
    using UglyToad.PdfPig.Tokens;

    internal static class ShadingParser
    {
        public static Shading Create(IToken shading, IPdfTokenScanner scanner, IResourceStore resourceStore, ILookupFilterProvider filterProvider)
        {
            DictionaryToken shadingDictionary = null;
            StreamToken shadingStream = null;

            if (shading is StreamToken fs)
            {
                shadingDictionary = fs.StreamDictionary;
                shadingStream = new StreamToken(fs.StreamDictionary, fs.Decode(filterProvider, scanner));
            }
            else if (shading is DictionaryToken fd)
            {
                shadingDictionary = fd;
            }

            ShadingTypes shadingType;
            if (shadingDictionary.TryGet<NumericToken>(NameToken.ShadingType, scanner, out var shadingTypeToken))
            {
                shadingType = (ShadingTypes)shadingTypeToken.Int;
            }
            else
            {
                throw new ArgumentException("ShadingType is required.");
            }

            ColorSpaceDetails colorSpaceDetails = null;
            if (shadingDictionary.TryGet<NameToken>(NameToken.ColorSpace, scanner, out var colorSpaceToken))
            {
                colorSpaceDetails = resourceStore.GetColorSpaceDetails(colorSpaceToken, shadingDictionary);
            }
            else if (shadingDictionary.TryGet<ArrayToken>(NameToken.ColorSpace, scanner, out var colorSpaceSToken))
            {
                var first = colorSpaceSToken.Data[0];
                if (first is NameToken firstColorSpaceName)
                {
                    colorSpaceDetails = resourceStore.GetColorSpaceDetails(firstColorSpaceName, shadingDictionary);
                }
                else
                {
                    throw new ArgumentException("ColorSpace is required.");
                }
            }
            else
            {
                throw new ArgumentException("ColorSpace is required.");
            }

            PdfFunction function = null;
            /*
             * In addition, some shading dictionaries also include a Function entry whose value shall be a
             * function object (dictionary or stream) defining how colours vary across the area to be shaded.
             * In such cases, the shading dictionary usually defines the geometry of the shading, and the
             * function defines the colour transitions across that geometry. The function is required for
             * some types of shading and optional for others.
             */
            if (shadingDictionary.TryGet<DictionaryToken>(NameToken.Function, scanner, out var functionToken))
            {
                function = PdfFunctionParser.Create(functionToken, scanner, filterProvider);
            }
            else if (shadingDictionary.TryGet<StreamToken>(NameToken.Function, scanner, out var functionStreamToken))
            {
                function = PdfFunctionParser.Create(functionStreamToken, scanner, filterProvider);
            }
            else
            {
                // 8.7.4.5.2 Type 1 (Function-Based) Shadings - Required
                // 8.7.4.5.3 Type 2 (Axial) Shadings - Required
                // 8.7.4.5.4 Type 3 (Radial) Shadings - Required
                // 8.7.4.5.5 Type 4 Shadings (Free-Form Gouraud-Shaded Triangle Meshes) - Optional
                // 8.7.4.5.6 Type 5 Shadings (Lattice-Form Gouraud-Shaded Triangle Meshes) - Optional
                // 8.7.4.5.7 Type 6 Shadings (Coons Patch Meshes) - Optional
                // 8.7.4.5.8 Type 7 Shadings (Tensor-Product Patch Meshes) - N/A

                if (shadingType == ShadingTypes.FunctionBased || shadingType == ShadingTypes.Axial || shadingType == ShadingTypes.Radial)
                {
                    throw new ArgumentNullException($"{NameToken.Function} is required for shading type '{shadingType}'.");
                }
            }

            if (!shadingDictionary.TryGet<ArrayToken>(NameToken.Background, scanner, out var backgroundToken))
            {
                // Optional
            }

            if (shadingDictionary.TryGet<ArrayToken>(NameToken.Bbox, scanner, out var bboxToken))
            {
                // TODO - check if array (sais it's 'rectangle')
                // Optional
            }

            if (!shadingDictionary.TryGet<BooleanToken>(NameToken.AntiAlias, scanner, out var antiAliasToken))
            {
                // Optional
                // Default value: false.
                antiAliasToken = BooleanToken.False;
            }

            if (!shadingDictionary.TryGet<ArrayToken>(NameToken.Coords, scanner, out var coordsToken))
            {

            }

            if (!shadingDictionary.TryGet<ArrayToken>(NameToken.Domain, scanner, out var domainToken))
            {
                // set default values
                domainToken = new ArrayToken(new IToken[] { new NumericToken(0), new NumericToken(1) });
            }

            if (!shadingDictionary.TryGet<ArrayToken>(NameToken.Extend, scanner, out var extendToken))
            {
                // set default values
                extendToken = new ArrayToken(new IToken[] { BooleanToken.False, BooleanToken.False });
            }

            return new Shading(shadingType, antiAliasToken.Data,
                shadingDictionary, colorSpaceDetails, function, coordsToken, domainToken,
                extendToken, bboxToken?.ToRectangle(scanner), backgroundToken);
        }
    }
}
