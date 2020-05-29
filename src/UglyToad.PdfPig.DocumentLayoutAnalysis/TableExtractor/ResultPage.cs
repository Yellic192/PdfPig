namespace UglyToad.PdfPig.DocumentLayoutAnalysis.TableExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Content;
    using Core;

    /// <summary>
    /// A Pdf page
    /// </summary>
    public class ResultPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultPage"/> class.
        /// </summary>
        public ResultPage()
        {
            Tables = new List<Table>();
            AllLines = new List<Line>();
        }

        /// <summary>
        /// Gets all lines.
        /// </summary>
        /// <value>
        /// All lines.
        /// </value>
        public List<Line> AllLines { get; private set; }

        /// <summary>
        /// Gets or sets the joined horizontal lines.
        /// </summary>
        /// <value>
        /// The joined horizontal lines.
        /// </value>
        public List<Line> JoinedHorizontalLines { get; set; }

        /// <summary>
        /// Gets or sets the joined vertical lines.
        /// </summary>
        /// <value>
        /// The joined vertical lines.
        /// </value>
        public List<Line> JoinedVerticalLines { get; set; }

        /// <summary>
        /// Gets or sets the joined lines.
        /// </summary>
        /// <value>
        /// The joined lines.
        /// </value>
        public List<Line> JoinedLines { get; set; }

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
        public List<Table> Tables { get; set; }

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
            AllLines = AllLines.Where(_ => _.StartPoint.IsValid() && _.EndPoint.IsValid()).ToList();
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }


        /// <summary>
        /// Determines the table structures.
        /// </summary>
        public void DetermineTableStructures()
        {
            JoinedLines = JoinLines(AllLines);

            // Find table borders
            foreach (Line horizontalLine in JoinedHorizontalLines.OrderBy(_ => _.StartPoint.Y))
            {
                // We consider that this line is a top line of a table if
                // 1. There is not a table with this line inside
                // 2. There is a vertical line starting from this line

                if (Tables.Any(_ => _.Contains(horizontalLine.StartPoint.Y)))
                    continue;

                Line? tableLine = JoinedVerticalLines
                    .Where(_ => _.StartPoint.Equals(horizontalLine.StartPoint, ContentExtractor.Tolerance) || _.StartPoint.Equals(horizontalLine.EndPoint, ContentExtractor.Tolerance))
                    .OrderByDescending(_ => _.EndPoint.Y - _.StartPoint.Y)
                    .Cast<Line?>()
                    .FirstOrDefault();

                if (tableLine == null)
                    continue;

                Table tableStructure = new Table()
                {
                    TopLeftPoint = horizontalLine.StartPoint,
                    BottomRightPoint = new PdfPoint(horizontalLine.EndPoint.X, tableLine.Value.EndPoint.Y)
                };

                Tables.Add(tableStructure);
            }

            // Add the first row and the first column to all tables
            foreach (Table tableStructure in Tables)
            {
                tableStructure.Rows.Add(new Row(){BeginY = tableStructure.TopLeftPoint.Y});
                tableStructure.Columns.Add(new Column(){BeginX = tableStructure.TopLeftPoint.X});
            }

            // Find rows
            foreach (Line horizontalLine in JoinedHorizontalLines.OrderBy(_ => _.StartPoint.Y))
            {
                var tableStructure = Tables.FirstOrDefault(_ => _.Contains(horizontalLine));
                // No table contains this line
                if (tableStructure == null)
                    continue;

                // Check if the row already belongs to the table
                if (tableStructure.Rows.Any(_ => Math.Abs(_.BeginY - horizontalLine.StartPoint.Y) < ContentExtractor.Tolerance))
                    continue;

                // Check if the row is the bottom edge of the table
                if (tableStructure.BottomRightPoint.Y - horizontalLine.StartPoint.Y < ContentExtractor.Tolerance)
                    continue;

                tableStructure.Rows.Add(new Row() {BeginY = horizontalLine.StartPoint.Y});
            }

            // Find columns
            foreach (Line verticalLine in JoinedVerticalLines.OrderBy(_ => _.StartPoint.X))
            {
                var tableStructure = Tables.FirstOrDefault(_ => _.Contains(verticalLine));
                // No table contains this line
                if (tableStructure == null)
                    continue;

                // The row already belongs to the table
                if (tableStructure.Columns.Any(_ => Math.Abs(_.BeginX - verticalLine.StartPoint.X) < ContentExtractor.Tolerance))
                    continue;

                // Check if the row is the bottom edge of the table
                if (tableStructure.BottomRightPoint.X - verticalLine.StartPoint.X < ContentExtractor.Tolerance)
                    continue;


                tableStructure.Columns.Add(new Column() { BeginX = verticalLine.StartPoint.X });
            }


            // Fix EndX and EndY and indexes
            foreach (Table tableStructure in Tables)
            {
                // Fix EndYs
                for (int i = 0; i < tableStructure.Rows.Count - 1; i++)
                    tableStructure.Rows[i].EndY = tableStructure.Rows[i + 1].BeginY - ContentExtractor.Tolerance * 0.1f;

                tableStructure.Rows[tableStructure.Rows.Count - 1].EndY = tableStructure.BottomRightPoint.Y;


                // Fix EndXs
                for (int i = 0; i < tableStructure.Columns.Count - 1; i++)
                    tableStructure.Columns[i].EndX = tableStructure.Columns[i + 1].BeginX - ContentExtractor.Tolerance * 0.1f;

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
        private List<Line> JoinLines(List<Line> allLines)
        {
            JoinedVerticalLines =  JoinVerticalLines(allLines);
            JoinedHorizontalLines = JoinHorizontalLines(allLines);

            return JoinedHorizontalLines.Union(JoinedVerticalLines).ToList();
        }

        /// <summary>
        /// Joins the vertical lines.
        /// </summary>
        /// <param name="allLines">All lines.</param>
        /// <returns>The vertical lines (eventually joined)</returns>
        private static List<Line> JoinVerticalLines(List<Line> allLines)
        {
            var lines = new List<Line>();

            var verticalLines = allLines.Where(_ => _.IsVertical()).OrderBy(_ => _.StartPoint.X).ThenBy(_ => _.StartPoint.Y).ToList();

            foreach (Line verticalLine in verticalLines)
            {
                if (lines.Count == 0)
                    lines.Add(verticalLine);
                else if (verticalLine.IsCoincident(lines[lines.Count - 1], ContentExtractor.Tolerance))
                    continue;
                else if (verticalLine.IsOverlapped(lines[lines.Count - 1], ContentExtractor.Tolerance))
                {
                    var joinedLine = lines[lines.Count - 1].Join(verticalLine, ContentExtractor.Tolerance);
                    lines.RemoveAt(lines.Count - 1);
                    lines.Add(joinedLine);
                }
                else
                    lines.Add(verticalLine);
            }

            return lines;
        }

        private static List<Line> JoinHorizontalLines(List<Line> allLines)
        {
            var lines = new List<Line>();

            var horizontalLines = allLines.Where(_ => _.IsHorizontal()).OrderBy(_ => _.StartPoint.Y).ThenBy(_ => _.StartPoint.X).ToList();

            foreach (Line horizontalLine in horizontalLines)
            {
                if (lines.Count == 0)
                    lines.Add(horizontalLine);
                else if (horizontalLine.IsCoincident(lines[lines.Count - 1], ContentExtractor.Tolerance))
                    continue;
                else if (horizontalLine.IsOverlapped(lines[lines.Count - 1], ContentExtractor.Tolerance))
                {
                    var joinedLine = horizontalLine.Join(lines[lines.Count - 1], ContentExtractor.Tolerance);
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
                .Where(_ => !Tables.Any(t => t.Contains(_.BoundingBox.Top)))
                .OrderBy(_ => _.BoundingBox.Top);

            foreach (var line in textBlockLines)
            {
                if (!Paragraphs.Any(t => t.Contains(new PdfPoint(line.BoundingBox.Left, line.BoundingBox.Top)) ))
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
                IPageContent targetPageContent = Contents.First(_ => _.Contains(line.BoundingBox.Top));
                targetPageContent.AddText(new PdfPoint(line.BoundingBox.Left, line.BoundingBox.Top), line.Text);
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