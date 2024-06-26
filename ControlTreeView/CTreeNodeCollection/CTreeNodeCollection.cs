﻿using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using static ControlTreeView.CTreeView;

namespace ControlTreeView
{
    public partial class CTreeNodeCollection
    {
        #region Properties
        private INodeContainer owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instanse of CTreeNodeCollection assigned to specified owner.
        /// </summary>
        /// <param name="owner">CTreeNode or CTreeView.</param>
        internal CTreeNodeCollection(INodeContainer owner)
        {
            this.owner = owner;
        }
        #endregion

        #region ParentCTreeView
        /// <summary>
        /// Gets the parent CTreeView that this collection is assigned to.
        /// </summary>
        internal CTreeView ParentCTreeView
        {
            get { return owner.OwnerCTreeView; }
        }
        #endregion

        #region BeginUpdateCTreeView, EndUpdateCTreeView
        private void BeginUpdateCTreeView()
        {
            if (ParentCTreeView != null) ParentCTreeView.BeginUpdate();
        }

        private void EndUpdateCTreeView()
        {
            if (ParentCTreeView != null) ParentCTreeView.EndUpdate();
        }
        #endregion

        #region InsertItem, RemoveItem, SetItem, ClearItems
        /// <summary>
        /// Inserts an element into the Collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        protected override void InsertItem(int index, CTreeNode item)
        {
            if (item.Parent != null)
            {
                throw new ArgumentException("The node is currently assigned to INodeContainer.");//item.OwnerCollection.Remove(item);
            }
            else
            {
                base.InsertItem(index, item);
                //item.OwnerCollection = this;

                if (owner is CTreeNode)
                {
                    CTreeNode ownerNode = (CTreeNode)owner;
                    item.ParentNode = ownerNode;
                    item.Level = ownerNode.Level + 1;

                    if (!(ownerNode.IsExpanded && ownerNode.Visible))
                        item.Visible = false;
                }

                if (ParentCTreeView != null)
                {
                    item.OwnerCTreeView = ParentCTreeView;

                    ParentCTreeView.SuspendLayout();
                    item.TraverseNodes(node => { ParentCTreeView.Controls.Add(node.Control); });
                    ParentCTreeView.ResumeLayout(false);

                    ParentCTreeView.Recalculate();
                }
            }
        }

        /// <summary>
        /// Removes the element at the specified index of the Collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            //CTreeNode removedNode = this[index];
            this[index].IsSelected = false;
            this[index].ParentNode = null;
            //this[index].OwnerCollection = null;

            if (ParentCTreeView != null)
            {
                ParentCTreeView.SuspendLayout();
                this[index].TraverseNodes(eachNode => { ParentCTreeView.Controls.Remove(eachNode.Control); });
                ParentCTreeView.ResumeLayout(false);

                this[index].OwnerCTreeView = null;
            }

            this[index].Level   = 0;
            this[index].Visible = true;

            base.RemoveItem(index);

            if (ParentCTreeView != null)
            {
                ParentCTreeView.Recalculate();
            }
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        protected override void SetItem(int index, CTreeNode item)
        {
            //CTreeView.SuspendLayout();
            BeginUpdateCTreeView();
            RemoveItem(index);//?
            InsertItem(index, item);//?
            //base.SetItem(index, item);
            EndUpdateCTreeView();
            //CTreeView.ResumeLayout(false);
        }

        /// <summary>
        /// Removes all elements from the Collection.
        /// </summary>
        protected override void ClearItems()
        {
            BeginUpdateCTreeView();
            foreach (CTreeNode childNode in this)
            {
                childNode.IsSelected = false;
                childNode.ParentNode = null;
                //childNode.OwnerCollection = null;
                childNode.Level = 0;
                childNode.Visible = true;
            }

            if (ParentCTreeView != null)
            {
                ParentCTreeView.SuspendLayout();

                foreach (CTreeNode childNode in this)
                {
                    childNode.TraverseNodes(eachNode => { ParentCTreeView.Controls.Remove(eachNode.Control); });
                    childNode.OwnerCTreeView = null;
                }

                ParentCTreeView.ResumeLayout(false);
                ParentCTreeView.Recalculate();//?
            }

            base.ClearItems();
            EndUpdateCTreeView();
        }
        #endregion

        #region GetLines
        /// <summary>
        /// Get the list includes common line and lines for nodes of this CTreeNodeCollection.
        /// </summary>
        /// <param name="LC">Delegates for Lines and PlusMinus coordinates calculus</param>
        /// <remarks>
        /// LC is a struct that contains Functions to calculate node lines and PlusMinus address.
        /// Here we use commonLineCalc and childLineCalc functions to calculate lines
        /// for each node in the collection.
        /// </remarks>
        /// <returns>List of lines of this nodes collection.</returns>
        
        ///// <param name="commonLineCalc">Function that calculates common line of nodes collection.</param>
        ///// <param name="childLineCalc">Function that calculates line of each node in collection.</param>
        //internal List<CTreeNode.Line> GetLines(Line_Coordinates_Sruct LC)
        //{
        //    List<CTreeNode.Line> lines = new List<CTreeNode.Line>();

        //    // If the node has at least one child
        //    if (Count > 1)
        //        lines.Add(LC.commonLineCalc(this));

        //    // For each child of the node
        //    foreach (CTreeNode childNode in this)
        //        lines.Add(LC.childLineCalc(childNode));

        //    return lines;
        //}

        internal CTreeNode.ColorLines GetLines2(Line_Coordinates_Sruct LC) {
            //List<CTreeNode.Line> lines = new List<CTreeNode.Line>();
            CTreeNode.ColorLines colorLines = new CTreeNode.ColorLines();

            // If the node has at least one child
            if (Count > 1) {
                //lines.Add(LC.commonLineCalc(this));
                colorLines.Common.Add(LC.commonLineCalc(this));

            }

            // For each child of the node
            foreach (CTreeNode childNode in this) {
                //lines.Add(LC.childLineCalc(childNode));
                colorLines.Child.Add(LC.childLineCalc(childNode));
            }

            return colorLines;
        }
        #endregion

        ///  <summary>Determines if the collection has children</summary>
        internal bool HasChildren  {  get { return Count > 0; }  }

        /// <summary>First Node of the Collection</summary>
        internal CTreeNode FirstNode  {  get { return this[0]; }  }

        /// <summary>Last Node of the Collection</summary>
        internal CTreeNode LastNode  {  get { return this[Count - 1]; }  }

        /// <summary>First Node index of the Collection</summary>
        internal int FirstNodeIndex { get { return HasChildren ? 0 : -1; } }

        /// <summary>Last Node index of the Collection</summary>
        internal int LastNodeIndex { get { return HasChildren ? Count - 1 : -1; } }

    }
}