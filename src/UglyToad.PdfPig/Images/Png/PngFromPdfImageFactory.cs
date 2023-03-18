namespace UglyToad.PdfPig.Images.Png
{
    using Content;
    using Graphics.Colors;
    using UglyToad.PdfPig.Core;

    internal static class PngFromPdfImageFactory
    {
        public static bool TryGenerate(IPdfImage image, out byte[] bytes)
        {
            bytes = null;

            var hasValidDetails = image.ColorSpaceDetails != null &&
                                  !(image.ColorSpaceDetails is UnsupportedColorSpaceDetails);
            //var actualColorSpace = hasValidDetails ? image.ColorSpaceDetails.BaseType : image.ColorSpace;
            if (!hasValidDetails)
            {
                return false;
            }

            var actualColorSpace = image.ColorSpaceDetails;

            var isColorSpaceSupported =
                actualColorSpace.Type == ColorSpace.DeviceGray || actualColorSpace.Type == ColorSpace.DeviceRGB
                || actualColorSpace.Type == ColorSpace.DeviceCMYK || actualColorSpace.Type == ColorSpace.CalGray
                || actualColorSpace.Type == ColorSpace.CalRGB || actualColorSpace.Type == ColorSpace.DeviceN
                || actualColorSpace.Type == ColorSpace.Indexed || actualColorSpace.Type == ColorSpace.Separation
                || actualColorSpace.Type == ColorSpace.ICCBased;

            if (!isColorSpaceSupported || !image.TryGetBytes(out var bytesPure))
            {
                return false;
            }

            try
            {
                bytesPure = ColorSpaceDetailsByteConverter.Convert(image.ColorSpaceDetails, bytesPure,
                    image.BitsPerComponent, image.WidthInSamples, image.HeightInSamples);

                var numberOfComponents = actualColorSpace.GetNumberOfComponents();

                //var is3Byte = numberOfComponents == 3;
                const bool hasAlphaChannel = true; // TODO - why should that ever be false??
                var builder = PngBuilder.Create(image.WidthInSamples, image.HeightInSamples, hasAlphaChannel);

                var requiredSize = (image.WidthInSamples * image.HeightInSamples * numberOfComponents);

                var actualSize = bytesPure.Count;
                var isCorrectlySized = bytesPure.Count == requiredSize ||
                    // Spec, p. 37: "...error if the stream contains too much data, with the exception that
                    // there may be an extra end-of-line marker..."
                    (actualSize == requiredSize + 1 && bytesPure[actualSize - 1] == ReadHelper.AsciiLineFeed) ||
                    (actualSize == requiredSize + 1 && bytesPure[actualSize - 1] == ReadHelper.AsciiCarriageReturn) ||
                    // The combination of a CARRIAGE RETURN followed immediately by a LINE FEED is treated as one EOL marker.
                    (actualSize == requiredSize + 2 &&
                        bytesPure[actualSize - 2] == ReadHelper.AsciiCarriageReturn &&
                        bytesPure[actualSize - 1] == ReadHelper.AsciiLineFeed);

                if (!isCorrectlySized)
                {
                    return false;
                }

                var i = 0;
                // The below shoud be in the respective color space (and optimised!)
                for (var col = 0; col < image.HeightInSamples; col++)
                {
                    for (var row = 0; row < image.WidthInSamples; row++)
                    {
                        switch (numberOfComponents)
                        {
                            case 4:
                                var c = (bytesPure[i++] / 255d);
                                var m = (bytesPure[i++] / 255d);
                                var y = (bytesPure[i++] / 255d);
                                var k = (bytesPure[i++] / 255d);
                                var rgb = actualColorSpace.GetColor(new decimal[] { (decimal)c, (decimal)m, (decimal)y, (decimal)k }).ToRGBValues();
                                builder.SetPixel((byte)(rgb.r * 255), (byte)(rgb.g * 255), (byte)(rgb.b * 255), row, col);
                                break;

                            case 3:
                                var r = (bytesPure[i++] / 255d);
                                var g = (bytesPure[i++] / 255d);
                                var b = (bytesPure[i++] / 255d);
                                var rgb3 = actualColorSpace.GetColor(new decimal[] { (decimal)r, (decimal)g, (decimal)b }).ToRGBValues();
                                builder.SetPixel((byte)(rgb3.r * 255), (byte)(rgb3.g * 255), (byte)(rgb3.b * 255), row, col);
                                break;

                            case 1:
                                decimal g1 = 0;
                                if (actualColorSpace is IndexedColorSpaceDetails indexed)
                                {
                                    g1 = bytesPure[i++]; // hack
                                }
                                else
                                {
                                    g1 = bytesPure[i++] / 255m;
                                }

                                var rgb1 = actualColorSpace.GetColor(new decimal[] { g1 }).ToRGBValues();
                                builder.SetPixel((byte)(rgb1.r * 255), (byte)(rgb1.g * 255), (byte)(rgb1.b * 255), row, col);
                                break;

                            default:
                                // case n
                                decimal[] comps = new decimal[numberOfComponents];
                                for (int k1 = 0; k1 < numberOfComponents; k1++)
                                {
                                    comps[k1] = (bytesPure[i++] / 255m);
                                }
                                var rgbN = actualColorSpace.GetColor(comps).ToRGBValues();
                                builder.SetPixel((byte)(rgbN.r * 255), (byte)(rgbN.g * 255), (byte)(rgbN.b * 255), row, col);
                                break;
                        }

                        //if (actualColorSpace == ColorSpace.DeviceCMYK)
                        //{
                            
                        //     //Where CMYK in 0..1
                        //     //R = 255 × (1-C) × (1-K)
                        //     //G = 255 × (1-M) × (1-K)
                        //     //B = 255 × (1-Y) × (1-K)
                             

                        //    var c = (bytesPure[i++]/255d);
                        //    var m = (bytesPure[i++]/255d);
                        //    var y = (bytesPure[i++]/255d);
                        //    var k = (bytesPure[i++]/255d);
                        //    var r = (byte)(255 * (1 - c) * (1 - k));
                        //    var g = (byte)(255 * (1 - m) * (1 - k));
                        //    var b = (byte)(255 * (1 - y) * (1 - k));

                        //    builder.SetPixel(r, g, b, row, col);
                        //}
                        //else if (is3Byte)
                        //{
                        //    builder.SetPixel(bytesPure[i++], bytesPure[i++], bytesPure[i++], row, col);
                        //}
                        //else
                        //{
                        //    var pixel = bytesPure[i++];
                        //    builder.SetPixel(pixel, pixel, pixel, row, col);
                        //}
                    }
                }

                bytes = builder.Save();
                return true;
            }
            catch
            {
                // ignored.
            }

            return false;
        }
    }
}
