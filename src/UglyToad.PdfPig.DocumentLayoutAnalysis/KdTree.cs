namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;

    // for kd-tree with line segments, see https://stackoverflow.com/questions/14376679/how-to-represent-line-segments-in-kd-tree 

    /// <summary>
    /// K-D tree data structure of <see cref="PdfPoint"/>.
    /// </summary>
    public class KdTree : KdTree<PdfPoint>
    {
        /// <summary>
        /// K-D tree data structure of <see cref="PdfPoint"/>.
        /// </summary>
        /// <param name="points">The points used to build the tree.</param>
        public KdTree(PdfPoint[] points) : base(points, p => p)
        { }

        /// <summary>
        /// Get the nearest neighbour to the pivot point.
        /// Only returns 1 neighbour, even if equidistant points are found.
        /// </summary>
        /// <param name="pivot">The point for which to find the nearest neighbour.</param>
        /// <param name="distanceMeasure">The distance measure used, e.g. the Euclidian distance.</param>
        /// <param name="index">The nearest neighbour's index (returns -1 if not found).</param>
        /// <param name="distance">The distance between the pivot and the nearest neighbour (returns <see cref="double.NaN"/> if not found).</param>
        /// <returns>The nearest neighbour's point.</returns>
        public PdfPoint FindNearestNeighbour(PdfPoint pivot, Func<PdfPoint, PdfPoint, double> distanceMeasure, out int index, out double distance)
        {
            return FindNearestNeighbour(pivot, p => p, distanceMeasure, out index, out distance);
        }

        /// <summary>
        /// Get the k nearest neighbours to the pivot point.
        /// Might return more than k neighbours if points are equidistant.
        /// <para>Use <see cref="FindNearestNeighbour(PdfPoint, Func{PdfPoint, PdfPoint, double}, out int, out double)"/> if only looking for the (single) closest point.</para>
        /// </summary>
        /// <param name="pivot">The point for which to find the nearest neighbour.</param>
        /// <param name="k">The number of neighbours to return. Might return more than k neighbours if points are equidistant.</param>
        /// <param name="distanceMeasure">The distance measure used, e.g. the Euclidian distance.</param>
        /// <returns>Returns a list of tuples of the k nearest neighbours. Tuples are (element, index, distance).</returns>
        public IReadOnlyList<(PdfPoint, int, double)> FindNearestNeighbours(PdfPoint pivot, int k, Func<PdfPoint, PdfPoint, double> distanceMeasure)
        {
            return FindNearestNeighbours(pivot, k, p => p, distanceMeasure);
        }
    }

    /// <summary>
    /// K-D tree data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KdTree<T>
    {
        /// <summary>
        /// The root of the tree.
        /// </summary>
        public readonly KdTreeNode<T> Root;

        private KdTreeNode<T>[] heap;

        private (int, PdfPoint, T)[] orig;

        /// <summary>
        /// Number of elements in the tree.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// K-D tree data structure.
        /// </summary>
        /// <param name="elements">The elements used to build the tree.</param>
        /// <param name="elementsPointFunc">The function that converts the candidate elements into a <see cref="PdfPoint"/>.</param>
        public KdTree(IReadOnlyList<T> elements, Func<T, PdfPoint> elementsPointFunc)
        {
            if (elements == null || elements.Count == 0)
            {
                throw new ArgumentException("KdTree(): candidates cannot be null or empty.", nameof(elements));
            }

            Count = elements.Count;
            heap = new KdTreeNode<T>[2 * Count + 2]; // wrong, should be n
            orig = Enumerable.Range(0, elements.Count).Zip(elements, (e, p) => (e, elementsPointFunc(p), p)).ToArray();
            BuildTree(1, 0);
        }

        private void Swap(int i, int j)
        {
            var tempNode = heap[i];
            heap[i] = heap[j];
            heap[j] = tempNode;
        }

        private void heapify(int index, int depth)
        {
            int parent = (int)Math.Floor((index - 1) / 2.0);

            if (depth % 2 == 0)
            {
                if (parent >= 0 && heap[index].Value.X > heap[parent].Value.X)
                {
                    Swap(index, parent);
                    heapify(parent, depth - 1);
                }
            }
            else
            {
                if (parent >= 0 && heap[index].Value.Y > heap[parent].Value.Y)
                {
                    Swap(index, parent);
                    heapify(parent, depth - 1);
                }
            }
        }

        private void BuildTree((int, PdfPoint, T)[] P, int depth, int heapIndex)
        {
            if (P.Length == 1)
            {
                heap[heapIndex] = new KdTreeNode<T>(P[0], depth) { IsLeaf = true };
                heapify(heapIndex, depth);
            }
            else if (P.Length > 1)
            {
                if (depth % 2 == 0)
                {
                    Array.Sort(P, (p0, p1) => p0.Item2.X.CompareTo(p1.Item2.X));
                }
                else
                {
                    Array.Sort(P, (p0, p1) => p0.Item2.Y.CompareTo(p1.Item2.Y));
                }

                if (P.Length == 2)
                {
                    heap[heapIndex] = new KdTreeNode<T>(P[1], depth);
                    heap[2 * heapIndex + 1] = new KdTreeNode<T>(P[0], depth + 1) { IsLeaf = true };

                    heapify(heapIndex, depth);
                }
                else
                {
                    int median = P.Length / 2;
                    heap[heapIndex] = new KdTreeNode<T>(P[median], depth);
                    heapify(heapIndex, depth);

                    BuildTree(P.Take(median).ToArray(), depth + 1, 2 * heapIndex + 1);      // left
                    BuildTree(P.Skip(median + 1).ToArray(), depth + 1, 2 * heapIndex + 2);  // right
                }
            }
        }

        #region NN
        /// <summary>
        /// Get the nearest neighbour to the pivot element.
        /// Only returns 1 neighbour, even if equidistant points are found.
        /// </summary>
        /// <param name="pivot">The element for which to find the nearest neighbour.</param>
        /// <param name="pivotPointFunc">The function that converts the pivot element into a <see cref="PdfPoint"/>.</param>
        /// <param name="distanceMeasure">The distance measure used, e.g. the Euclidian distance.</param>
        /// <param name="index">The nearest neighbour's index (returns -1 if not found).</param>
        /// <param name="distance">The distance between the pivot and the nearest neighbour (returns <see cref="double.NaN"/> if not found).</param>
        /// <returns>The nearest neighbour's element.</returns>
        public T FindNearestNeighbour(T pivot, Func<T, PdfPoint> pivotPointFunc, Func<PdfPoint, PdfPoint, double> distanceMeasure, out int index, out double distance)
        {
            var result = FindNearestNeighbour(0, pivot, pivotPointFunc, distanceMeasure);
            index = result.Item1.HasValue ? heap[result.Item1.Value].Index : -1;
            distance = result.Item2 ?? double.NaN;
            return result.Item1.HasValue ? heap[result.Item1.Value].Element : default;
        }

        private (int?, double?) FindNearestNeighbour(int node, T pivot, Func<T, PdfPoint> pivotPointFunc, Func<PdfPoint, PdfPoint, double> distance)
        {
            if (heap[node] is null)
            {
                return (null, null);
            }
            else if (heap[node].IsLeaf)
            {
                if (heap[node].Element.Equals(pivot))
                {
                    return (null, null);
                }
                return (node, distance(heap[node].Value, pivotPointFunc(pivot)));
            }
            else
            {
                var point = pivotPointFunc(pivot);
                var currentNearestNode = node;
                var currentDistance = distance(heap[node].Value, point);

                int? newNode = null;
                double? newDist = null;

                var pointValue = heap[node].IsAxisCutX ? point.X : point.Y;

                if (pointValue < heap[node].L)
                {
                    // start left
                    (newNode, newDist) = FindNearestNeighbour(node * 2 + 1, pivot, pivotPointFunc, distance);

                    if (newDist.HasValue && newDist <= currentDistance && !heap[newNode.Value].Element.Equals(pivot))
                    {
                        currentDistance = newDist.Value;
                        currentNearestNode = newNode.Value;
                    }

                    if (node * 2 + 2 < heap.Length && pointValue + currentDistance >= heap[node].L)
                    {
                        (newNode, newDist) = FindNearestNeighbour(node * 2 + 2, pivot, pivotPointFunc, distance);
                    }
                }
                else
                {
                    // start right
                    (newNode, newDist) = FindNearestNeighbour(node * 2 + 2, pivot, pivotPointFunc, distance);

                    if (newDist.HasValue && newDist <= currentDistance && !heap[newNode.Value].Element.Equals(pivot))
                    {
                        currentDistance = newDist.Value;
                        currentNearestNode = newNode.Value;
                    }

                    if (node * 2 + 1 < heap.Length && pointValue - currentDistance <= heap[node].L)
                    {
                        (newNode, newDist) = FindNearestNeighbour(node * 2 + 1, pivot, pivotPointFunc, distance);
                    }
                }

                if (newDist.HasValue && newDist <= currentDistance && !heap[newNode.Value].Element.Equals(pivot))
                {
                    currentDistance = newDist.Value;
                    currentNearestNode = newNode.Value;
                }

                return (currentNearestNode, currentDistance);
            }
        }
        #endregion

        #region k-NN
        /// <summary>
        /// Get the k nearest neighbours to the pivot element.
        /// Might return more than k neighbours if points are equidistant.
        /// <para>Use XXXXXXXXXX if only looking for the (single) closest point.</para>
        /// </summary>
        /// <param name="pivot">The element for which to find the k nearest neighbours.</param>
        /// <param name="k">The number of neighbours to return. Might return more than k neighbours if points are equidistant.</param>
        /// <param name="pivotPointFunc">The function that converts the pivot element into a <see cref="PdfPoint"/>.</param>
        /// <param name="distanceMeasure">The distance measure used, e.g. the Euclidian distance.</param>
        /// <returns>Returns a list of tuples of the k nearest neighbours. Tuples are (element, index, distance).</returns>
        public IReadOnlyList<(T, int, double)> FindNearestNeighbours(T pivot, int k, Func<T, PdfPoint> pivotPointFunc, Func<PdfPoint, PdfPoint, double> distanceMeasure)
        {
            var kdTreeNodes = new KNearestNeighboursQueue(k);
            FindNearestNeighbours(Root, pivot, k, pivotPointFunc, distanceMeasure, kdTreeNodes);
            return kdTreeNodes.SelectMany(n => n.Value.Select(e => (e.Element, e.Index, n.Key))).ToList();
        }

        private static (KdTreeNode<T>, double) FindNearestNeighbours(KdTreeNode<T> node, T pivot, int k,
            Func<T, PdfPoint> pivotPointFunc, Func<PdfPoint, PdfPoint, double> distance, KNearestNeighboursQueue queue)
        {
            throw new Exception();
            /*
            if (node == null)
            {
                return (null, double.NaN);
            }
            else if (node.IsLeaf)
            {
                if (node.Element.Equals(pivot))
                {
                    return (null, double.NaN);
                }

                var currentDistance = distance(node.Value, pivotPointFunc(pivot));
                var currentNearestNode = node;

                if (!queue.IsFull || currentDistance <= queue.LastDistance)
                {
                    queue.Add(currentDistance, currentNearestNode);
                    currentDistance = queue.LastDistance;
                    currentNearestNode = queue.LastElement;
                }

                return (currentNearestNode, currentDistance);
            }
            else
            {
                var point = pivotPointFunc(pivot);
                var currentNearestNode = node;
                var currentDistance = distance(node.Value, point);
                if (!queue.IsFull || currentDistance <= queue.LastDistance)
                {
                    queue.Add(currentDistance, currentNearestNode);
                    currentDistance = queue.LastDistance;
                    currentNearestNode = queue.LastElement;
                }

                KdTreeNode<T> newNode = null;
                double newDist = double.NaN;

                var pointValue = node.IsAxisCutX ? point.X : point.Y;

                if (pointValue < node.L)
                {
                    // start left
                    (newNode, newDist) = FindNearestNeighbours(node.LeftChild, pivot, k, pivotPointFunc, distance, queue);

                    if (!double.IsNaN(newDist) && newDist <= currentDistance && !newNode.Element.Equals(pivot))
                    {
                        queue.Add(newDist, newNode);
                        currentDistance = queue.LastDistance;
                        currentNearestNode = queue.LastElement;
                    }

                    if (node.RightChild != null && pointValue + currentDistance >= node.L)
                    {
                        (newNode, newDist) = FindNearestNeighbours(node.RightChild, pivot, k, pivotPointFunc, distance, queue);
                    }
                }
                else
                {
                    // start right
                    (newNode, newDist) = FindNearestNeighbours(node.RightChild, pivot, k, pivotPointFunc, distance, queue);

                    if (!double.IsNaN(newDist) && newDist <= currentDistance && !newNode.Element.Equals(pivot))
                    {
                        queue.Add(newDist, newNode);
                        currentDistance = queue.LastDistance;
                        currentNearestNode = queue.LastElement;
                    }

                    if (node.LeftChild != null && pointValue - currentDistance <= node.L)
                    {
                        (newNode, newDist) = FindNearestNeighbours(node.LeftChild, pivot, k, pivotPointFunc, distance, queue);
                    }
                }

                if (!double.IsNaN(newDist) && newDist <= currentDistance && !newNode.Element.Equals(pivot))
                {
                    queue.Add(newDist, newNode);
                    currentDistance = queue.LastDistance;
                    currentNearestNode = queue.LastElement;
                }

                return (currentNearestNode, currentDistance);
            }*/
        }

        private class KNearestNeighboursQueue : SortedList<double, HashSet<KdTreeNode<T>>>
        {
            public readonly int K;

            public KdTreeNode<T> LastElement { get; private set; }

            public double LastDistance { get; private set; }

            public bool IsFull => Count >= K;

            public KNearestNeighboursQueue(int k) : base(k)
            {
                K = k;
                LastDistance = double.PositiveInfinity;
            }

            public void Add(double key, KdTreeNode<T> value)
            {
                if (key > LastDistance && IsFull)
                {
                    return;
                }

                if (!ContainsKey(key))
                {
                    base.Add(key, new HashSet<KdTreeNode<T>>());
                    if (Count > K)
                    {
                        RemoveAt(Count - 1);
                    }
                }

                if (this[key].Add(value))
                {
                    var last = this.Last();
                    LastElement = last.Value.Last();
                    LastDistance = last.Key;
                }
            }
        }
        #endregion

        /// <summary>
        /// K-D tree node.
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        public class KdTreeNode<Q>
        {
            /// <summary>
            /// Split value (X or Y axis).
            /// </summary>
            public double L => IsAxisCutX ? Value.X : Value.Y;

            /// <summary>
            /// Split point.
            /// </summary>
            public PdfPoint Value { get; }

            /// <summary>
            /// The node's element.
            /// </summary>
            public Q Element { get; }

            /// <summary>
            /// True if this cuts with X axis, false if cuts with Y axis.
            /// </summary>
            public bool IsAxisCutX { get; }

            /// <summary>
            /// The element's depth in the tree.
            /// </summary>
            public int Depth { get; }

            /// <summary>
            /// Return true if leaf.
            /// </summary>
            public bool IsLeaf { get; set; }

            /// <summary>
            /// The index of the element in the original array.
            /// </summary>
            public int Index { get; }

            internal KdTreeNode((int, PdfPoint, Q) point, int depth)
            {
                Value = point.Item2;
                Element = point.Item3;
                Depth = depth;
                IsAxisCutX = depth % 2 == 0;
                Index = point.Item1;
                IsLeaf = false;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return "Node->" + Value.ToString();
            }
        }
    }
}
