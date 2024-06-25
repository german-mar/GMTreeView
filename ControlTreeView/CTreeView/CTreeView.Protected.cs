﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections.Generic;

namespace ControlTreeView
{
    public partial class CTreeView
    {
        #region Properties

        #region DefaultSize
        /// <summary>
        /// Gets the default size of the control.
        /// </summary>
        /// <value>The default Size of the control.</value>
        protected override Size DefaultSize
        {
            get { return new Size(100, 100); }
        }
        #endregion

        #endregion

        #region Events

        #region OnCollapseNode, OnExpandNode
        /// <summary>
        /// Raises the CollapseNode event.
        /// </summary>
        /// <param name="e">A CTreeViewEventArgs that contains the event data.</param>
        protected internal virtual void OnCollapseNode(CTreeViewEventArgs e)
        {
            Recalculate();
            if (CollapseNode != null) CollapseNode(this, e);
        }
        
        /// <summary>
        /// Raises the ExpandNode event.
        /// </summary>
        /// <param name="e">A CTreeViewEventArgs that contains the event data</param>
        protected internal virtual void OnExpandNode(CTreeViewEventArgs e)
        {
            Recalculate();
            if (ExpandNode != null) ExpandNode(this, e);
        }
        #endregion

        #region OnSelectNode
        /// <summary>
        /// Raises the SelectNode event.
        /// </summary>
        /// <param name="e">A CTreeViewEventArgs that contains the event data</param>
        protected internal virtual void OnSelectNode(CTreeViewEventArgs e)
        {
            //ParentCTreeView.Invalidate();
            //ParentCTreeView.Update();
            OwnerCTreeView.Refresh();
            if (SelectNode != null) SelectNode(this, e);
        }
        #endregion

        #region Drag Events: OnDragOver, OnDragEnter, OnDragLeave, OnDragDrop

        #region OnDragOver
        /// <summary>
        /// Raises the DragOver event.
        /// </summary>
        /// <param name="drgevent">A DragEventArgs that contains the event data.</param>
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(typeof(List<CTreeNode>)) /*&& DragAndDropMode != CTreeViewDragAndDropMode.Nothing*/)
            {
                List<CTreeNode> sourceNodes = drgevent.Data.GetData(typeof(List<CTreeNode>)) as List<CTreeNode>;
                Point dragPoint = this.PointToClient(new Point(drgevent.X, drgevent.Y));

                SetScrollDirections(
                    VScroll && dragPoint.Y < 20,
                    VScroll && dragPoint.Y > ClientSize.Height - 20,
                    HScroll && dragPoint.X > ClientSize.Width  - 20,
                    HScroll && dragPoint.X < 20);

                dragPoint.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                SetDragTargetPosition(dragPoint);

                if (sourceNodes[0].OwnerCTreeView == this)
                {
                    if (/*dragDestination.Enabled &&*/ CheckValidDrop(sourceNodes))
                        drgevent.Effect = DragDropEffects.Move;
                    else
                        drgevent.Effect = DragDropEffects.None;
                }
            }

