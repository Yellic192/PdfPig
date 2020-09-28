namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Geometry;

    /******************************************************************************
	Copyright (c) 2011 John McDonald and Gary Texmo

	This software is provided 'as-is', without any express or implied
	warranty. In no event will the authors be held liable for any damages
	arising from the use of this software.

	Permission is granted to anyone to use this software for any purpose,
	including commercial applications, and to alter it and redistribute it
	freely, subject to the following restrictions:

		1. The origin of this software must not be misrepresented; you must not
		claim that you wrote the original software. If you use this software
		in a product, an acknowledgment in the product documentation would be
		appreciated but is not required.

		2. Altered source versions must be plainly marked as such, and must not be
		misrepresented as being the original software.

		3. This notice may not be removed or altered from any source
		distribution.
	 ******************************************************************************/

    /*
	 * Original source code can be found here: https://sourceforge.net/projects/quadtree/
	 * Code modified for use with PdfPig.
	 */

    /// <summary>
    /// A QuadTree data structure that provides fast and efficient storage of objects in a world space.
    /// </summary>
    /// <typeparam name="T">Any object that can be transformed into a PdfRectangle.</typeparam>
    public class QuadTree<T> : ICollection<T>
    {
        #region Private Members
        private readonly Dictionary<T, QuadTreeObject<T>> wrappedDictionary = new Dictionary<T, QuadTreeObject<T>>();
        private readonly Func<T, PdfRectangle> transf;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a QuadTree for the specified area.
        /// </summary>
        /// <param name="x0">The bottom-left x coordinate of the area rectangle.</param>
        /// <param name="y0">The bottom-left y coordinate of the area rectangle.</param>
        /// <param name="x1">The top-right x coordinate of the area rectangle.</param>
        /// <param name="y1">The top-right y coordinate of the area rectangle.</param>
        /// <param name="elementsRectangleFunc">The function that converts an element into a PdfRectangle.
        /// After this first transformation, the resulting rectangle will be normalised to be axis-aligned.</param>
        public QuadTree(double x0, double y0, double x1, double y1, Func<T, PdfRectangle> elementsRectangleFunc)
        {
            RootQuad = new QuadTreeNode<T>(x0, y0, x1, y1);
            this.transf = elementsRectangleFunc;
        }

        /// <summary>
        /// Creates a QuadTree for the specified area.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        /// <param name="elementsRectangleFunc">The function that converts an element into a PdfRectangle.
        /// After this first transformation, the resulting rectangle will be normalised to be axis-aligned.</param>
        public QuadTree(PdfRectangle rect, Func<T, PdfRectangle> elementsRectangleFunc)
        {
            var normalised = rect.Normalise();
            RootQuad = new QuadTreeNode<T>(normalised.Left, normalised.Bottom, normalised.Right, normalised.Top);
            this.transf = elementsRectangleFunc;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a copy of the rectangle that bounds this QuadTree.
        /// </summary>
        public PdfRectangle QuadRect
        {
            get { return new PdfRectangle(RootQuad.QuadRect.Left, RootQuad.QuadRect.Bottom, RootQuad.QuadRect.Right, RootQuad.QuadRect.Top); }
        }

        /// <summary>
        /// Get the nearest neighbour to the pivot point.
        /// Only returns 1 neighbour, even if equidistant points are found.
        /// </summary>
        /// <param name="pivot">The point for which to find the nearest neighbour.</param>
        public T FindNearestNeighbour(PdfPoint pivot)
        {
            double dist = double.MaxValue;
            T element = default;
            RootQuad.FindNearestNeighbour(pivot, ref element, ref dist);
            return element;
        }

        /// <summary>
        /// Get the objects in this tree that intersect with the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to find objects in.</param>
        public List<T> GetObjectsIntersects(PdfRectangle rect)
        {
            var normalised = rect.Normalise();
            return GetObjectsIntersects(new QTRectangle(normalised.Left, normalised.Bottom, normalised.Right, normalised.Top));
        }

        /// <summary>
        /// Get the objects in this tree that intersect with the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to find objects in.</param>
        private List<T> GetObjectsIntersects(QTRectangle rect)
        {
            return RootQuad.GetObjectsIntersects(rect);
        }

        /// <summary>
        /// Get the objects in this tree that are contained inside the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to find objects in.</param>
        /// <param name="includeborder"></param>
        public List<T> GetObjectsContains(PdfRectangle rect, bool includeborder)
        {
            var normalised = rect.Normalise();
            return GetObjectsContains(new QTRectangle(normalised.Left, normalised.Bottom, normalised.Right, normalised.Top), includeborder);
        }

        /// <summary>
        /// Get the objects in this tree that are contained inside the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to find objects in.</param>
        /// <param name="includeborder"></param>
        private List<T> GetObjectsContains(QTRectangle rect, bool includeborder)
        {
            return RootQuad.GetObjectsContains(rect, includeborder);
        }

        /// <summary>
        /// Get all objects in this Quad, and it's children.
        /// </summary>
        public List<T> GetAllObjects()
        {
            return new List<T>(wrappedDictionary.Keys);
        }

        /// <summary>
        /// Moves the object in the tree
        /// </summary>
        /// <param name="item">The item that has moved</param>
        public bool Move(T item)
        {
            if (Contains(item))
            {
                RootQuad.Move(wrappedDictionary[item]);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region ICollection<T> Members
        ///<summary>
        ///Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///</summary>
        ///
        ///<param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(T item)
        {
            QuadTreeObject<T> wrappedObject = new QuadTreeObject<T>(item, transf);
            wrappedDictionary.Add(item, wrappedObject);
            RootQuad.Insert(wrappedObject);
        }

        ///<summary>
        ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///</summary>
        ///
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            wrappedDictionary.Clear();
            RootQuad.Clear();
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        ///</summary>
        ///
        ///<returns>
        ///true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        ///</returns>
        ///
        ///<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(T item)
        {
            return wrappedDictionary.ContainsKey(item);
        }

        ///<summary>
        ///Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        ///</summary>
        ///
        ///<param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        ///<param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        ///<exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
        ///<exception cref="T:System.ArgumentException"><paramref name="array" /> is multidimensional.-or-<paramref name="arrayIndex" /> is equal to or greater than the length of <paramref name="array" />.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type <typeparamref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            wrappedDictionary.Keys.CopyTo(array, arrayIndex);
        }

        ///<summary>
        ///Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///</summary>
        ///<returns>
        ///The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///</returns>
        public int Count
        {
            get { return wrappedDictionary.Count; }
        }

        ///<summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        ///</summary>
        ///
        ///<returns>
        ///true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        ///</returns>
        ///
        public bool IsReadOnly
        {
            get { return false; }
        }

        ///<summary>
        ///Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///</summary>
        ///
        ///<returns>
        ///true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///</returns>
        ///
        ///<param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(T item)
        {
            if (Contains(item))
            {
                RootQuad.Delete(wrappedDictionary[item], true);
                wrappedDictionary.Remove(item);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IEnumerable<T> and IEnumerable Members
        ///<summary>
        /// Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return wrappedDictionary.Keys.GetEnumerator();
        }

        ///<summary>
        /// Returns an enumerator that iterates through a collection.
        ///</summary>
        ///
        ///<returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        /// <summary>
        /// The top left child for this QuadTree, only usable in debug mode.
        /// </summary>
        public QuadTreeNode<T> RootQuad { get; }
    }


    /// <summary>
    /// Used internally to attach an Owner to each object stored in the QuadTree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class QuadTreeObject<T>
    {
        /// <summary>
        /// The wrapped data value
        /// </summary>
        public T Data
        {
            get;
            private set;
        }

        public QTRectangle Rect { get; }

        /// <summary>
        /// The QuadTreeNode that owns this object
        /// </summary>
        internal QuadTreeNode<T> Owner
        {
            get;
            set;
        }

        /// <summary>
        /// Wraps the data value
        /// </summary>
        /// <param name="data">The data value to wrap</param>
        /// <param name="transf"></param>
        public QuadTreeObject(T data, Func<T, PdfRectangle> transf)
        {
            Data = data;
            var bbox = transf(data).Normalise();
            Rect = new QTRectangle(bbox.Left, bbox.Bottom, bbox.Right, bbox.Top);
        }
    }

    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of objects in a world space.
    /// </summary>
    /// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
    public class QuadTreeNode<T>
    {
        #region Constants
        /// <summary>
        /// How many objects can exist in a QuadTree before it sub divides itself
        /// </summary>
        private const int MaxObjectsPerNode = 2;
        #endregion

        #region Private Members
        private List<QuadTreeObject<T>> objects = null;
        /// <summary>
        /// The area this QuadTree represents
        /// </summary>
        private QTRectangle rect;

        /// <summary>
        /// The parent of this quad
        /// </summary>
        private readonly QuadTreeNode<T> parent = null;

        private QuadTreeNode<T> childTL = null; // Top Left Child
        private QuadTreeNode<T> childTR = null; // Top Right Child
        private QuadTreeNode<T> childBL = null; // Bottom Left Child
        private QuadTreeNode<T> childBR = null; // Bottom Right Child
        #endregion

        #region Public Properties
        /// <summary>
        /// The area this QuadTree represents.
        /// </summary>
        internal QTRectangle QuadRect
        {
            get { return rect; }
        }

        /// <summary>
        /// The top left child for this QuadTree
        /// </summary>
        public QuadTreeNode<T> TopLeftChild
        {
            get { return childTL; }
        }

        /// <summary>
        /// The top right child for this QuadTree
        /// </summary>
        public QuadTreeNode<T> TopRightChild
        {
            get { return childTR; }
        }

        /// <summary>
        /// The bottom left child for this QuadTree
        /// </summary>
        public QuadTreeNode<T> BottomLeftChild
        {
            get { return childBL; }
        }

        /// <summary>
        /// The bottom right child for this QuadTree
        /// </summary>
        public QuadTreeNode<T> BottomRightChild
        {
            get { return childBR; }
        }

        /// <summary>
        /// This QuadTree's parent.
        /// </summary>
        public QuadTreeNode<T> Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// The objects contained in this QuadTree at it's level (ie, excludes children).
        /// </summary>
        internal List<QuadTreeObject<T>> Objects
        {
            get { return objects; }
        }

        /// <summary>
        /// How many total objects are contained within this QuadTree (ie, includes children).
        /// </summary>
        public int Count
        {
            get { return ObjectCount(); }
        }

        /// <summary>
        /// Returns true if this is a empty leaf node.
        /// </summary>
        public bool IsEmptyLeaf
        {
            get { return Count == 0 && childTL == null; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a QuadTree for the specified area.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        internal QuadTreeNode(QTRectangle rect)
        {
            this.rect = rect;
        }

        /// <summary>
        /// Creates a QuadTree for the specified area.
        /// </summary>
        /// <param name="x0">The bottom-left x coordinate of the area rectangle.</param>
        /// <param name="y0">The bottom-left y coordinate of the area rectangle.</param>
        /// <param name="x1">The top-right x coordinate of the area rectangle.</param>
        /// <param name="y1">The top-right y coordinate of the area rectangle.</param>
        public QuadTreeNode(double x0, double y0, double x1, double y1)
            : this(new QTRectangle(x0, y0, x1, y1))
        { }

        private QuadTreeNode(QuadTreeNode<T> parent, QTRectangle rect)
            : this(rect)
        {
            this.parent = parent;
        }
        #endregion

        #region Private Members
        /// <summary>
        /// Add an item to the object list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        private void Add(QuadTreeObject<T> item)
        {
            if (objects == null)
            {
                //m_objects = new List<T>();
                objects = new List<QuadTreeObject<T>>();
            }

            item.Owner = this;
            objects.Add(item);
        }

        /// <summary>
        /// Remove an item from the object list.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        private void Remove(QuadTreeObject<T> item)
        {
            if (objects != null)
            {
                int removeIndex = objects.IndexOf(item);
                if (removeIndex >= 0)
                {
                    objects[removeIndex] = objects[objects.Count - 1];
                    objects.RemoveAt(objects.Count - 1);
                }
            }
        }

        /// <summary>
        /// Get the total for all objects in this QuadTree, including children.
        /// </summary>
        /// <returns>The number of objects contained within this QuadTree and its children.</returns>
        private int ObjectCount()
        {
            int count = 0;

            // Add the objects at this level
            if (objects != null)
            {
                count += objects.Count;
            }

            // Add the objects that are contained in the children
            if (childTL != null)
            {
                count += childTL.ObjectCount();
                count += childTR.ObjectCount();
                count += childBL.ObjectCount();
                count += childBR.ObjectCount();
            }

            return count;
        }

        /// <summary>
        /// Subdivide this QuadTree and move it's children into the appropriate Quads where applicable.
        /// </summary>
        private void Subdivide()
        {
            // We've reached capacity, subdivide...
            double width = rect.Width / 2.0;
            double height = rect.Height / 2.0;
            double midX = rect.Left + width;
            double midY = rect.Bottom + height;

            childTL = new QuadTreeNode<T>(this, new QTRectangle(rect.Left, rect.Top - height, rect.Left + width, rect.Top));
            childTR = new QuadTreeNode<T>(this, new QTRectangle(midX, rect.Top - height, midX + width, rect.Top));
            childBL = new QuadTreeNode<T>(this, new QTRectangle(rect.Left, midY - height, rect.Left + width, midY));
            childBR = new QuadTreeNode<T>(this, new QTRectangle(midX, midY - height, midX + width, midY));

            // If they're completely contained by the quad, bump objects down
            for (int i = 0; i < objects.Count; i++)
            {
                QuadTreeNode<T> destTree = GetDestinationTree(objects[i]);
                if (destTree != this)
                {
                    // Insert to the appropriate tree, remove the object, and back up one in the loop
                    destTree.Insert(objects[i]);
                    Remove(objects[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Get the child Quad that would contain an object.
        /// </summary>
        /// <param name="item">The object to get a child for.</param>
        /// <returns></returns>
        private QuadTreeNode<T> GetDestinationTree(QuadTreeObject<T> item)
        {
            // If a child can't contain an object, it will live in this Quad

            if (childTL.QuadRect.Contains(item.Rect, true))
            {
                return childTL;
            }
            else if (childTR.QuadRect.Contains(item.Rect, true))
            {
                return childTR;
            }
            else if (childBL.QuadRect.Contains(item.Rect, true))
            {
                return childBL;
            }
            else if (childBR.QuadRect.Contains(item.Rect, true))
            {
                return childBR;
            }

            return this;
        }

        private void Relocate(QuadTreeObject<T> item)
        {
            // Are we still inside our parent?
            if (QuadRect.Contains(item.Rect, true))
            {
                // Good, have we moved inside any of our children?
                if (childTL != null)
                {
                    QuadTreeNode<T> dest = GetDestinationTree(item);
                    if (item.Owner != dest)
                    {
                        // Delete the item from this quad and add it to our child
                        // Note: Do NOT clean during this call, it can potentially delete our destination quad
                        QuadTreeNode<T> formerOwner = item.Owner;
                        Delete(item, false);
                        dest.Insert(item);

                        // Clean up ourselves
                        formerOwner.CleanUpwards();
                    }
                }
            }
            else
            {
                // We don't fit here anymore, move up, if we can
                parent?.Relocate(item);
            }
        }

        private void CleanUpwards()
        {
            if (childTL != null)
            {
                // If all the children are empty leaves, delete all the children
                if (childTL.IsEmptyLeaf &&
                    childTR.IsEmptyLeaf &&
                    childBL.IsEmptyLeaf &&
                    childBR.IsEmptyLeaf)
                {
                    childTL = null;
                    childTR = null;
                    childBL = null;
                    childBR = null;

                    if (parent != null && Count == 0)
                    {
                        parent.CleanUpwards();
                    }
                }
            }
            else
            {
                // I could be one of 4 empty leaves, tell my parent to clean up
                if (parent != null && Count == 0)
                {
                    parent.CleanUpwards();
                }
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Clears the QuadTree of all objects, including any objects living in its children.
        /// </summary>
        internal void Clear()
        {
            // Clear out the children, if we have any
            if (childTL != null)
            {
                childTL.Clear();
                childTR.Clear();
                childBL.Clear();
                childBR.Clear();
            }

            // Clear any objects at this level
            if (objects != null)
            {
                objects.Clear();
                objects = null;
            }

            // Set the children to null
            childTL = null;
            childTR = null;
            childBL = null;
            childBR = null;
        }

        /// <summary>
        /// Deletes an item from this QuadTree. If the object is removed causes this Quad to have no objects in its children, it's children will be removed as well.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="clean">Whether or not to clean the tree</param>
        internal void Delete(QuadTreeObject<T> item, bool clean)
        {
            if (item.Owner != null)
            {
                if (item.Owner == this)
                {
                    Remove(item);
                    if (clean)
                    {
                        CleanUpwards();
                    }
                }
                else
                {
                    item.Owner.Delete(item, clean);
                }
            }
        }

        /// <summary>
        /// Insert an item into this QuadTree object.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        internal void Insert(QuadTreeObject<T> item)
        {
            // If this quad doesn't contain the items rectangle, do nothing, unless we are the root
            if (!rect.Contains(item.Rect, true))
            {
                System.Diagnostics.Debug.Assert(parent == null, "We are not the root, and this object doesn't fit here. How did we get here?");
                if (parent == null)
                {
                    // This object is outside of the QuadTree bounds, we should add it at the root level
                    Add(item);
                }
                else
                {
                    return;
                }
            }

            if (objects == null ||
                (childTL == null && objects.Count + 1 <= MaxObjectsPerNode))
            {
                // If there's room to add the object, just add it
                Add(item);
            }
            else
            {
                // No quads, create them and bump objects down where appropriate
                if (childTL == null)
                {
                    Subdivide();
                }

                // Find out which tree this object should go in and add it there
                QuadTreeNode<T> destTree = GetDestinationTree(item);
                if (destTree == this)
                {
                    Add(item);
                }
                else
                {
                    destTree.Insert(item);
                }
            }
        }

        /// <summary>
        /// Get the objects in this tree that intersect with the specified rectangle.
        /// </summary>
        /// <param name="searchRect">The rectangle to find objects in.</param>
        internal List<T> GetObjectsIntersects(QTRectangle searchRect)
        {
            List<T> results = new List<T>();
            GetObjectsIntersects(searchRect, ref results);
            return results;
        }

        internal void FindNearestNeighbour(PdfPoint point, ref T element, ref double distance)
        {
            if (point.X < QuadRect.X0 - distance ||
                point.X > QuadRect.X1 + distance ||
                point.Y < QuadRect.Y0 - distance ||
                point.Y > QuadRect.Y1 + distance)
            {
                // this QuadRect cannot contain a point that is closer
                return;
            }

            if (Objects != null)
            {
                foreach (var o in Objects)
                {
                    var centroid = new PdfPoint((o.Rect.X0 + o.Rect.X1) / 2.0, (o.Rect.Y0 + o.Rect.Y1) / 2.0);
                    var currentDist = Distances.Euclidean(point, centroid);
                    if (currentDist < distance)
                    {
                        distance = currentDist;
                        element = o.Data;
                    }
                }
            }

            if (childTL != null)
            {
                if (point.X > (QuadRect.X0 + QuadRect.X1) / 2)
                {
                    // start right
                    if (point.Y > (QuadRect.Y0 + QuadRect.Y1) / 2)
                    {
                        // start top
                        childTR.FindNearestNeighbour(point, ref element, ref distance);
                        childTL.FindNearestNeighbour(point, ref element, ref distance);
                        childBR.FindNearestNeighbour(point, ref element, ref distance);
                        childBL.FindNearestNeighbour(point, ref element, ref distance);
                    }
                    else
                    {
                        // start bottom
                        childBR.FindNearestNeighbour(point, ref element, ref distance);
                        childBL.FindNearestNeighbour(point, ref element, ref distance);
                        childTR.FindNearestNeighbour(point, ref element, ref distance);
                        childTL.FindNearestNeighbour(point, ref element, ref distance);
                    }
                }
                else
                {
                    // start left
                    if (point.Y > (QuadRect.Y0 + QuadRect.Y1) / 2)
                    {
                        // start top
                        childTL.FindNearestNeighbour(point, ref element, ref distance);
                        childTR.FindNearestNeighbour(point, ref element, ref distance);
                        childBL.FindNearestNeighbour(point, ref element, ref distance);
                        childBR.FindNearestNeighbour(point, ref element, ref distance);
                    }
                    else
                    {
                        // start bottom
                        childBL.FindNearestNeighbour(point, ref element, ref distance);
                        childBR.FindNearestNeighbour(point, ref element, ref distance);
                        childTL.FindNearestNeighbour(point, ref element, ref distance);
                        childTR.FindNearestNeighbour(point, ref element, ref distance);
                    }
                }
            }
        }

        /// <summary>
        /// Get the objects in this tree that intersect with the specified rectangle.
        /// </summary>
        /// <param name="searchRect">The rectangle to find objects in.</param>
        /// <param name="results">A reference to a list that will be populated with the results.</param>
        internal void GetObjectsIntersects(QTRectangle searchRect, ref List<T> results)
        {
            // We can't do anything if the results list doesn't exist
            if (results != null)
            {
                if (searchRect.Contains(this.rect, true))
                {
                    // If the search area completely contains this quad, just get every object this quad and all it's children have
                    GetAllObjects(ref results);
                }
                else if (searchRect.Intersects(this.rect))
                {
                    // Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle
                    if (objects != null)
                    {
                        for (int i = 0; i < objects.Count; i++)
                        {
                            if (searchRect.Intersects(objects[i].Rect))
                            {
                                results.Add(objects[i].Data);
                            }
                        }
                    }

                    // Get the objects for the search rectangle from the children
                    if (childTL != null)
                    {
                        childTL.GetObjectsIntersects(searchRect, ref results);
                        childTR.GetObjectsIntersects(searchRect, ref results);
                        childBL.GetObjectsIntersects(searchRect, ref results);
                        childBR.GetObjectsIntersects(searchRect, ref results);
                    }
                }
            }
        }

        /// <summary>
        /// Get the objects in this tree that are contained inside the specified rectangle.
        /// </summary>
        /// <param name="searchRect">The rectangle to find objects in.</param>
        /// <param name="includeBorder"></param>
        internal List<T> GetObjectsContains(QTRectangle searchRect, bool includeBorder)
        {
            List<T> results = new List<T>();
            GetObjectsContains(searchRect, includeBorder, ref results);
            return results;
        }

        /// <summary>
        /// Get the objects in this tree that are contained inside the specified rectangle.
        /// </summary>
        /// <param name="searchRect">The rectangle to find objects in.</param>
        /// <param name="includeBorder"></param>
        /// <param name="results">A reference to a list that will be populated with the results.</param>
        internal void GetObjectsContains(QTRectangle searchRect, bool includeBorder, ref List<T> results)
        {
            // We can't do anything if the results list doesn't exist
            if (results != null)
            {
                if (searchRect.Contains(this.rect, includeBorder))
                {
                    // If the search area completely contains this quad, just get every object this quad and all it's children have
                    GetAllObjects(ref results);
                }
                else if (searchRect.Intersects(this.rect))
                {
                    // Otherwise, if the quad isn't fully contained, only add objects that are inside the search rectangle
                    if (objects != null)
                    {
                        for (int i = 0; i < objects.Count; i++)
                        {
                            if (searchRect.Contains(objects[i].Rect, includeBorder))
                            {
                                results.Add(objects[i].Data);
                            }
                        }
                    }

                    // Get the objects for the search rectangle from the children
                    if (childTL != null)
                    {
                        childTL.GetObjectsContains(searchRect, includeBorder, ref results);
                        childTR.GetObjectsContains(searchRect, includeBorder, ref results);
                        childBL.GetObjectsContains(searchRect, includeBorder, ref results);
                        childBR.GetObjectsContains(searchRect, includeBorder, ref results);
                    }
                }
            }
        }

        /// <summary>
        /// Get all objects in this Quad, and it's children.
        /// </summary>
        /// <param name="results">A reference to a list in which to store the objects.</param>
        internal void GetAllObjects(ref List<T> results)
        {
            // If this Quad has objects, add them
            if (objects != null)
            {
                foreach (QuadTreeObject<T> qto in objects)
                {
                    results.Add(qto.Data);
                }
            }

            // If we have children, get their objects too
            if (childTL != null)
            {
                childTL.GetAllObjects(ref results);
                childTR.GetAllObjects(ref results);
                childBL.GetAllObjects(ref results);
                childBR.GetAllObjects(ref results);
            }
        }

        /// <summary>
        /// Moves the QuadTree object in the tree
        /// </summary>
        /// <param name="item">The item that has moved</param>
        internal void Move(QuadTreeObject<T> item)
        {
            if (item.Owner != null)
            {
                item.Owner.Relocate(item);
            }
            else
            {
                Relocate(item);
            }
        }
        #endregion
    }

    /// <summary>
    /// An axis-aligned QuadTree rectangle/area.
    /// </summary>
    internal struct QTRectangle
    {
        /// <summary>
        /// Left coordinate.
        /// </summary>
        public double X0 { get; }

        /// <summary>
        /// Bottom coordinate.
        /// </summary>
        public double Y0 { get; }

        /// <summary>
        /// Right coordinate.
        /// </summary>
        public double X1 { get; }

        /// <summary>
        /// Top coordinate.
        /// </summary>
        public double Y1 { get; }

        public double Left => X0;

        public double Bottom => Y0;

        public double Right => X1;

        public double Top => Y1;

        public double Width { get; }

        public double Height { get; }

        /// <summary>
        /// Create a new axis-aligned QuadTree rectangle.
        /// </summary>
        /// <param name="x0">Left</param>
        /// <param name="y0">Bottom</param>
        /// <param name="x1">Right</param>
        /// <param name="y1">Top</param>
        public QTRectangle(double x0, double y0, double x1, double y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
            Width = X1 - X0;
            Height = Y1 - Y0;
        }

        public bool Contains(QTRectangle other, bool includeBorder)
        {
            if (includeBorder)
            {
                return other.Left >= Left &&
                       other.Right <= Right &&
                       other.Bottom >= Bottom &&
                       other.Top <= Top;
            }
            else
            {
                return other.Left > Left &&
                       other.Right < Right &&
                       other.Bottom > Bottom &&
                       other.Top < Top;
            }
        }

        public bool Intersects(QTRectangle other)
        {
            if (Left > other.Right || other.Left > Right)
            {
                return false;
            }

            if (Top < other.Bottom || other.Top < Bottom)
            {
                return false;
            }

            return true;
        }
    }
}
