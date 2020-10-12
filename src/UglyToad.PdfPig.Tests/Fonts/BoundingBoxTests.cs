namespace UglyToad.PdfPig.Tests.Fonts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    public class BoundingBoxTests
    {
        [Fact]
        public void Type0CidFontZeroHeightBBox()
        {
            using (var document = PdfDocument.Open(Integration.IntegrationHelpers.GetDocumentPath("68-1990-01_A")))
            {
                var page = document.GetPage(2);

                foreach (var letter in page.Letters)
                {
                    if (letter.Value == " ") continue;
                    Assert.NotEqual(0, letter.GlyphRectangle.Height);
                }
            }
        }

        [Fact]
        public void Type1FontSimpleZeroHeightBBox()
        {
            using (var document = PdfDocument.Open(@"D:\MachineLearning\Document Layout Analysis\text samples\3a9202f9f176d3377516e3da0866cc19148c033b.pdf"))
            {
                var page = document.GetPage(1);

                foreach (var letter in page.Letters)
                {
                    if (letter.Value == " ") continue;
                    Assert.NotEqual(0, letter.GlyphRectangle.Height);
                }
            }
        }

        [Fact]
        public void Type3FontZeroHeightBBox()
        {
            using (var document = PdfDocument.Open(@"D:\MachineLearning\Document Layout Analysis\text samples\CCM2UsersGuide.pdf"))
            {
                var page = document.GetPage(1);

                foreach (var letter in page.Letters)
                {
                    if (letter.Value == " ") continue;
                    Assert.True(letter.GlyphRectangle.Height > 0);
                }
            }
        }

        [Fact]
        public void Type3FontZeroHeightBBox2()
        {
            using (var document = PdfDocument.Open(@"D:\MachineLearning\Document Layout Analysis\text samples\spru054.pdf"))
            {
                var page = document.GetPage(1);

                foreach (var letter in page.Letters)
                {
                    if (letter.Value == " ") continue;
                    Assert.True(letter.GlyphRectangle.Height > 0);
                }
            }
        }

        [Fact]
        public void Type3FontZeroHeightBBox3()
        {
            using (var document = PdfDocument.Open(@"D:\MachineLearning\Document Layout Analysis\text samples\Type3_Font_issue.pdf"))
            {
                var page = document.GetPage(1);

                foreach (var letter in page.Letters)
                {
                    if (letter.Value == " ") continue;
                    Assert.True(letter.GlyphRectangle.Height > 0);
                }
            }
        }

        //Typesetting CJK using LaTEX
    }
}