            base.OnDragOver(drgevent);
        }
        #endregion

        #region OnDragEnter
        /// <summary>
        /// Raises the DragEnter event.
        /// </summary>
        /// <param name="drgevent">A DragEventArgs that contains the event data.</param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(typeof(List<CTreeNode>)) /*&& DragAndDropMode != CTreeViewDragAndDropMode.Nothing*/)
            {
                List<CTreeNode> sourceNodes = drgevent.Data.GetData(typeof(List<CTreeNode>)) as List<CTreeNode>;
                Point dragPoint = this.PointToClient(new Point(drgevent.X, drgevent.Y));

                SetScrollDirections(
                    VScroll && dragPoint.Y < 20,
                    VScroll && dragPoint.Y > ClientSize.Height - 20,
                    HScroll && dragPoint.X > ClientSize.Width  - 20,
                    HScroll && dragPoint.X < 20);

                dragPoint.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                SetDragTargetPosition(dragPoint);

                if (sourceNodes[0].OwnerCTreeView == this)
                {
                    if (CheckValidDrop(sourceNodes)) drgevent.Effect = DragDropEffects.Move;
                    else drgevent.Effect = DragDropEffects.None;
                }
            }

            base.OnDragEnter(drgevent);
        }
        #endregion

        #region OnDragLeave
        /// <summary>
        /// Raises the DragLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnDragLeave(EventArgs e)
        {
            /*if (DragAndDropMode != CTreeViewDragAndDropMode.Nothing)*/
            ResetDragTargetPosition();
            base.OnDragLeave(e);
        }
        #endregion

        #region OnDragDrop
        /// <summary>
        /// Raises the DragDrop event.
        /// </summary>
        /// <param name="drgevent">A DragEventArgs that contains the event data.</param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(typeof(List<CTreeNode>)) /*&& DragAndDropMode != CTreeViewDragAndDropMode.Nothing*/)
            {
                List<CTreeNode> sourceNodes = drgevent.Data.GetData(typeof(List<CTreeNode>)) as List<CTreeNode>;
                Point dropPoint = this.PointToClient(new Point(drgevent.X, drgevent.Y));
                dropPoint.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                SetDragTargetPosition(dropPoint);

                if (sourceNodes[0].OwnerCTreeView == this && CheckValidDrop(sourceNodes))
                {
                    BeginUpdate();

                    foreach (CTreeNode sourceNode in sourceNodes) 
                        sourceNode.Parent.Nodes.Remove(sourceNode);

                    if (DragTargetPosition.NodeDirect != null)
                    {
                        DragTargetPosition.NodeDirect.Nodes.AddRange(sourceNodes.ToArray());
                    }
                    else if (DragTargetPosition.NodeBefore != null && !sourceNodes.Contains(DragTargetPosition.NodeBefore))
                    {
                        int index = DragTargetPosition.NodeBefore.Index + 1;
                        DragTargetPosition.NodeBefore.Parent.Nodes.InsertRange(index, sourceNodes.ToArray());
                        //foreach (CTreeNode sourceNode in sourceNodes) DragTargetPosition.NodeBefore.Parent.Nodes.Insert(index++, sourceNode);
                    }
                    else if (DragTargetPosition.NodeAfter != null && !sourceNodes.Contains(DragTargetPosition.NodeAfter))
                    {
                        int index = DragTargetPosition.NodeAfter.Index;
                        DragTargetPosition.NodeAfter.Parent.Nodes.InsertRange(index, sourceNodes.ToArray());
                        //foreach (CTreeNode sourceNode in sourceNodes) DragTargetPosition.NodeAfter.Parent.Nodes.Insert(index++, sourceNode);
                    }

                    EndUpdate();
                }
                //ResetDragTargetPosition();
            }

            base.OnDragDrop(drgevent);
            ResetDragTargetPosition();
        }
        #endregion

        #endregion

        #region OnMouseDown
        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ShowPlusMinus)
            {
                if (!Focused) Focus();//?
                CTreeNode toggleNode = null;

                this.Nodes.TraverseNodes(node => node.Visible && node.Nodes.Count > 0, node =>
                {
                    Point cursorLocation = e.Location;
                    cursorLocation.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                    if (node.PlusMinus != null && node.PlusMinus.IsUnderMouse(cursorLocation))
                    {
                        toggleNode = node;
                    }
                });

                ClearSelection();

                if (toggleNode != null)
                {
                    toggleNode.Toggle();

                    if (SelectionMode != CTreeViewSelectionMode.None)
                        toggleNode.IsSelected = true;
                }
            }

            base.OnMouseDown(e);
        }
        #endregion

        #region OnPaint
        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

            #region Paint lines
            //Paint lines
            if (ShowLines)
            {
                this.Nodes.TraverseNodes(node => node.IsExpanded, node =>
                {
                    if (node.Lines != null)
                        foreach (CTreeNode.Line line in node.Lines)
                            e.Graphics.DrawLine(LinesPen, line.Point1, line.Point2);
                });

                if (rootLines != null)
                    foreach (CTreeNode.Line line in rootLines)
                        e.Graphics.DrawLine(LinesPen, line.Point1, line.Point2);
            }
            #endregion

            #region Paint drag and drop destination animation
            //Paint drag and drop destination animation.
            if (DragTargetPosition.Enabled)
            {
                if ( dragDrop.haveRectangle() )
                    e.Graphics.FillRectangle(selectionBrush, dragDrop.Rectangle);

                if ( dragDrop.haveLines() )
                    e.Graphics.DrawLine(dragDropLinePen, dragDrop.LinePoint1, dragDrop.LinePoint2);
            }
            #endregion

            #region Paint selection
            //Paint selection
            foreach (CTreeNode node in SelectedNodes)
            {
                Rectangle selectionRectangle = node.Bounds;
                selectionRectangle.Inflate(2, 2);

                if (!DragTargetPosition.Enabled)
                    e.Graphics.FillRectangle(selectionBrush, selectionRectangle);

                selectionRectangle.Width--; selectionRectangle.Height--;    //костыль
                e.Graphics.DrawRectangle(selectionPen, selectionRectangle);
            }
            #endregion

            #region Paint PlusMinus buttons
            //Paint PlusMinus buttons
            if (ShowPlusMinus)
            {
                this.Nodes.TraverseNodes(node => node.Visible && node.Nodes.Count > 0, node =>
                {
                    if (node.PlusMinus != null)
                    {
                        Bitmap image    = (node.IsExpanded) ? PlusMinus.Minus : PlusMinus.Plus;
                        Point  location = node.PlusMinus.Location;

                        e.Graphics.DrawImage(image, location);
                    }
                });
            }
            #endregion

            ////Test bounds
            //this.Nodes.TraverseNodes(node => node.Visible, node =>
            //{
            //    e.Graphics.DrawRectangle(new Pen(Color.Silver, 1.0F), node.BoundsSubtree);
            //});

            base.OnPaint(e);
        }
        #endregion

        #endregion
    }
}