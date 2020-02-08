namespace UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor
{
    using Content;
    using Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline;
    using Util;

    /// <summary>
    /// Nearest Neighbour Word Extractor, using the <see cref="Distances.Manhattan"/> distance.
    /// This implementation leverages bounding boxes.
    /// </summary>
    public class NearestNeighbourWordExtractor : IWordExtractor, ILayoutTransformer<IReadOnlyList<Letter>, IReadOnlyList<Word>>
    {
        /// <summary>
        /// Create an instance of Nearest Neighbour Word Extractor, <see cref="NearestNeighbourWordExtractor"/>.
        /// </summary>
        public static IWordExtractor Instance { get; } = new NearestNeighbourWordExtractor();

        /// <summary>
        /// Gets or sets the maximum number of concurrent tasks enabled. Default value is -1.
        /// <para>A positive property value limits the number of concurrent operations to the set value. 
        /// If it is -1, there is no limit on the number of concurrently running operations.</para>
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

        private Func<Letter, Letter, double> maxDistanceFunctionH;
        private Func<Letter, Letter, double> maxDistanceFunction270;
        private Func<Letter, Letter, double> maxDistanceFunction180;
        private Func<Letter, Letter, double> maxDistanceFunction90;
        private Func<Letter, Letter, double> maxDistanceFunctionOther;

        private Func<PdfPoint, PdfPoint, double> distMeasureH;
        private Func<PdfPoint, PdfPoint, double> distMeasure270;
        private Func<PdfPoint, PdfPoint, double> distMeasure180;
        private Func<PdfPoint, PdfPoint, double> distMeasure90;
        private Func<PdfPoint, PdfPoint, double> distMeasureOther;

        /// <summary>
        /// Create a new <see cref="NearestNeighbourWordExtractor"/>.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">Sets the maximum number of concurrent tasks enabled. 
        /// <para>A positive property value limits the number of concurrent operations to the set value. 
        /// If it is -1, there is no limit on the number of concurrently running operations.</para></param>
        public NearestNeighbourWordExtractor(int maxDegreeOfParallelism = -1)
            : this((l1, l2) => Math.Max(Math.Abs(l1.GlyphRectangle.Width), Math.Abs(l2.GlyphRectangle.Width)) * 0.2, Distances.Manhattan,
                   (l1, l2) => Math.Max(Math.Abs(l1.GlyphRectangle.Width), Math.Abs(l2.GlyphRectangle.Width)) * 0.5, Distances.Euclidean, maxDegreeOfParallelism)
        { }

        /// <summary>
        /// Create a new <see cref="NearestNeighbourWordExtractor"/>.
        /// </summary>
        /// <param name="maxDistanceFunction"></param>
        /// <param name="distMeasure"></param>
        /// <param name="maxDegreeOfParallelism"></param>
        public NearestNeighbourWordExtractor(Func<Letter, Letter, double> maxDistanceFunction, Func<PdfPoint, PdfPoint, double> distMeasure,
                                             int maxDegreeOfParallelism = -1)
            : this(maxDistanceFunction, distMeasure, maxDistanceFunction, distMeasure, maxDegreeOfParallelism)
        { }

        /// <summary>
        /// Create a new <see cref="NearestNeighbourWordExtractor"/>.
        /// </summary>
        /// <param name="maxDistanceFunction">The function that determines the maximum distance between two Letters, e.g. Max(GlyphRectangle.Width) x 20%.</param>
        /// <param name="distMeasure">The distance measure between two start and end base line points, e.g. the Manhattan distance.</param>
        /// <param name="maxDistanceFunctionOther">The function that determines the maximum distance between two Letters when TextDirection is <see cref="TextDirection.Other"/>, e.g. Max(GlyphRectangle.Width) x 50%.</param>
        /// <param name="distMeasureOther">The distance measure between two start and end base line points when TextDirection is <see cref="TextDirection.Other"/>, e.g. the Euclidean distance.</param>
        /// <param name="maxDegreeOfParallelism">Sets the maximum number of concurrent tasks enabled. 
        /// <para>A positive property value limits the number of concurrent operations to the set value. 
        /// If it is -1, there is no limit on the number of concurrently running operations.</para></param>
        public NearestNeighbourWordExtractor(Func<Letter, Letter, double> maxDistanceFunction, Func<PdfPoint, PdfPoint, double> distMeasure,
                                             Func<Letter, Letter, double> maxDistanceFunctionOther, Func<PdfPoint, PdfPoint, double> distMeasureOther,
                                             int maxDegreeOfParallelism = -1)
            : this(maxDistanceFunction, distMeasure,
                   maxDistanceFunction, distMeasure,
                   maxDistanceFunction, distMeasure,
                   maxDistanceFunction, distMeasure,
                   maxDistanceFunctionOther, distMeasureOther,
                   maxDegreeOfParallelism)
        { }

        /// <summary>
        /// Create a new <see cref="NearestNeighbourWordExtractor"/>.
        /// </summary>
        /// <param name="maxDistanceFunctionH">The function that determines the maximum distance between two Letters when TextDirection is <see cref="TextDirection.Horizontal"/>, e.g. Max(GlyphRectangle.Width) x 20%.</param>
        /// <param name="distMeasureH">The distance measure between two start and end base line points when TextDirection is <see cref="TextDirection.Horizontal"/>, e.g. the Manhattan distance.</param>
        /// <param name="maxDistanceFunction270">The function that determines the maximum distance between two Letters when TextDirection is <see cref="TextDirection.Rotate270"/>, e.g. Max(GlyphRectangle.Width) x 20%.</param>
        /// <param name="distMeasure270">The distance measure between two start and end base line points when TextDirection is <see cref="TextDirection.Rotate270"/>, e.g. the Manhattan distance.</param>
        /// <param name="maxDistanceFunction180">The function that determines the maximum distance between two Letters when TextDirection is <see cref="TextDirection.Rotate180"/>, e.g. Max(GlyphRectangle.Width) x 20%.</param>
        /// <param name="distMeasure180">The distance measure between two start and end base line points when TextDirection is <see cref="TextDirection.Rotate180"/>, e.g. the Manhattan distance.</param>
        /// <param name="maxDistanceFunction90">The function that determines the maximum distance between two Letters when TextDirection is <see cref="TextDirection.Rotate90"/>, e.g. Max(GlyphRectangle.Width) x 20%.</param>
        /// <param name="distMeasure90">The distance measure between two start and end base line points when TextDirection is <see cref="TextDirection.Rotate90"/>, e.g. the Manhattan distance.</param>
        /// <param name="maxDistanceFunctionOther">The function that determines the maximum distance between two Letters when TextDirection is <see cref="TextDirection.Other"/>, e.g. Max(GlyphRectangle.Width) x 50%.</param>
        /// <param name="distMeasureOther">The distance measure between two start and end base line points when TextDirection is <see cref="TextDirection.Other"/>, e.g. the Euclidean distance.</param>
        /// <param name="maxDegreeOfParallelism"></param>
        public NearestNeighbourWordExtractor(Func<Letter, Letter, double> maxDistanceFunctionH, Func<PdfPoint, PdfPoint, double> distMeasureH,
                                             Func<Letter, Letter, double> maxDistanceFunction270, Func<PdfPoint, PdfPoint, double> distMeasure270,
                                             Func<Letter, Letter, double> maxDistanceFunction180, Func<PdfPoint, PdfPoint, double> distMeasure180,
                                             Func<Letter, Letter, double> maxDistanceFunction90, Func<PdfPoint, PdfPoint, double> distMeasure90,
                                             Func<Letter, Letter, double> maxDistanceFunctionOther, Func<PdfPoint, PdfPoint, double> distMeasureOther,
                                             int maxDegreeOfParallelism = -1)
        {
            this.maxDistanceFunctionH = maxDistanceFunctionH;
            this.distMeasureH = distMeasureH;

            this.maxDistanceFunction270 = maxDistanceFunction270;
            this.distMeasure270 = distMeasure270;

            this.maxDistanceFunction180 = maxDistanceFunction180;
            this.distMeasure180 = distMeasure180;

            this.maxDistanceFunction90 = maxDistanceFunction90;
            this.distMeasure90 = distMeasure90;

            this.maxDistanceFunctionOther = maxDistanceFunctionOther;
            this.distMeasureOther = distMeasureOther;

            this.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// Gets the words.
        /// </summary>
        /// <param name="letters">The letters in the page.</param>
        public IEnumerable<Word> GetWords(IReadOnlyList<Letter> letters)
        {
            List<Word> words = GetWords(letters.Where(l => l.TextDirection == TextDirection.Horizontal), maxDistanceFunctionH, distMeasureH, MaxDegreeOfParallelism);
            words.AddRange(GetWords(letters.Where(l => l.TextDirection == TextDirection.Rotate270), maxDistanceFunction270, distMeasure270, MaxDegreeOfParallelism));
            words.AddRange(GetWords(letters.Where(l => l.TextDirection == TextDirection.Rotate180), maxDistanceFunction180, distMeasure180, MaxDegreeOfParallelism));
            words.AddRange(GetWords(letters.Where(l => l.TextDirection == TextDirection.Rotate90), maxDistanceFunction90, distMeasure90, MaxDegreeOfParallelism));
            words.AddRange(GetWords(letters.Where(l => l.TextDirection == TextDirection.Other), maxDistanceFunctionOther, distMeasureOther, MaxDegreeOfParallelism));
            return words;
        }

        /// <summary>
        /// Private method to get the words.
        /// </summary>
        /// <param name="pageLetters">The letters in the page, they must have
        /// the same text directions.</param>
        /// <param name="maxDistanceFunction">The function that determines the maximum distance between two Letters,
        /// e.g. Max(GlyphRectangle.Width) x 20%.</param>
        /// <param name="distMeasure">The distance measure between two start and end base line points,
        /// e.g. the Manhattan distance.</param>
        /// <param name="maxDegreeOfParallelism">Sets the maximum number of concurrent tasks enabled. 
        /// <para>A positive property value limits the number of concurrent operations to the set value. 
        /// If it is -1, there is no limit on the number of concurrently running operations.</para></param>
        private List<Word> GetWords(IEnumerable<Letter> pageLetters,
            Func<Letter, Letter, double> maxDistanceFunction, Func<PdfPoint, PdfPoint, double> distMeasure,
            int maxDegreeOfParallelism)
        {
            if (pageLetters == null || pageLetters.Count() == 0) return new List<Word>();
            TextDirection textDirection = pageLetters.ElementAt(0).TextDirection;

            if (pageLetters.Any(x => textDirection != x.TextDirection))
            {
                throw new ArgumentException("NearestNeighbourWordExtractor.GetWords(): Mixed Text Direction.");
            }

            Letter[] letters = pageLetters.ToArray();

            var groupedIndexes = ClusteringAlgorithms.ClusterNearestNeighbours(letters,
                distMeasure, maxDistanceFunction,
                l => l.EndBaseLine, l => l.StartBaseLine,
                l => !string.IsNullOrWhiteSpace(l.Value),
                (l1, l2) => !string.IsNullOrWhiteSpace(l2.Value),
                maxDegreeOfParallelism).ToList();

            List<Word> words = new List<Word>();
            for (int a = 0; a < groupedIndexes.Count(); a++)
            {
                words.Add(new Word(groupedIndexes[a].Select(i => letters[i]).ToList()));
            }

            return words;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IReadOnlyList<Word> Get(IReadOnlyList<Letter> input, DLAContext context)
        {
            return GetWords(input).ToList();
        }
    }
}
