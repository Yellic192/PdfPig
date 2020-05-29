namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Collections.Generic;
    using Content;
    using Core;

    /// <summary>
    /// The content extractor
    /// </summary>
    public class ContentExtractor
    {

        /// <summary>
        /// The show parser information
        /// </summary>
        public static bool ShowParserInfo = false;

        /// <summary>
        /// The ignore white lines
        /// </summary>
        public static bool IgnoreWhiteLines = true;

        /// <summary>
        /// The tolerance. This parameter is used to determine same line/points
        /// Decrease this value if you need to discover more table cells/paragraphs
        /// Increase this value if you need to discover less table cells/paragraphs
        /// Often the right parameter is determined by the line boldness. Bold lines, 
        /// in pdf files, are box filled
        /// </summary>
        public static float Tolerance = 2f;



        /// <summary>
        /// The parsing errors
        /// </summary>
        public List<string> Errors = new List<string>();


        /// <summary>
        /// Reads the specified pages.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <returns></returns>
        public List<ResultPage> Read(IEnumerable<Page> pages)
        {
            var result = new List<ResultPage>();
            foreach (Page page in pages)
            {
                result.Add(Read(page));
            }

            return result;
        }


        /// <summary>
        /// Reads the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public ResultPage Read(Page page)
        {
            var resultPage = new ResultPage();

            resultPage.Words = page.GetWords();

            foreach (var path in page.ExperimentalAccess.Paths)
            {
                foreach (var subPath in path)
                {
                    PdfSubpath.Move move = new PdfSubpath.Move(new PdfPoint());
                    var lastPosition = Point.Origin;
                    foreach (var command in subPath.Commands)
                    {
                        if (command is PdfSubpath.Move m)
                        {
                            move = m;
                        }
                        else if (command is PdfSubpath.Line l)
                        {
                            var from = new Point(l.From.X, l.From.Y);
                            var to = new Point(l.To.X, l.To.Y);
                            if (from != to)
                                resultPage.AllLines.Add(new Line(from, to));
                            lastPosition = to;
                        } 
                        else if (command is PdfSubpath.BezierCurve)
                        {
                            continue;
                        } 
                        else if (command is PdfSubpath.Close cl)
                        {
                            
                            var from = lastPosition;
                            var to = new Point(move.Location.X, move.Location.Y);
                            if (from != to)
                                resultPage.AllLines.Add(new Line(from, to));
                            
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                }

            }

            resultPage.DeleteWrongLines();

            resultPage.DetermineTableStructures();
            resultPage.DetermineParagraphs();

            resultPage.FillContent();


            return resultPage;
        }

    }
}