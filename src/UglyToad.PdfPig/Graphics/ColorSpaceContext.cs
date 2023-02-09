namespace UglyToad.PdfPig.Graphics
{
    using System;
    using System.Collections.Generic;
    using Colors;
    using Content;
    using Tokens;

    internal class ColorSpaceContext : IColorSpaceContext
    {
        public IColorSpaceContext DeepClone()
        {
            return new ColorSpaceContext(currentStateFunc, resourceStore)
            {
                CurrentStrokingColorSpaceDetails = CurrentStrokingColorSpaceDetails,
                CurrentNonStrokingColorSpaceDetails = CurrentNonStrokingColorSpaceDetails
            };
        }

        private readonly Func<CurrentGraphicsState> currentStateFunc;
        private readonly IResourceStore resourceStore;

        /// <summary>
        /// The <see cref="ColorSpaceDetails"/> used for stroking operations.
        /// </summary>
        public ColorSpaceDetails CurrentStrokingColorSpaceDetails { get; private set; } = DeviceGrayColorSpaceDetails.Instance;

        /// <summary>
        /// The <see cref="ColorSpaceDetails"/> used for non-stroking operations.
        /// </summary>
        public ColorSpaceDetails CurrentNonStrokingColorSpaceDetails { get; private set; } = DeviceGrayColorSpaceDetails.Instance;

        //public ColorSpace CurrentStrokingColorSpace { get; private set; } = ColorSpace.DeviceGray;

        //public ColorSpace CurrentNonStrokingColorSpace { get; private set; } = ColorSpace.DeviceGray;

        public NameToken AdvancedStrokingColorSpace { get; private set; }

        public NameToken AdvancedNonStrokingColorSpace { get; private set; }

        public ColorSpaceContext(Func<CurrentGraphicsState> currentStateFunc, IResourceStore resourceStore)
        {
            this.currentStateFunc = currentStateFunc ?? throw new ArgumentNullException(nameof(currentStateFunc));
            this.resourceStore = resourceStore ?? throw new ArgumentNullException(nameof(resourceStore));
        }

        public void SetStrokingColorspace(NameToken colorspace)
        {
            void DefaultColorSpace(ColorSpace? colorSpaceActual = null)
            {
                if (colorSpaceActual.HasValue)
                {
                    switch (colorSpaceActual)
                    {
                        case ColorSpace.DeviceGray:
                            currentStateFunc().CurrentStrokingColor = GrayColor.Black;
                            break;
                        case ColorSpace.DeviceRGB:
                            currentStateFunc().CurrentStrokingColor = RGBColor.Black;
                            break;
                        case ColorSpace.DeviceCMYK:
                            currentStateFunc().CurrentStrokingColor = CMYKColor.Black;
                            break;
                        default:
                            currentStateFunc().CurrentStrokingColor = GrayColor.Black;
                            break;
                    }
                }
                else
                {
                    CurrentNonStrokingColorSpaceDetails = DeviceGrayColorSpaceDetails.Instance;
                    //CurrentStrokingColorSpace = ColorSpace.DeviceGray;
                    currentStateFunc().CurrentStrokingColor = GrayColor.Black;
                }
            }

            AdvancedStrokingColorSpace = null;
            CurrentStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(colorspace, null);

            /*
            if (colorspace.TryMapToColorSpace(out var colorspaceActual))
            {
                CurrentStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(colorspaceActual,
                    new DictionaryToken(new Dictionary<NameToken, IToken>()));
                //CurrentStrokingColorSpace = colorspaceActual;
                return;
            }
            else if (resourceStore.TryGetNamedColorSpace(colorspace, out var namedColorSpace))
            {
                if (namedColorSpace.Name.TryMapToColorSpace(out var mapped))
                {
                    if (namedColorSpace.Data is ArrayToken separationArray)
                    {
                        var pseudoDictionary = new DictionaryToken(
                            new Dictionary<NameToken, IToken>
                            {
                                { NameToken.ColorSpace, separationArray }
                            });
                        CurrentStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(mapped, pseudoDictionary);
                        DefaultColorSpace(CurrentStrokingColorSpaceDetails.BaseType); // TODO - is it needed??

                        // TODO - to remove
                        //AdvancedStrokingColorSpace = namedColorSpace.Name;
                        //CurrentStrokingColorSpace = CurrentStrokingColorSpaceDetails.BaseType;
                        //DefaultColorSpace(CurrentStrokingColorSpace);
                        // End TODO

                        return;
                    }
                }
            }
            */

            DefaultColorSpace(CurrentNonStrokingColorSpaceDetails.BaseType);
        }

        public void SetNonStrokingColorspace(NameToken colorspace)
        {
            void DefaultColorSpace(ColorSpace? colorSpaceActual = null)
            {
                if (colorSpaceActual.HasValue)
                {
                    switch (colorSpaceActual)
                    {
                        case ColorSpace.DeviceGray:
                            currentStateFunc().CurrentNonStrokingColor = GrayColor.Black;
                            break;
                        case ColorSpace.DeviceRGB:
                            currentStateFunc().CurrentNonStrokingColor = RGBColor.Black;
                            break;
                        case ColorSpace.DeviceCMYK:
                            currentStateFunc().CurrentNonStrokingColor = CMYKColor.Black;
                            break;
                        default:
                            currentStateFunc().CurrentNonStrokingColor = GrayColor.Black;
                            break;
                    }
                }
                else
                {
                    CurrentNonStrokingColorSpaceDetails = DeviceGrayColorSpaceDetails.Instance;
                    //CurrentNonStrokingColorSpace = ColorSpace.DeviceGray;
                    currentStateFunc().CurrentNonStrokingColor = GrayColor.Black;
                }
            }

            AdvancedNonStrokingColorSpace = null;
            CurrentNonStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(colorspace, null);

            /*
            if (colorspace.TryMapToColorSpace(out var colorspaceActual))
            {
                CurrentNonStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(colorspaceActual,
                    new DictionaryToken(new Dictionary<NameToken, IToken>()));
                //CurrentNonStrokingColorSpace = colorspaceActual;
                return;
            }
            else if (resourceStore.TryGetNamedColorSpace(colorspace, out var namedColorSpace))
            {
                if (namedColorSpace.Name.TryMapToColorSpace(out var mapped))
                {
                    if (namedColorSpace.Data is ArrayToken separationArray)
                    {
                        var pseudoDictionary = new DictionaryToken(
                          new Dictionary<NameToken, IToken>
                          {
                          { NameToken.ColorSpace, separationArray }
                          });
                        CurrentNonStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(mapped, pseudoDictionary);

                        // TODO - to remove
                        //AdvancedNonStrokingColorSpace = namedColorSpace.Name;
                        //CurrentNonStrokingColorSpace = CurrentNonStrokingColorSpaceDetails.BaseType;
                        DefaultColorSpace(CurrentNonStrokingColorSpaceDetails.BaseType);
                        // End TODO

                        return;
                    }
                }
            }
            */
            DefaultColorSpace(CurrentNonStrokingColorSpaceDetails.BaseType);
        }

        public void SetStrokingColor(IReadOnlyList<decimal> operands)
        {
            currentStateFunc().CurrentStrokingColor = CurrentStrokingColorSpaceDetails.GetColor(operands);
        }

        public void SetStrokingColorGray(decimal gray)
        {
            //CurrentStrokingColorSpace = ColorSpace.DeviceGray;
            CurrentStrokingColorSpaceDetails = DeviceGrayColorSpaceDetails.Instance;

            if (gray == 0)
            {
                currentStateFunc().CurrentStrokingColor = GrayColor.Black;
            }
            else if (gray == 1)
            {
                currentStateFunc().CurrentStrokingColor = GrayColor.White;
            }
            else
            {
                currentStateFunc().CurrentStrokingColor = new GrayColor(gray);
            }
        }

        public void SetStrokingColorRgb(decimal r, decimal g, decimal b)
        {
            //CurrentStrokingColorSpace = ColorSpace.DeviceRGB;
            CurrentStrokingColorSpaceDetails = DeviceRgbColorSpaceDetails.Instance;

            if (r == 0 && g == 0 && b == 0)
            {
                currentStateFunc().CurrentStrokingColor = RGBColor.Black;
            }
            else if (r == 1 && g == 1 && b == 1)
            {
                currentStateFunc().CurrentStrokingColor = RGBColor.White;
            }
            else
            {
                currentStateFunc().CurrentStrokingColor = new RGBColor(r, g, b);
            }
        }

        public void SetStrokingColorCmyk(decimal c, decimal m, decimal y, decimal k)
        {
            //CurrentStrokingColorSpace = ColorSpace.DeviceCMYK;
            CurrentStrokingColorSpaceDetails = DeviceCmykColorSpaceDetails.Instance;

            currentStateFunc().CurrentStrokingColor = new CMYKColor(c, m, y, k);
        }

        public void SetNonStrokingColor(IReadOnlyList<decimal> operands)
        {
            currentStateFunc().CurrentNonStrokingColor = CurrentNonStrokingColorSpaceDetails.GetColor(operands);
        }

        public void SetNonStrokingColorGray(decimal gray)
        {
            //CurrentNonStrokingColorSpace = ColorSpace.DeviceGray;
            CurrentNonStrokingColorSpaceDetails = DeviceGrayColorSpaceDetails.Instance;

            if (gray == 0)
            {
                currentStateFunc().CurrentNonStrokingColor = GrayColor.Black;
            }
            else if (gray == 1)
            {
                currentStateFunc().CurrentNonStrokingColor = GrayColor.White;
            }
            else
            {
                currentStateFunc().CurrentNonStrokingColor = new GrayColor(gray);
            }
        }

        public void SetNonStrokingColorRgb(decimal r, decimal g, decimal b)
        {
            //CurrentNonStrokingColorSpace = ColorSpace.DeviceRGB;
            CurrentNonStrokingColorSpaceDetails = DeviceRgbColorSpaceDetails.Instance;

            if (r == 0 && g == 0 && b == 0)
            {
                currentStateFunc().CurrentNonStrokingColor = RGBColor.Black;
            }
            else if (r == 1 && g == 1 && b == 1)
            {
                currentStateFunc().CurrentNonStrokingColor = RGBColor.White;
            }
            else
            {
                currentStateFunc().CurrentNonStrokingColor = new RGBColor(r, g, b);
            }
        }

        public void SetNonStrokingColorCmyk(decimal c, decimal m, decimal y, decimal k)
        {
            //CurrentNonStrokingColorSpace = ColorSpace.DeviceCMYK;
            CurrentNonStrokingColorSpaceDetails = DeviceCmykColorSpaceDetails.Instance;

            currentStateFunc().CurrentNonStrokingColor = new CMYKColor(c, m, y, k);
        }
    }
}
