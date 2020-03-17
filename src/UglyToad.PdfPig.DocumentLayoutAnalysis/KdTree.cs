namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Core;

    // for kd-tree with line segments, see https://stackoverflow.com/questions/14376679/how-to-represent-line-segments-in-kd-tree 

    /// <summary>
    /// 
    /// </summary>
    public class KdTree : KdTree<PdfPoint>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidates"></param>
        public KdTree(PdfPoint[] candidates) : base(candidates, p => p)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="distanceMeasure"></param>
        /// <param name="index"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public PdfPoint FindNearestNeighbour(PdfPoint pivot, Func<PdfPoint, PdfPoint, double> distanceMeasure, out int index, out double distance)
        {
            return FindNearestNeighbour(pivot, p => p, distanceMeasure, out index, out distance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="k"></param>
        /// <param name="distanceMeasure"></param>
        /// <returns></returns>
        public IReadOnlyList<(PdfPoint, int, double)> FindNearestNeighbours(PdfPoint pivot, int k, Func<PdfPoint, PdfPoint, double> distanceMeasure)
        {
            return FindNearestNeighbours(pivot, k, p => p, distanceMeasure);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KdTree<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly KdTreeNode<T> Root;

        /// <summary>
        /// 
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="candidatesPointFunc"></param>
        public KdTree(IReadOnlyList<T> candidates, Func<T, PdfPoint> candidatesPointFunc)
        {
            if (candidates == null || candidates.Count == 0)
            {
                throw new ArgumentException("KdTree(): candidates cannot be null or empty.", nameof(candidates));
            }

            Count = candidates.Count;
            Root = BuildTree(Enumerable.Range(0, candidates.Count).Zip(candidates, (e, p) => (e, candidatesPointFunc(p), p)).ToArray(), 0);
        }

        private KdTreeNode<T> BuildTree((int, PdfPoint, T)[] P, int depth)
        {
            if (P.Length == 0)
            {
                return null;
            }
            else if (P.Length == 1)
            {
                return new KdTreeLeaf<T>(P[0], depth);
            }

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
                return new KdTreeNode<T>(new KdTreeLeaf<T>(P[0], depth + 1), null, P[1], depth);
            }

            int median = P.Length / 2;

            KdTreeNode<T> vLeft = BuildTree(P.Take(median).ToArray(), depth + 1);
            KdTreeNode<T> vRight = BuildTree(P.Skip(median + 1).ToArray(), depth + 1);

            return new KdTreeNode<T>(vLeft, vRight, P[median], depth);
        }

        #region NN
        /// <summary>
        /// Get the nearest neighbour to the pivot element.
        /// Only returns 1 neighbour, even if points are equidistant.
        /// </summary>
        /// <param name="pivot">The element for which to find the nearest neighbour.</param>
        /// <param name="pivotPointFunc"></param>
        /// <param name="distanceMeasure"></param>
        /// <param name="index">The nearest neighbour's index (returns -1 if not found).</param>
        /// <param name="distance">The distance between the pivot and the nearest neighbour (returns <see cref="double.NaN"/> if not found).</param>
        /// <returns>The nearest neighbour's element.</returns>
        public T FindNearestNeighbour(T pivot, Func<T, PdfPoint> pivotPointFunc, Func<PdfPoint, PdfPoint, double> distanceMeasure, out int index, out double distance)
        {
            var result = FindNearestNeighbour(Root, pivot, pivotPointFunc, distanceMeasure);
            index = result.Item1 != null ? result.Item1.Index : -1;
            distance = result.Item2 ?? double.NaN;
            return result.Item1 != null ? result.Item1.Element : default;
        }

        private static (KdTreeNode<T>, double?) FindNearestNeighbour(KdTreeNode<T> node, T pivot, Func<T, PdfPoint> pivotPointFunc, Func<PdfPoint, PdfPoint, double> distance)
        {
            if (node == null)
            {
                return (null, null);
            }
            else if (node.IsLeaf)
            {
                if (node.Element.Equals(pivot))
                {
                    return (null, null);
                }
                return (node, distance(node.Value, pivotPointFunc(pivot)));
            }
            else
            {
                var point = pivotPointFunc(pivot);
                var currentNearestNode = node;
                var currentDistance = distance(node.Value, point);

                KdTreeNode<T> newNode = null;
                double? newDist = null;

                var pointValue = node.IsAxisCutX ? point.X : point.Y;

                if (pointValue < node.L)
                {
                    // start left
                    (newNode, newDist) = FindNearestNeighbour(node.LeftChild, pivot, pivotPointFunc, distance);

                    if (newDist.HasValue && newDist <= currentDistance && !newNode.Element.Equals(pivot))
                    {
                        currentDistance = newDist.Value;
                        currentNearestNode = newNode;
                    }

                    if (node.RightChild != null && pointValue + currentDistance >= node.L)
                    {
                        (newNode, newDist) = FindNearestNeighbour(node.RightChild, pivot, pivotPointFunc, distance);
                    }
                }
                else
                {
                    // start right
                    (newNode, newDist) = FindNearestNeighbour(node.RightChild, pivot, pivotPointFunc, distance);

                    if (newDist.HasValue && newDist <= currentDistance && !newNode.Element.Equals(pivot))
                    {
                        currentDistance = newDist.Value;
                        currentNearestNode = newNode;
                    }

                    if (node.LeftChild != null && pointValue - currentDistance <= node.L)
                    {
                        (newNode, newDist) = FindNearestNeighbour(node.LeftChild, pivot, pivotPointFunc, distance);
                    }
                }

                if (newDist.HasValue && newDist <= currentDistance && !newNode.Element.Equals(pivot))
                {
                    currentDistance = newDist.Value;
                    currentNearestNode = newNode;
                }

                return (currentNearestNode, currentDistance);
            }
        }
        #endregion

        #region k-NN
        /// <summary>
        /// Get the k nearest neighbours to the pivot element. If elements are equidistant, they are counted as one.
        /// Might return more than k neighbours if points are equidistant.
        /// <para>Use <see cref="FindNearestNeighbour(KdTreeNode{T}, T, Func{T, PdfPoint}, Func{PdfPoint, PdfPoint, double})"/> if only looking for the closest point.</para>
        /// </summary>
        /// <param name="pivot">The element for which to find the k nearest neighbours.</param>
        /// <param name="k">The number of neighbours to return. If elements are equidistant, they are counted as one.</param>
        /// <param name="pivotPointFunc"></param>
        /// <param name="distanceMeasure"></param>
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
            }
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
        /// 
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        public class KdTreeLeaf<Q> : KdTreeNode<Q>
        {
            /// <summary>
            /// 
            /// </summary>
            public override bool IsLeaf => true;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="point"></param>
            /// <param name="depth"></param>
            public KdTreeLeaf((int, PdfPoint, Q) point, int depth)
                : base(null, null, point, depth)
            { }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "Leaf->" + Value.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        public class KdTreeNode<Q>
        {
            /// <summary>
            /// Split value.
            /// </summary>
            public double L => IsAxisCutX ? Value.X : Value.Y;

            /// <summary>
            /// 
            /// </summary>
            public PdfPoint Value { get; }

            /// <summary>
            /// 
            /// </summary>
            public KdTreeNode<Q> LeftChild { get; internal set; }

            /// <summary>
            /// 
            /// </summary>
            public KdTreeNode<Q> RightChild { get; internal set; }

            /// <summary>
            /// 
            /// </summary>
            public Q Element { get; }

            /// <summary>
            /// True if this cuts with X axis, false if cuts with Y axis.
            /// </summary>
            public bool IsAxisCutX { get; }

            /// <summary>
            /// 
            /// </summary>
            public int Depth { get; }

            /// <summary>
            /// 
            /// </summary>
            public virtual bool IsLeaf => false;

            /// <summary>
            /// 
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="leftChild"></param>
            /// <param name="rightChild"></param>
            /// <param name="point"></param>
            /// <param name="depth"></param>
            public KdTreeNode(KdTreeNode<Q> leftChild, KdTreeNode<Q> rightChild, (int, PdfPoint, Q) point, int depth)
            {
                LeftChild = leftChild;
                RightChild = rightChild;
                Value = point.Item2;
                Element = point.Item3;
                Depth = depth;
                IsAxisCutX = depth % 2 == 0;
                Index = point.Item1;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public IEnumerable<KdTreeLeaf<Q>> GetLeaves()
            {
                var leaves = new List<KdTreeLeaf<Q>>();
                RecursiveGetLeaves(LeftChild, ref leaves);
                RecursiveGetLeaves(RightChild, ref leaves);
                return leaves;
            }

            private void RecursiveGetLeaves(KdTreeNode<Q> leaf, ref List<KdTreeLeaf<Q>> leaves)
            {
                if (leaf == null) return;
                if (leaf is KdTreeLeaf<Q> lLeaf)
                {
                    leaves.Add(lLeaf);
                }
                else
                {
                    RecursiveGetLeaves(leaf.LeftChild, ref leaves);
                    RecursiveGetLeaves(leaf.RightChild, ref leaves);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "Node->" + Value.ToString();
            }
        }
    }
}
