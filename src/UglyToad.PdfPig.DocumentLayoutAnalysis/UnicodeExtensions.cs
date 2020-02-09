namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// 
    /// </summary>
    public static class UnicodeExtensions
    {
        /*internal static void Test()
        {
            // http://www.unicode.org/Public/12.0.0/ucd/UnicodeData.txt
            string[] lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(folderPath, "UnicodeData12.txt"));

            foreach (var line in lines)
            {
                var properties = line.Split(';');
                bool isRAL = properties[4] == "R" || properties[4] == "AL";
                int int32 = int.Parse(properties[0], System.Globalization.NumberStyles.AllowHexSpecifier);

                if (IsRorAL(int32) != isRAL)
                {
                    System.Console.WriteLine("ERROR: " + int32.ToString() + "\t" + isRAL);
                }
            }
        }*/

        /// <summary>
        /// Check if <see cref="Letter"/> contains at least an R (Right-to-Left) or AL (Right-to-Left Arabic) character.
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static bool IsRorAL(this Letter letter)
        {
            return letter.Value.IsRorAL();
        }

        /// <summary>
        /// Check if <see cref="string"/> contains at least an R (Right-to-Left) or AL (Right-to-Left Arabic) character.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsRorAL(this string s)
        {
            for (var i = 0; i < s.Length; i += char.IsSurrogatePair(s, i) ? 2 : 1)
            {
                var codepoint = char.ConvertToUtf32(s, i);
                if (IsRorAL(codepoint))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if <see cref="char"/> is R (Right-to-Left) or AL (Right-to-Left Arabic).
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsRorAL(this char c)
        {
            return IsRorAL(char.ConvertToUtf32(c.ToString(), 0));
        }

        /// <summary>
        /// Check if char is R (Right-to-Left) or AL (Right-to-Left Arabic).
        /// Unicode 12.0.0
        /// <para>https://www.unicode.org/reports/tr9/#Bidirectional_Character_Types</para>
        /// <para>http://www.unicode.org/Public/12.0.0/ucd/UnicodeData.txt</para>
        /// <para>https://www.w3.org/International/articles/inline-bidi-markup/uba-basics</para>
        /// </summary>
        /// <param name="utf32"></param>
        /// <returns></returns>
        private static bool IsRorAL(int utf32)
        {
            if (utf32 < 1470 || utf32 > 126651)
            {
                return false;
            }

            if (utf32 == 1470 || utf32 == 1472 || utf32 == 1475 || utf32 == 1478)
            {
                return true;
            }
            else if (utf32 >= 1488 && utf32 <= 1514)
            {
                return true;
            }
            else if (utf32 >= 1519 && utf32 <= 1524)
            {
                return true;
            }
            else if (utf32 == 1544 || utf32 == 1547 || utf32 == 1549 || utf32 == 1563 || utf32 == 1564)
            {
                return true;
            }
            else if (utf32 >= 1566 && utf32 <= 1610)
            {
                return true;
            }
            else if (utf32 == 1645 || utf32 == 1646 || utf32 == 1647)
            {
                return true;
            }
            else if (utf32 >= 1649 && utf32 <= 1749)
            {
                return true;
            }
            else if (utf32 == 1765 || utf32 == 1766 || utf32 == 1774 || utf32 == 1775)
            {
                return true;
            }
            else if (utf32 >= 1786 && utf32 <= 1839)
            {
                if (utf32 == 1806 || utf32 == 1809) return false;
                return true;
            }
            else if (utf32 >= 1869 && utf32 <= 1957)
            {
                return true;
            }
            else if (utf32 == 1969)
            {
                return true;
            }
            else if (utf32 >= 1984 && utf32 <= 2026)
            {
                return true;
            }
            else if (utf32 == 2036 || utf32 == 2037 || utf32 == 2042)
            {
                return true;
            }
            else if (utf32 >= 2046 && utf32 <= 2069)
            {
                return true;
            }
            else if (utf32 == 2074 || utf32 == 2084 || utf32 == 2088)
            {
                return true;
            }
            else if (utf32 >= 2096 && utf32 <= 2136)
            {
                if (utf32 == 2111) return false;
                return true;
            }
            else if (utf32 >= 2142 && utf32 <= 2154)
            {
                if (utf32 == 2143) return false;
                return true;
            }
            else if (utf32 >= 2208 && utf32 <= 2237)
            {
                if (utf32 == 2229) return false;
                return true;
            }
            else if (utf32 == 8207)
            {
                return true;
            }
            else if (utf32 >= 64285 && utf32 <= 64310)
            {
                if (utf32 == 64286 || utf32 == 64297) return false;
                return true;
            }
            else if (utf32 >= 64312 && utf32 <= 64316)
            {
                return true;
            }
            else if (utf32 == 64318)
            {
                return true;
            }
            else if (utf32 >= 64320 && utf32 <= 64324)
            {
                return true;
            }
            else if (utf32 >= 64326 && utf32 <= 64449)
            {
                return true;
            }
            else if (utf32 >= 64467 && utf32 <= 64829)
            {
                return true;
            }
            else if (utf32 >= 64848 && utf32 <= 64911)
            {
                return true;
            }
            else if (utf32 >= 64914 && utf32 <= 64967)
            {
                return true;
            }
            else if (utf32 >= 65008 && utf32 <= 65020)
            {
                return true;
            }
            else if (utf32 >= 65136 && utf32 <= 65140)
            {
                return true;
            }
            else if (utf32 >= 65142 && utf32 <= 65276)
            {
                return true;
            }
            else if (utf32 >= 67584 && utf32 <= 67589)
            {
                return true;
            }
            else if (utf32 == 67592)
            {
                return true;
            }
            else if (utf32 >= 67594 && utf32 <= 67637)
            {
                return true;
            }
            else if (utf32 == 67639 || utf32 == 67640 || utf32 == 67644)
            {
                return true;
            }
            else if (utf32 >= 67647 && utf32 <= 67669)
            {
                return true;
            }
            else if (utf32 >= 67671 && utf32 <= 67759)
            {
                return true;
            }
            else if (utf32 >= 67808 && utf32 <= 67826)
            {
                return true;
            }
            else if (utf32 == 67828 || utf32 == 67829)
            {
                return true;
            }
            else if (utf32 >= 67835 && utf32 <= 68096)
            {
                if (utf32 == 67871) return false;
                return true;
            }
            else if (utf32 >= 68112 && utf32 <= 68115)
            {
                return true;
            }
            else if (utf32 >= 68117 && utf32 <= 68119)
            {
                return true;
            }
            else if (utf32 >= 68121 && utf32 <= 68149)
            {
                return true;
            }
            else if (utf32 >= 68160 && utf32 <= 68168)
            {
                return true;
            }
            else if (utf32 >= 68176 && utf32 <= 68184)
            {
                return true;
            }
            else if (utf32 >= 68192 && utf32 <= 68255)
            {
                return true;
            }
            else if (utf32 >= 68288 && utf32 <= 68342)
            {
                if (utf32 == 68325 || utf32 == 68326) return false;
                return true;
            }
            else if (utf32 >= 68352 && utf32 <= 68405)
            {
                return true;
            }
            else if (utf32 >= 68416 && utf32 <= 68437)
            {
                return true;
            }
            else if (utf32 >= 68440 && utf32 <= 68466)
            {
                return true;
            }
            else if (utf32 >= 68472 && utf32 <= 68497)
            {
                return true;
            }
            else if (utf32 >= 68505 && utf32 <= 68508)
            {
                return true;
            }
            else if (utf32 >= 68521 && utf32 <= 68527)
            {
                return true;
            }
            else if (utf32 >= 68608 && utf32 <= 68680)
            {
                return true;
            }
            else if (utf32 >= 68736 && utf32 <= 68786)
            {
                return true;
            }
            else if (utf32 >= 68800 && utf32 <= 68850)
            {
                return true;
            }
            else if (utf32 >= 68858 && utf32 <= 68899)
            {
                return true;
            }
            else if (utf32 >= 69376 && utf32 <= 69415)
            {
                return true;
            }
            else if (utf32 >= 69424 && utf32 <= 69445)
            {
                return true;
            }
            else if (utf32 >= 69457 && utf32 <= 69465)
            {
                return true;
            }
            else if (utf32 >= 69600 && utf32 <= 69622)
            {
                return true;
            }
            else if (utf32 >= 124928 && utf32 <= 125135)
            {
                if (utf32 == 125125 || utf32 == 125126) return false;
                return true;
            }
            else if (utf32 >= 125184 && utf32 <= 125251)
            {
                return true;
            }
            else if (utf32 == 125259)
            {
                return true;
            }
            else if (utf32 >= 125264 && utf32 <= 125273)
            {
                return true;
            }
            else if (utf32 == 125278 || utf32 == 125279)
            {
                return true;
            }
            else if (utf32 >= 126065 && utf32 <= 126132)
            {
                return true;
            }
            else if (utf32 >= 126209 && utf32 <= 126269)
            {
                return true;
            }
            else if (utf32 >= 126464 && utf32 <= 126498)
            {
                if (utf32 == 126468 || utf32 == 126496) return false;
                return true;
            }
            else if (utf32 == 126500 || utf32 == 126503)
            {
                return true;
            }
            else if (utf32 >= 126505 && utf32 <= 126523)
            {
                if (utf32 == 126515 || utf32 == 126520 || utf32 == 126522) return false;
                return true;
            }
            else if (utf32 == 126530 || utf32 == 126535 || utf32 == 126537 || utf32 == 126539)
            {
                return true;
            }
            else if (utf32 >= 126541 && utf32 <= 126548)
            {
                if (utf32 == 126544 || utf32 == 126547) return false;
                return true;
            }
            else if (utf32 == 126551 || utf32 == 126553 || utf32 == 126555 || utf32 == 126557 ||
                     utf32 == 126559 || utf32 == 126561 || utf32 == 126562 || utf32 == 126564)
            {
                return true;
            }
            else if (utf32 >= 126567 && utf32 <= 126590)
            {
                if (utf32 == 126571 || utf32 == 126579 || utf32 == 126584 || utf32 == 126589) return false;
                return true;
            }
            else if (utf32 >= 126592 && utf32 <= 126619)
            {
                if (utf32 == 126602) return false;
                return true;
            }
            else if (utf32 >= 126625 && utf32 <= 126651)
            {
                if (utf32 == 126628 || utf32 == 126634) return false;
                return true;
            }

            return false;
        }

    }
}
