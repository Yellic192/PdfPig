using System;

namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using Core;

    static class PdfPigHelper
    {
        /// <summary>
        /// Returns true if this point is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this point is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(this PdfPoint point)
        {
            if (point.X < 0 || point.Y < 0)
                return false;

            if (point.X > 10000 || point.Y > 10000)
                return false;

            return true;
        }

    }
}
