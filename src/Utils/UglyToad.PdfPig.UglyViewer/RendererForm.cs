namespace UglyToad.PdfPig.UglyViewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Content;
    using Core;
    using DocumentLayoutAnalysis.TableExtractor;

    /// <summary>
    /// Test form
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class RendererForm : Form
    {
        private List<SimpleTableExtractor> contentExtractorResults;
        private readonly List<Page> documentPages = new List<Page>();

        private int currentPageIndex = -1;



        public RendererForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            fileOpen.Value = Properties.Settings.Default.FileName;

            splitContainer.Panel2.MouseWheel += splitContainer_Panel2_MouseWheel;
        }

        private void ShowDocument(string fileName)
        {
            using (var document = PdfDocument.Open(fileName))
            {
                var dp = document.GetPages();
                contentExtractorResults = new List<SimpleTableExtractor>();
                foreach (Page page in dp)
                {
                    SimpleTableExtractor contentExtractor = new SimpleTableExtractor();
                    contentExtractor.Read(page.ExperimentalAccess.Paths, page.GetWords());
                    contentExtractorResults.Add(contentExtractor);
                    this.documentPages.Add(page);
                }
                lblPages.Text = contentExtractorResults.Count.ToString();


                DrawPage(0);

            }
        }


        private void DrawPage(int pageIndex)
        {
            currentPageIndex = pageIndex;

            if (!contentExtractorResults[pageIndex].IsRefreshed)
            {
                contentExtractorResults[pageIndex].DetermineTableStructures();
                //contentExtractorResults[pageIndex].DetermineParagraphs();

                contentExtractorResults[pageIndex].FillContent();
            }

            txtPage.Text = (pageIndex + 1).ToString();
            txtPageContent.Text = contentExtractorResults[pageIndex].ToString();
            RedrawLines();

        }

        private void RedrawLines()
        {
            if (currentPageIndex < 0)
                return;

            var currentPage = contentExtractorResults[currentPageIndex];
            Page documentPage = documentPages[currentPageIndex];

            float maxY = (float)documentPage.Height;

            using (var g = splitContainer.Panel2.CreateGraphics())
            {
                g.Clear(splitContainer.Panel2.BackColor);

                if (chkLines.Checked)
                {
                    foreach (PdfSubpath.Line line in currentPage.AllLines)
                        g.DrawLine(Pens.DarkGray, (float)line.From.X, (float)line.From.Y, (float)line.To.X, (float)line.To.Y);

                    foreach (PdfSubpath.Line line in currentPage.JoinedHorizontalLines)
                        g.DrawLine(Pens.Blue, (float)line.From.X + 2, (float)line.From.Y + 2, (float)line.To.X + 2, (float) line.To.Y + 2);

                    foreach (PdfSubpath.Line line in currentPage.JoinedVerticalLines)
                        g.DrawLine(Pens.Blue, (float)line.From.X + 2, (float)line.From.Y + 2, (float)line.To.X + 2, (float) line.To.Y + 2);
                }

                if (chkTables.Checked)
                {
                    foreach (TableBlock tableStructure in currentPage.Tables)
                    {
                        g.DrawRectangle(Pens.OrangeRed, (float)tableStructure.TopLeftPoint.X + 4, (float)tableStructure.TopLeftPoint.Y + 4, (float)tableStructure.Width, (float) tableStructure.Heigth);

                        if (chkLines.Checked)
                        {
                            // To avoid too many lines
                            foreach (TableBlock.Row row in tableStructure.Rows)
                                g.FillRectangle(Brushes.OrangeRed, (float)tableStructure.TopLeftPoint.X + 5, (float) row.EndY + 5, 4, 4);

                            foreach (TableBlock.Column column in tableStructure.Columns)
                                g.FillRectangle(Brushes.OrangeRed, (float)column.BeginX + 5, (float) tableStructure.BottomRightPoint.Y + 5, 4, 4);

                        }
                        else
                        {
                            for (int i = 0; i < tableStructure.Rows.Count - 1; i++)
                            {
                                TableBlock.Row row = tableStructure.Rows[i];
                                g.DrawLine(Pens.OrangeRed, (float)tableStructure.TopLeftPoint.X + 5, (float)row.EndY + 5, (float)tableStructure.BottomRightPoint.X + 5, (float)row.EndY + 5);
                            }

                            for (int i = 1; i < tableStructure.Columns.Count; i++)
                            {
                                TableBlock.Column column = tableStructure.Columns[i];
                                g.DrawLine(Pens.OrangeRed, (float)column.BeginX + 5, (float)tableStructure.BottomRightPoint.Y + 5, (float)column.BeginX + 5, (float)tableStructure.TopLeftPoint.Y + 5);
                            }
                        }
                    }
                }

                if (chkParagraphs.Checked)
                {
                    /*
                    foreach (Paragraph paragraph in currentPage.Paragraphs)
                        g.FillRectangle(Brushes.OrangeRed, 0, (float)paragraph.Y + 5, 10, 4);
                    */
                }

                if (chkText.Checked)
                {
                    if (chkTextRealSize.Checked)
                    {
                        /* The words are missing the font size
                        foreach (var line in currentPage.Words.Where(_ => _. > 0))
                        {
                            Font font = new Font("Arial", (float)line.FontHeight * 0.7f);
                            g.DrawString(line.Content, font, Brushes.Black, (float)line.Position.X + 4, (float)line.Position.Y + 4);
                        }
                        */
                    }
                    else
                    {
                        foreach (var line in currentPage.Words)
                            g.DrawString(line.Text, this.Font, Brushes.Black, (float)line.BoundingBox.Left + 4, maxY - (float)line.BoundingBox.Top + 4);
                    }
                }
            }
        }

        private void splitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {
            RedrawLines();
        }

        private void chk_CheckedChanged(object sender, EventArgs e)
        {
            RedrawLines();
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            DrawPage(0);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            MovePreviousPage();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            MoveNextPage();
        }

        private void splitContainer_Panel2_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                MoveNextPage();
            else if (e.Delta > 0)
                MovePreviousPage();
        }

        private void MoveNextPage()
        {
            if (currentPageIndex < 0)
                return;
            if (currentPageIndex < contentExtractorResults.Count - 1)
                DrawPage(currentPageIndex + 1);
        }

        private void MovePreviousPage()
        {
            if (currentPageIndex < 0)
                return;
            if (currentPageIndex > 0)
                DrawPage(currentPageIndex - 1);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            DrawPage(contentExtractorResults.Count - 1);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            int page;
            if (int.TryParse(txtPage.Text, out page) && page > 0 && page <= contentExtractorResults.Count)
                DrawPage(page - 1);

        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fileOpen.Text))
                return;

            ShowDocument(fileOpen.Text);

            Properties.Settings.Default.FileName = fileOpen.Text;
            Properties.Settings.Default.Save();
        }


        private void btnHtmlExport_Click(object sender, EventArgs e)
        {
            string htmlFileName = fileOpen.Text + ".html";
            File.WriteAllText(htmlFileName, HtmlConverter.Convert(contentExtractorResults.Select(_ => _.Contents).ToList()));
            Process.Start(htmlFileName);
        }


    }
}
