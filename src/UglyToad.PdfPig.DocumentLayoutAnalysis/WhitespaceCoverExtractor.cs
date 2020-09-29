namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.Geometry;
    using UglyToad.PdfPig.Graphics;

    /// <summary>
    /// A top-down algorithm that finds a cover of the background whitespace of a document in terms of maximal empty rectangles.
    /// <para>See Section 3.2 of 'High precision text extraction from PDF documents' by Øyvind Raddum Berg and Section 2 of 'Two geometric algorithms for layout analysis' by Thomas M. Breuel.</para>
    /// </summary>
    public static class WhitespaceCoverExtractor
    {

        /// <summary>
        /// Gets the cover of the background whitespace of a page in terms of maximal empty rectangles.
        /// </summary>
        /// <param name="words">The words in the page.</param>
        /// <param name="images">The images in the page.</param>
        /// <param name="paths">The paths drawn in the page.</param>
        /// <param name="maxRectangleCount">The maximum number of rectangles to find.</param>
        /// <param name="maxBoundQueueSize">The maximum size of the queue used in the algorithm.</param>
        /// <returns>The identified whitespace rectangles.</returns>
        public static IReadOnlyList<PdfRectangle> GetWhitespaces(IEnumerable<Word> words, IEnumerable<IPdfImage> images = null, IEnumerable<PdfPath> paths = null,
            int maxRectangleCount = 40, int maxBoundQueueSize = 0)
        {
            return GetWhitespaces(words,
                                  images,
                                  paths,
                                  words.SelectMany(w => w.Letters).Select(x => x.GlyphRectangle.Width).Mode() * 1.25,
                                  words.SelectMany(w => w.Letters).Select(x => x.GlyphRectangle.Height).Mode() * 1.25,
                                  maxRectangleCount: maxRectangleCount,
                                  maxBoundQueueSize: maxBoundQueueSize);
        }

        /// <summary>
        /// Gets the cover of the background whitespace of a page in terms of maximal empty rectangles.
        /// </summary>
        /// <param name="words">The words in the page.</param>
        /// <param name="images">The images in the page.</param>
        /// <param name="paths">The paths drawn in the page.</param>
        /// <param name="minWidth">Lower bounds for the width of rectangles.</param>
        /// <param name="minHeight">Lower bounds for the height of rectangles.</param>
        /// <param name="maxRectangleCount">The maximum number of rectangles to find.</param>
        /// <param name="whitespaceFuzziness">Constant value to allow candidate whitespace rectangle to overlap the
        /// surrounding obstacles by some percent. Default value is 15%.</param>
        /// <param name="maxBoundQueueSize">The maximum size of the queue used in the algorithm.</param>
        /// <returns>The identified whitespace rectangles.</returns>
        public static IReadOnlyList<PdfRectangle> GetWhitespaces(IEnumerable<Word> words, IEnumerable<IPdfImage> images, IEnumerable<PdfPath> paths,
            double minWidth, double minHeight, int maxRectangleCount = 40, double whitespaceFuzziness = 0.15, int maxBoundQueueSize = 0)
        {
            var bboxes = words.Where(w => w.BoundingBox.Width > 0 && w.BoundingBox.Height > 0)
                .Select(o => o.BoundingBox).ToList();

            if (images?.Any() == true)
            {
                bboxes.AddRange(images.Where(w => w.Bounds.Width > 0 && w.Bounds.Height > 0).Select(o => o.Bounds));
            }

            if (paths?.Any() == true)
            {
                foreach (var path in paths)
                {
                    var bbox = path.GetBoundingRectangle();
                    if (bbox.HasValue && bbox.Value.Width > 0 && bbox.Value.Height > 0)
                    {
                        bboxes.Add(bbox.Value);
                    }
                }
            }

            return GetWhitespaces(bboxes.Select(b => b.Normalise()), // normalise
                                  minWidth: minWidth,
                                  minHeight: minHeight,
                                  maxRectangleCount: maxRectangleCount,
                                  whitespaceFuzziness: whitespaceFuzziness,
                                  maxBoundQueueSize: maxBoundQueueSize);
        }

        /// <summary>
        /// Gets the cover of the background whitespace of a page in terms of maximal empty rectangles.
        /// </summary>
        /// <param name="boundingboxes">The list of obstacles' bounding boxes in the page.</param>
        /// <param name="minWidth">Lower bounds for the width of rectangles.</param>
        /// <param name="minHeight">Lower bounds for the height of rectangles.</param>
        /// <param name="maxRectangleCount">The maximum number of rectangles to find.</param>
        /// <param name="whitespaceFuzziness">Constant value to allow candidate whitespace rectangle to overlap the
        /// surrounding obstacles by some percent. Default value is 15%.</param>
        /// <param name="maxBoundQueueSize">The maximum size of the queue used in the algorithm.</param>
        /// <returns>The identified whitespace rectangles.</returns>
        public static IReadOnlyList<PdfRectangle> GetWhitespaces(IEnumerable<PdfRectangle> boundingboxes,
            double minWidth, double minHeight, int maxRectangleCount = 40, double whitespaceFuzziness = 0.15, int maxBoundQueueSize = 0)
        {
            if (!boundingboxes.Any()) return EmptyArray<PdfRectangle>.Instance;

            var obstacles = boundingboxes.Distinct(); // distinct
            var pageBound = GetBound(obstacles);
            return GetMaximalRectangles(pageBound,
                                        obstacles,
                                        minWidth: minWidth,
                                        minHeight: minHeight,
                                        maxRectangleCount: maxRectangleCount,
                                        whitespaceFuzziness: whitespaceFuzziness,
                                        maxBoundQueueSize: maxBoundQueueSize);
        }

        private static IReadOnlyList<PdfRectangle> GetMaximalRectangles(double[] bound,
            IEnumerable<PdfRectangle> obstacles, double minWidth, double minHeight, int maxRectangleCount,
            double whitespaceFuzziness, int maxBoundQueueSize)
        {
            HashSet<double[]> selected = new HashSet<double[]>();
            HashSet<QueueEntry> holdList = new HashSet<QueueEntry>();

            QuadTree<double[]> obstaclesTree = new QuadTree<double[]>(new PdfRectangle(bound[0], bound[1], bound[2], bound[3]),
                obstacles.Select(o => new double[] { o.Left, o.Bottom, o.Right, o.Top }), x => new PdfRectangle(x[0], x[1], x[2], x[3]));

            QueueEntries queueEntries = new QueueEntries(maxBoundQueueSize);
            queueEntries.Enqueue(new QueueEntry(bound, obstaclesTree, whitespaceFuzziness));

            while (queueEntries.Any())
            {
                var current = queueEntries.Dequeue();

                if (current.IsEmptyEnough(obstaclesTree))
                {
                    if (selected.Any(c => Inside(c, current.Bound))) continue;

                    // A check was added which impeded the algorithm from accepting
                    // rectangles which were not adjacent to an already accepted 
                    // rectangle, or to the border of the page.
                    if (!IsAdjacentToPageBounds(bound, current.Bound) &&        // NOT in contact to border page AND
                        !selected.Any(q => IsAdjacentTo(q, current.Bound)))     // NOT in contact to any already accepted rectangle
                    {
                        // In order to maintain the correctness of the algorithm, 
                        // rejected rectangles are put in a hold list. 
                        holdList.Add(current);
                        continue;
                    }

                    selected.Add(current.Bound);

                    if (selected.Count >= maxRectangleCount) return selected.Select(x => new PdfRectangle(x[0], x[1], x[2], x[3])).ToList();

                    obstaclesTree.Add(current.Bound);

                    // Each time a new rectangle is identified and accepted, this hold list 
                    // will be added back to the queue in case any of them will have become valid.
                    foreach (var hold in holdList)
                    {
                        queueEntries.Enqueue(hold);
                    }

                    // After a maximal rectangle has been found, it is added back to the list 
                    // of obstacles. Whenever a QueueEntry is dequeued, its list of obstacles 
                    // can be recomputed to include newly identified whitespace rectangles.
                    foreach (var overlapping in queueEntries)
                    {
                        if (OverlapsHard(current.Bound, overlapping.Bound))
                            overlapping.AddWhitespace(current.Bound);
                    }

                    continue;
                }

                var pivot = current.GetPivot();
                var b = current.Bound;

                if (b[2] > pivot[2] && (b[3] - b[1]) > minHeight && (b[2] - pivot[2]) > minWidth)
                {
                    var rRight = new double[] { pivot[2], b[1], b[2], b[3] };
                    queueEntries.Enqueue(new QueueEntry(rRight, current.Obstacles.Where(o => OverlapsHard(rRight, o)), whitespaceFuzziness));
                }

                if (b[0] < pivot[0] && (b[3] - b[1]) > minHeight && (pivot[0] - b[0]) > minWidth)
                {
                    var rLeft = new double[] { b[0], b[1], pivot[0], b[3] };
                    queueEntries.Enqueue(new QueueEntry(rLeft, current.Obstacles.Where(o => OverlapsHard(rLeft, o)), whitespaceFuzziness));
                }

                if (b[1] < pivot[1] && (pivot[1] - b[1]) > minHeight && (b[2] - b[0]) > minWidth)
                {
                    var rAbove = new double[] { b[0], b[1], b[2], pivot[1] };
                    queueEntries.Enqueue(new QueueEntry(rAbove, current.Obstacles.Where(o => OverlapsHard(rAbove, o)), whitespaceFuzziness));
                }

                if (b[3] > pivot[3] && (b[3] - pivot[3]) > minHeight && (b[2] - b[0]) > minWidth)
                {
                    double[] rBelow = new double[] { b[0], pivot[3], b[2], b[3] };
                    queueEntries.Enqueue(new QueueEntry(rBelow, current.Obstacles.Where(o => OverlapsHard(rBelow, o)), whitespaceFuzziness));
                }
            }

            return selected.Select(x => new PdfRectangle(x[0], x[1], x[2], x[3])).ToList();
        }
        private static bool OverlapsHard(double[] rectangle1, double[] rectangle2)
        {
            return rectangle1[0] < rectangle2[2] &&
                   rectangle2[0] < rectangle1[2] &&
                   rectangle1[3] > rectangle2[1] &&
                   rectangle2[3] > rectangle1[1];
        }

        private static bool IsAdjacentTo(double[] rectangle1, double[] rectangle2)
        {
            if (rectangle1[0] > rectangle2[2] ||
                rectangle2[0] > rectangle1[2] ||
                rectangle1[3] < rectangle2[1] ||
                rectangle2[3] < rectangle1[1])
            {
                return false;
            }

            return rectangle1[0] == rectangle2[2] ||
                   rectangle1[2] == rectangle2[0] ||
                   rectangle1[1] == rectangle2[3] ||
                   rectangle1[3] == rectangle2[1];
        }

        private static bool IsAdjacentToPageBounds(double[] pageBound, double[] rectangle)
        {
            return rectangle[1] == pageBound[1] ||
                   rectangle[3] == pageBound[3] ||
                   rectangle[0] == pageBound[0] ||
                   rectangle[2] == pageBound[2];
        }

        private static bool Inside(double[] rectangle1, double[] rectangle2)
        {
            return rectangle2[2] <= rectangle1[2] && rectangle2[0] >= rectangle1[0] &&
                   rectangle2[3] <= rectangle1[3] && rectangle2[1] >= rectangle1[1];
        }

        private static double[] GetBound(IEnumerable<PdfRectangle> obstacles)
        {
            return new double[]
            {
                obstacles.Min(b => b.Left),
                obstacles.Min(b => b.Bottom),
                obstacles.Max(b => b.Right),
                obstacles.Max(b => b.Top)
            };
        }

        #region Sorted Queue
        private class QueueEntries : SortedSet<QueueEntry>
        {
            private readonly int bound;

            public QueueEntries(int maximumBound)
            {
                bound = maximumBound;
            }

            public QueueEntry Dequeue()
            {
                var current = Max;
                Remove(current);
                return current;
            }

            public void Enqueue(QueueEntry queueEntry)
            {
                if (this.Contains(queueEntry)) return;
                if (bound > 0 && Count > bound)
                {
                    Remove(Min);
                }
                Add(queueEntry);
            }
        }

        private class QueueEntry : IComparable<QueueEntry>
        {
            private readonly double quality;
            private readonly double whitespaceFuzziness;

            public double[] Bound { get; }

            public List<double[]> Obstacles { get; }

            public QueueEntry(double[] bound, IEnumerable<double[]> obstacles, double whitespaceFuzziness)
            {
                Bound = bound;
                quality = ScoringFunction(Bound);
                Obstacles = obstacles.ToList();
                this.whitespaceFuzziness = whitespaceFuzziness;
            }

            private double[] Centroid(double[] bound)
            {
                return new double[] { (bound[0] + bound[2]) / 2.0, (bound[1] + bound[3]) / 2.0 };
            }

            public double[] GetPivot()
            {
                // find closest rectangle to centroid
                var bCentr = Centroid(Bound);
                double distance = double.MaxValue;
                double[] closest = Obstacles[0];
                foreach (var o in Obstacles)
                {
                    var oCentr = Centroid(o);
                    double dx = bCentr[0] - oCentr[0];
                    double dy = bCentr[1] - oCentr[1];
                    double currentDistance = dx * dx + dy * dy; // squred dist

                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                        closest = o;
                    }
                }

                return closest;
            }

            public bool IsEmptyEnough()
            {
                return Obstacles.Count == 0;
            }

            public bool IsEmptyEnough(QuadTree<double[]> pageObstacles)
            {
                if (IsEmptyEnough())
                {
                    return true;
                }

                double sum = 0;
                foreach (var obstacle in pageObstacles.GetObjectsIntersects(Bound[0], Bound[1], Bound[2], Bound[3], false))
                {
                    var intersect = Intersect(Bound, obstacle);
                    if (intersect == null) continue; //continue; // return false;

                    double minimumArea = MinimumOverlappingArea(obstacle, Bound, whitespaceFuzziness);
                    double intersectArea = (intersect[2] - intersect[0]) * (intersect[3] - intersect[1]);
                    if (intersectArea > minimumArea)
                    {
                        return false;
                    }
                    sum += intersectArea;
                }
                return sum < (Bound[2] - Bound[0]) * (Bound[3] - Bound[1]) * whitespaceFuzziness;
            }

            private bool IntersectsWith(double[] rectangle, double[] other)
            {
                if (rectangle[0] > other[2] || other[0] > rectangle[2])
                {
                    return false;
                }

                if (rectangle[3] < other[1] || other[3] < rectangle[1])
                {
                    return false;
                }

                return true;
            }


            /// <summary>
            /// Gets the <see cref="PdfRectangle"/> that is the intersection of two rectangles.
            /// <para>Only works for axis-aligned rectangles.</para>
            /// </summary>
            private double[] Intersect(double[] rectangle, double[] other)
            {
                if (!IntersectsWith(rectangle, other)) return null;
                return new double[]
                {
                    Math.Max(rectangle[0], other[0]),
                    Math.Max(rectangle[1], other[1]),
                    Math.Min(rectangle[2], other[2]),
                    Math.Min(rectangle[3], other[3])
                };
            }

            public override string ToString()
            {
                return "Q=" + quality.ToString("#0.0") + ", O=" + Obstacles.Count + ", " + Bound.ToString();
            }

            public void AddWhitespace(double[] rectangle)
            {
                if (Obstacles.Contains(rectangle)) return;
                Obstacles.Add(rectangle);
            }

            public int CompareTo(QueueEntry entry)
            {
                return quality.CompareTo(entry.quality);
            }

            public override bool Equals(object obj)
            {
                if (obj is QueueEntry entry)
                {
                    return Bound[0] == entry.Bound[0] &&
                           Bound[1] == entry.Bound[1] &&
                           Bound[2] == entry.Bound[2] &&
                           Bound[3] == entry.Bound[3] &&
                           Obstacles == entry.Obstacles;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (Bound[0], Bound[1],
                        Bound[2], Bound[3],
                        Obstacles).GetHashCode();
            }

            private static double MinimumOverlappingArea(double[] r1, double[] r2, double whitespaceFuzziness)
            {
                double area1 = (r1[2] - r1[0]) * (r1[3] - r1[1]);
                double area2 = (r2[2] - r2[0]) * (r2[3] - r2[1]);
                return Math.Min(area1, area2) * whitespaceFuzziness;
            }

            /// <summary>
            /// The scoring function Q(r) which is subsequently used to sort a priority queue.
            /// </summary>
            /// <param name="rectangle"></param>
            private static double ScoringFunction(double[] rectangle)
            {
                // As can be seen, tall rectangles are preferred. The trick while choosing this Q(r) was
                // to keep that preference while still allowing wide rectangles to be chosen. After having
                // experimented with quite a few variations, this simple function was considered a good
                // solution.

                double height = rectangle[3] - rectangle[1];
                double area = height * (rectangle[2] - rectangle[0]);

                return area * (height / 4.0);
            }
        }
        #endregion
    }
}
