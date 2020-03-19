namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Fonts.Type1.Parser;
    using UglyToad.PdfPig.Graphics.Colors;

    // https://www.sarasoueidan.com/blog/svg-coordinate-systems/

    /// <summary>
    /// 
    /// </summary>
    public class SvgTextExporter : ITextExporter
    {
        static readonly int rounding = 4;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string Get(Page page)
        {
            var builder = new StringBuilder($"<svg width='{page.Width}' height='{page.Height}'><g transform=\"scale(1, 1) translate(0, 0)\">");

            var paths = page.ExperimentalAccess.Paths;
            foreach (var path in paths)
            {
                if (path.IsClipping)
                {
                    var svg = PathToSvg(path, page.Height);
                    svg = svg.Replace("stroke='black'", "stroke='yellow'");
                    builder.Append(svg);
                }
                else
                {
                    builder.Append(PathToSvg(path, page.Height));
                }
            }

            foreach (var letter in page.Letters)
            {
                builder.Append(LetterToSvg(letter, page.Height));
            }

            builder.Append("</g></svg>");
            return builder.ToString();
        }

        static readonly Dictionary<string, string> _fonts = new Dictionary<string, string>()
        {
            { "ArialMT", "Arial Rounded MT Bold" }
        };

        private static string LetterToSvg(Letter l, double height)
        {
            /*string BboxToRect(PdfRectangle box, TextDirection textDirection, string fill, string stroke)
            {
                var x = box.Left;
                var y = height - box.Bottom - box.Height;

                var overallBbox = $"<rect x='{x}' y='{y}' width='{box.Width}' height='{box.Height}' stroke-width='1' fill='{fill}' stroke='{stroke}'>";

                if (textDirection != TextDirection.Horizontal)
                {
                    overallBbox += $"<g transform='rotate({Math.Round(box.Rotation, 0)} {x + box.Width / 2.0},{y + box.Height / 2.0})'>";
                }

                overallBbox += "</rect>";
                return overallBbox;
            }

            var glyphColor = ColorToSvg(l.Color);

            var bbox = BboxToRect(l.GlyphRectangle, l.TextDirection, glyphColor, glyphColor);*/

            string fontFamily = GetFontFamily(l.FontName, out string style, out string weight);
            string rotation = "";
            if (l.GlyphRectangle.Rotation != 0)
            {
                rotation = $" transform='rotate({Math.Round(-l.GlyphRectangle.Rotation, rounding)} {Math.Round(l.GlyphRectangle.BottomLeft.X, rounding)},{Math.Round(height - l.GlyphRectangle.TopLeft.Y, rounding)})'";
            }

            string fontSize = l.FontSize != 1 ? $"font-size='{l.FontSize.ToString("0")}'" : $"style='font-size:{Math.Round(l.GlyphRectangle.Height, 2)}px'";

            return $"<text x='{Math.Round(l.StartBaseLine.X, rounding)}' y='{Math.Round(height - l.StartBaseLine.Y, rounding)}'{rotation} font-family='{fontFamily}' font-style='{style}' font-weight='{weight}' {fontSize} fill='{ColorToSvg(l.Color)}'>{l.Value}</text>";
        }

        private static string GetFontFamily(string fontName, out string style, out string weight)
        {
            style = "normal";   // normal | italic | oblique
            weight = "normal";  // normal | bold | bolder | lighter

            // remove subset prefix
            if (fontName.Contains('+'))
            {
                if (fontName.Length > 7 && fontName[6] == '+')
                {
                    var split = fontName.Split('+');
                    if (split[0].All(c => char.IsUpper(c)))
                    {
                        fontName = split[1];
                    }
                }
            }

            // 
            if (fontName.Contains('-'))
            {
                var infos = fontName.Split('-');
                fontName = infos[0];

                for (int i = 1; i < infos.Length; i++)
                {
                    string infoLower = infos[i].ToLowerInvariant();
                    if (infoLower.Contains("light"))
                    {
                        weight = "lighter";
                    }
                    else if (infoLower.Contains("bolder"))
                    {
                        weight = "bolder";
                    }
                    else if (infoLower.Contains("bold"))
                    {
                        weight = "bold";
                    }

                    if (infoLower.Contains("italic"))
                    {
                        style = "italic";
                    }
                    else if (infoLower.Contains("oblique"))
                    {
                        style = "oblique";
                    }
                }
            }
            
            if (_fonts.ContainsKey(fontName)) fontName = _fonts[fontName];
            return fontName;
        }

        private static string ColorToSvg(IColor color)
        {
            var (r, g, b) = color.ToRGBValues();
            return $"rgb({Math.Round(r * 255, 0)},{Math.Round(g * 255, 0)},{Math.Round(b * 255, 0)})";
        }

        private static string PathToSvg(PdfPath p, double height)
        {
            var builder = new StringBuilder();
            foreach (var pathCommand in p.Commands)
            {
                //Console.WriteLine(pathCommand.GetType().ToString());
                pathCommand.WriteSvg(builder, height);
            }
            //Console.WriteLine();

            if (builder.Length == 0)
            {
                return string.Empty;
            }

            if (builder[builder.Length - 1] == ' ')
            {
                builder.Remove(builder.Length - 1, 1);
            }

            var glyph = builder.ToString();

            var path = $"<path d='{glyph}' stroke='black' stroke-width='1' fill='none'></path>";
            return path;
        }
    }
}
