namespace UglyToad.PdfPig.Tests.Integration
{
    using Xunit;

    public class FontPathsTests
    {
        [Fact]
        public void CanGetAllFontPaths()
        {
            var path = IntegrationHelpers.GetDocumentPath("Layer pdf - 322_High_Holborn_building_Brochure.pdf");

            using (var document = PdfDocument.Open(path))
            {
                foreach (var page in document.GetPages())
                {
                    foreach (var letter in page.Letters)
                    {
                        var rect = letter.GlyphRectangle;
                    }
                }
            }
        }
    }
}
