namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Collections.Generic;
    using Content;
    using Core;
    using System.Linq;
    using Graphics;

    /// <summary>
    /// The content extractor
    /// </summary>
    public class ContentExtractor
    {
        /// <summary>
        /// The ignore white lines
        /// </summary>
        public bool IgnoreWhiteLines { get; set; } = true;

        /// <summary>
        /// The tolerance. This parameter is used to determine same line/points
        /// Decrease this value if you need to discover more table cells/paragraphs
        /// Increase this value if you need to discover less table cells/paragraphs
        /// Often the right parameter is determined by the line boldness. Bold lines, 
        /// in pdf files, are box filled
        /// </summary>
        public float Tolerance { get; set; }= 2f;

        /// <summary>
        /// Reads the specified page.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <param name="words">The words.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public IEnumerable<IPageContent> Read(IEnumerable<PdfPath> paths, IEnumerable<Word> words)
        {

            Words = words;

            foreach (var path in paths)
            {
                foreach (var subPath in path)
                {
                    PdfSubpath.Move move = new PdfSubpath.Move(new PdfPoint());
                    var lastPosition = PdfPoint.Origin;
                    foreach (var command in subPath.Commands)
                    {
                        if (command is PdfSubpath.Move m)
                        {
                            move = m;
                        }
                        else if (command is PdfSubpath.Line l)
                        {
                            var from = new PdfPoint(l.From.X, l.From.Y);
                            var to = new PdfPoint(l.To.X, l.To.Y);
                            switch (from.CompareTo(to, Tolerance))
                            {
                                case -1:
                                    AllLines.Add(new PdfSubpath.Line(from, to));
                                    break;
                                case 1:
                                    AllLines.Add(new PdfSubpath.Line(to, from));
                                    break;
                            }
                            lastPosition = to;
                        } 
                        else if (command is PdfSubpath.BezierCurve)
                        {
                            continue;
                        } 
                        else if (command is PdfSubpath.Close)
                        {
                            
                            var from = lastPosition;
                            var to = new PdfPoint(move.Location.X, move.Location.Y);
                            switch (from.CompareTo(to, Tolerance))
                            {
                                case -1:
                                    AllLines.Add(new PdfSubpath.Line(from, to));
                                    break;
                                case 1:
                                    AllLines.Add(new PdfSubpath.Line(to, from));
                                    break;
                            }
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                }

            }

            DeleteWrongLines();

            DetermineTableStructures();
            DetermineParagraphs();

            FillContent();


            return Contents;
        }



        /// <summary>
        /// Gets all lines.
        /// </summary>
        /// <value>
        /// All lines.
        /// </value>
        public List<PdfSubpath.Line> AllLines { get; private set; } = new List<PdfSubpath.Line>();

        /// <summary>
        /// Gets or sets the joined horizontal lines.
        /// </summary>
        /// <value>
        /// The joined horizontal lines.
        /// </value>
        public List<PdfSubpath.Line> JoinedHorizontalLines { get; set; }

        /// <summary>
        /// Gets or sets the joined vertical lines.
        /// </summary>
        /// <value>
        /// The joined vertical lines.
        /// </value>
        public List<PdfSubpath.Line> JoinedVerticalLines { get; set; }

        /// <summary>
        /// Gets or sets the joined lines.
        /// </summary>
        /// <value>
        /// The joined lines.
        /// </value>
        public List<PdfSubpath.Line> JoinedLines { get; set; }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>
        /// The rotation.
        /// </value>
        public int Rotation { get; set; }

        /// <summary>
        /// Gets or sets the table structures.
        /// </summary>
        /// <value>
        /// The table structures.
        /// </value>
        public List<Table> Tables { get; } = new List<Table>();

        /// <summary>
        /// Gets or sets the paragraphs.
        /// </summary>
        /// <value>
        /// The paragraphs.
        /// </value>
        public List<Paragraph> Paragraphs { get; set; }

        /// <summary>
        /// Gets or sets the contents.
        /// </summary>
        /// <value>
        /// The contents.
        /// </value>
        public List<IPageContent> Contents { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is refreshed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is refreshed; otherwise, <c>false</c>.
        /// </value>
        public bool IsRefreshed { get { return JoinedLines != null; } }


        /// <summary>
        /// Gets or sets the text blocks.
        /// </summary>
        /// <value>
        /// The text blocks.
        /// </value>
        public IEnumerable<Word> Words { get; set; }

        /// <summary>
        /// Deletes the wrong lines.
        /// </summary>
        public void DeleteWrongLines()
        {
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            AllLines = AllLines.Where(_ => _.From.IsValid() && _.To.IsValid()).ToList();
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }


        /// <summary>
        /// Determines the table structures.
        /// </summary>
        public void DetermineTableStructures()
        {
            JoinedLines = JoinLines(AllLines);

            // Find table borders
            foreach (PdfSubpath.Line horizontalLine in JoinedHorizontalLines.OrderBy(_ => _.From.Y))
            {
                // We consider that this line is a top line of a table if
                // 1. There is not a table with this line inside
                // 2. There is a vertical line starting from this line

                if (Tables.Any(_ => _.Contains(horizontalLine.From.Y, Tolerance)))
                    continue;

                PdfSubpath.Line tableLine = JoinedVerticalLines
                    .Where(_ => _.From.Equals(horizontalLine.From, Tolerance) || _.From.Equals(horizontalLine.To, Tolerance))
                    .OrderByDescending(_ => _.To.Y - _.From.Y)
                    .FirstOrDefault();

                if (tableLine == null)
                    continue;

                Table tableStructure = new Table()
                {
                    TopLeftPoint = horizontalLine.From,
                    BottomRightPoint = new PdfPoint(horizontalLine.To.X, tableLine.To.Y)
                };

                Tables.Add(tableStructure);
            }

            // Add the first row and the first column to all tables
            foreach (Table tableStructure in Tables)
            {
                tableStructure.Rows.Add(new Row() { BeginY = tableStructure.TopLeftPoint.Y });
                tableStructure.Columns.Add(new Column() { BeginX = tableStructure.TopLeftPoint.X });
            }

            // Find rows
            foreach (PdfSubpath.Line horizontalLine in JoinedHorizontalLines.OrderBy(_ => _.From.Y))
            {
                var tableStructure = Tables.FirstOrDefault(_ => _.Contains(horizontalLine, Tolerance));
                // No table contains this line
                if (tableStructure == null)
                    continue;

                // Check if the row already belongs to the table
                if (tableStructure.Rows.Any(_ => Math.Abs(_.BeginY - horizontalLine.From.Y) < Tolerance))
                    continue;

                // Check if the row is the bottom edge of the table
                if (tableStructure.BottomRightPoint.Y - horizontalLine.From.Y < Tolerance)
                    continue;

                tableStructure.Rows.Add(new Row() { BeginY = horizontalLine.From.Y });
            }

            // Find columns
            foreach (PdfSubpath.Line verticalLine in JoinedVerticalLines.OrderBy(_ => _.From.X))
            {
                var tableStructure = Tables.FirstOrDefault(_ => _.Contains(verticalLine, Tolerance));
                // No table contains this line
                if (tableStructure == null)
                    continue;

                // The row already belongs to the table
                if (tableStructure.Columns.Any(_ => Math.Abs(_.BeginX - verticalLine.From.X) < Tolerance))
                    continue;

                // Check if the row is the bottom edge of the table
                if (tableStructure.BottomRightPoint.X - verticalLine.From.X < Tolerance)
                    continue;


                tableStructure.Columns.Add(new Column() { BeginX = verticalLine.From.X });
            }


            // Fix EndX and EndY and indexes
            foreach (Table tableStructure in Tables)
            {
                // Fix EndYs
                for (int i = 0; i < tableStructure.Rows.Count - 1; i++)
                    tableStructure.Rows[i].EndY = tableStructure.Rows[i + 1].BeginY - Tolerance * 0.1f;

                tableStructure.Rows[tableStructure.Rows.Count - 1].EndY = tableStructure.BottomRightPoint.Y;


                // Fix EndXs
                for (int i = 0; i < tableStructure.Columns.Count - 1; i++)
                    tableStructure.Columns[i].EndX = tableStructure.Columns[i + 1].BeginX - Tolerance * 0.1f;

                tableStructure.Columns[tableStructure.Columns.Count - 1].EndX = tableStructure.BottomRightPoint.X;

                int index;

                index = 0;
                foreach (var column in tableStructure.Columns.OrderBy(_ => _.BeginX))
                {
                    column.Index = index;
                    index++;
                }

                index = 0;
                foreach (var row in tableStructure.Rows.OrderByDescending(_ => _.BeginY))
                {
                    row.Index = index;
                    index++;
                }

                tableStructure.CreateContent();

            }

        }


        /// <summary>
        /// Joins the horizontal and vertical lines.
        /// </summary>
        /// <param name="allLines">All the lines.</param>
        /// <returns>The orizontal and the vertical lines (eventually joined)</returns>
        private List<PdfSubpath.Line> JoinLines(List<PdfSubpath.Line> allLines)
        {
            JoinedVerticalLines = JoinVerticalLines(allLines, Tolerance);
            JoinedHorizontalLines = JoinHorizontalLines(allLines, Tolerance);

            return JoinedHorizontalLines.Union(JoinedVerticalLines).ToList();
        }

        /// <summary>
        /// Joins the vertical lines.
        /// </summary>
        /// <param name="allLines">All lines.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        /// The vertical lines (eventually joined)
        /// </returns>
        private static List<PdfSubpath.Line> JoinVerticalLines(List<PdfSubpath.Line> allLines, float tolerance)
        {
            var lines = new List<PdfSubpath.Line>();

            var verticalLines = allLines.Where(_ => _.IsVertical(tolerance)).OrderBy(_ => _.From.X).ThenBy(_ => _.From.Y).ToList();

            foreach (PdfSubpath.Line verticalLine in verticalLines)
            {
                if (lines.Count == 0)
                    lines.Add(verticalLine);
                else if (verticalLine.IsCoincident(lines[lines.Count - 1], tolerance))
                    continue;
                else if (verticalLine.IsOverlapped(lines[lines.Count - 1], tolerance))
                {
                    var joinedLine = lines[lines.Count - 1].Join(verticalLine, tolerance);
                    lines.RemoveAt(lines.Count - 1);
                    lines.Add(joinedLine);
                }
                else
                    lines.Add(verticalLine);
            }

            return lines;
        }

        private static List<PdfSubpath.Line> JoinHorizontalLines(List<PdfSubpath.Line> allLines, float tolerance)
        {
            var lines = new List<PdfSubpath.Line>();

            var horizontalLines = allLines.Where(_ => _.IsHorizontal(tolerance)).OrderBy(_ => _.From.Y).ThenBy(_ => _.From.X).ToList();

            foreach (PdfSubpath.Line horizontalLine in horizontalLines)
            {
                if (lines.Count == 0)
                    lines.Add(horizontalLine);
                else if (horizontalLine.IsCoincident(lines[lines.Count - 1], tolerance))
                    continue;
                else if (horizontalLine.IsOverlapped(lines[lines.Count - 1], tolerance))
                {
                    var joinedLine = horizontalLine.Join(lines[lines.Count - 1], tolerance);
                    lines.RemoveAt(lines.Count - 1);
                    lines.Add(joinedLine);
                }
                else
                    lines.Add(horizontalLine);
            }

            return lines;
        }


        /// <summary>
        /// Determines the paragraphs.
        /// </summary>
        public void DetermineParagraphs()
        {
            Paragraphs = new List<Paragraph>();

            var textBlockLines = Words
                .Where(_ => !string.IsNullOrWhiteSpace(_.Text))
                .Where(_ => !Tables.Any(t => t.Contains(_.BoundingBox.Top, Tolerance)))
                .OrderBy(_ => _.BoundingBox.Top);

            foreach (var line in textBlockLines)
            {
                if (!Paragraphs.Any(t => t.Contains(new PdfPoint(line.BoundingBox.Left, line.BoundingBox.Top), Tolerance)))
                    Paragraphs.Add(new Paragraph(line.BoundingBox.Top));
            }
        }


        /// <summary>
        /// Fills the content.
        /// </summary>
        public void FillContent()
        {
            Contents = new List<IPageContent>();
            Contents.AddRange(Paragraphs.Cast<IPageContent>().Union(Tables).OrderBy(_ => _.Y));

            var textBoxLines = Words
                .Where(_ => !string.IsNullOrWhiteSpace(_.Text))
                .OrderBy(_ => _.BoundingBox.Top).ThenBy(_ => _.BoundingBox.Left);

            foreach (var line in textBoxLines.Where(_ => _.BoundingBox.TopLeft.IsValid()))
            {
                IPageContent targetPageContent = Contents.First(_ => _.Contains(line.BoundingBox.Top, Tolerance));
                targetPageContent.AddText(new PdfPoint(line.BoundingBox.Left, line.BoundingBox.Top), Tolerance, line.Text);
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string pageContent = string.Empty;
            if (Contents != null)
            {
                foreach (IPageContent content in Contents)
                    pageContent += string.Format("{0}\r\n", content);
            }
            return pageContent;
        }



    }
}