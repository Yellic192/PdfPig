using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UglyToad.PdfPig.Tests.Integration
{
    public class WrongBbox1
    {
        private static string GetFilename()
        {
            return IntegrationHelpers.GetDocumentPath("data.pdf");
        }

        [Fact]
        public void LettersHaveCorrectBbox()
        {
            using (var document = PdfDocument.Open(GetFilename()))
            {
                var page = document.GetPage(1);


            }
        }
    }
}
