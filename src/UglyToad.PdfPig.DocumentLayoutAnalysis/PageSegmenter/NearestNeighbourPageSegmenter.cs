namespace UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using static UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter.DocstrumBoundingBoxes;

    /// <summary>
    /// 
    /// </summary>
    public class NearestNeighbourPageSegmenter : IPageSegmenter
    {
        /// <summary>
        /// Create an instance of nearest neighbour page segmenter, <see cref="NearestNeighbourPageSegmenter"/>.
        /// </summary>
        public static NearestNeighbourPageSegmenter Instance { get; } = new NearestNeighbourPageSegmenter();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageWords"></param>
        /// <returns></returns>
        public IReadOnlyList<TextBlock> GetBlocks(IEnumerable<Word> pageWords)
        {
            var words = pageWords.Where(w => !string.IsNullOrWhiteSpace(w.Text)).ToList();

            // group words into lines
            double avgWidth = words.Average(w => w.BoundingBox.Width);
            var groupedWords = ClusteringAlgorithms.ClusterNearestNeighbours(words, Distances.Euclidean, (w1, w2) => avgWidth,
                       pivot => pivot.BoundingBox.BottomRight,
                       candidate => candidate.BoundingBox.BottomLeft,
                       w => true,
                       (pivot, candidate) => new AngleBounds(pivot.BoundingBox.Rotation - 10, pivot.BoundingBox.Rotation + 10).Contains(Distances.Angle(pivot.BoundingBox.BottomRight, candidate.BoundingBox.BottomLeft)),
                       -1).ToList();

            List<TextLine> lines = new List<TextLine>();
            for (int a = 0; a < groupedWords.Count; a++)
            {
                lines.Add(new TextLine(groupedWords[a].Select(i => words[i]).Reverse().ToList()));
            }

            // group lines into blocks
            var groupedLines = ClusteringAlgorithms.ClusterNearestNeighbours(lines, Distances.Euclidean, (w1, w2) => avgWidth*2,
                pivot => pivot.BoundingBox.Centroid,
                candidate => candidate.BoundingBox.Centroid,
                w => true,
                (pivot, candidate) => true, //(pivot, candidate) => new AngleBounds(pivot.BoundingBox.Rotation - 90 - 45, pivot.BoundingBox.Rotation - 90 + 45).Contains(Distances.Angle(pivot.BoundingBox.Centroid, candidate.BoundingBox.Centroid)),
                -1).ToList();

            List<TextBlock> blocks = new List<TextBlock>();
            for (int a = 0; a < groupedLines.Count; a++)
            {
                blocks.Add(new TextBlock(groupedLines[a].Select(i => lines[i]).ToList()));
            }

            return blocks;
        }
    }
}
