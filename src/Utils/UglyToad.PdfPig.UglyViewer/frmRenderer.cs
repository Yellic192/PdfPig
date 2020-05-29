namespace UglyToad.PdfPig.UglyViewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Content;
    using DocumentLayoutAnalysis.TableExtractor;

    public partial class frmRenderer : Form
    {
        private List<ResultPage> _pages;
        private List<Page> _documentPages = new List<Page>();

        private int _currentPageIndex = -1;



        public frmRenderer()
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
                var documentPages = document.GetPages();
                ContentExtractor contentExtractor = new ContentExtractor();
                _pages = contentExtractor.Read(documentPages);
                lblPages.Text = _pages.Count.ToString();

                foreach (Page page in documentPages)
                {
                    _documentPages.Add(page);
                }

                DrawPage(0);

            }
        }


        private void DrawPage(int pageIndex)
        {
            _currentPageIndex = pageIndex;

            if (!_pages[pageIndex].IsRefreshed)
            {
                _pages[pageIndex].DetermineTableStructures();
                _pages[pageIndex].DetermineParagraphs();

                _pages[pageIndex].FillContent();
            }

            txtPage.Text = (pageIndex + 1).ToString();
            txtPageContent.Text = _pages[pageIndex].ToString();
            RedrawLines();

        }

        private void RedrawLines()
        {
            if (_currentPageIndex < 0)
                return;

            ResultPage currentPage = _pages[_currentPageIndex];
            Page documentPage = _documentPages[_currentPageIndex];

            using (var g = splitContainer.Panel2.CreateGraphics())
            {

                g.Clear(splitContainer.Panel2.BackColor);

                if (chkLines.Checked)
                {
                    foreach (Line line in currentPage.AllLines)
                        g.DrawLine(Pens.DarkGray, (float)line.StartPoint.X, (float)line.StartPoint.Y, (float)line.EndPoint.X, (float)line.EndPoint.Y);

                    foreach (Line line in currentPage.JoinedHorizontalLines)
                        g.DrawLine(Pens.Blue, (float)line.StartPoint.X + 2, (float)line.StartPoint.Y + 2, (float)line.EndPoint.X + 2, (float) line.EndPoint.Y + 2);

                    foreach (Line line in currentPage.JoinedVerticalLines)
                        g.DrawLine(Pens.Blue, (float)line.StartPoint.X + 2, (float)line.StartPoint.Y + 2, (float)line.EndPoint.X + 2, (float) line.EndPoint.Y + 2);
                }

                if (chkTables.Checked)
                {
                    foreach (Table tableStructure in currentPage.Tables)
                    {
                        g.DrawRectangle(Pens.OrangeRed, (float)tableStructure.TopLeftPoint.X + 4, (float)tableStructure.TopLeftPoint.Y + 4, (float)tableStructure.Width, (float) tableStructure.Heigth);

                        if (chkLines.Checked)
                        {
                            // To avoid too many lines
                            foreach (Row row in tableStructure.Rows)
                                g.FillRectangle(Brushes.OrangeRed, (float)tableStructure.TopLeftPoint.X + 5, (float) row.EndY + 5, 4, 4);

                            foreach (Column column in tableStructure.Columns)
                                g.FillRectangle(Brushes.OrangeRed, (float)column.BeginX + 5, (float) tableStructure.BottomRightPoint.Y + 5, 4, 4);

                        }
                        else
                        {
                            for (int i = 0; i < tableStructure.Rows.Count - 1; i++)
                            {
                                Row row = tableStructure.Rows[i];
                                g.DrawLine(Pens.OrangeRed, (float)tableStructure.TopLeftPoint.X + 5, (float)row.EndY + 5, (float)tableStructure.BottomRightPoint.X + 5, (float)row.EndY + 5);
                            }

                            for (int i = 1; i < tableStructure.Columns.Count; i++)
                            {
                                Column column = tableStructure.Columns[i];
                                g.DrawLine(Pens.OrangeRed, (float)column.BeginX + 5, (float)tableStructure.BottomRightPoint.Y + 5, (float)column.BeginX + 5, (float)tableStructure.TopLeftPoint.Y + 5);
                            }
                        }
                    }
                }

                if (chkParagraphs.Checked)
                {
                    foreach (Paragraph paragraph in currentPage.Paragraphs)
                        g.FillRectangle(Brushes.OrangeRed, 0, (float)paragraph.Y + 5, 10, 4);
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
                            g.DrawString(line.Text, this.Font, Brushes.Black, (float)line.BoundingBox.Left + 4, (float)line.BoundingBox.Top + 4);
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
            if (_currentPageIndex < 0)
                return;
            if (_currentPageIndex < _pages.Count - 1)
                DrawPage(_currentPageIndex + 1);
        }

        private void MovePreviousPage()
        {
            if (_currentPageIndex < 0)
                return;
            if (_currentPageIndex > 0)
                DrawPage(_currentPageIndex - 1);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            DrawPage(_pages.Count - 1);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            int page;
            if (int.TryParse(txtPage.Text, out page) && page > 0 && page <= _pages.Count)
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
            File.WriteAllText(htmlFileName, HtmlConverter.Convert(_pages));
            Process.Start(htmlFileName);
        }


    }
}
