using System;
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

                    if ( DragTargetPosition.HaveNodeDirect() )
                    {
                        DragTargetPosition.NodeDirect.Nodes.AddRange(sourceNodes.ToArray());
                    }
                    else if (DragTargetPosition.HaveNodeBefore() && !sourceNodes.Contains(DragTargetPosition.NodeBefore))
                    {
                        int index = DragTargetPosition.NodeBefore.Index + 1;
                        DragTargetPosition.NodeBefore.Parent.Nodes.InsertRange(index, sourceNodes.ToArray());
                        //foreach (CTreeNode sourceNode in sourceNodes) DragTargetPosition.NodeBefore.Parent.Nodes.Insert(index++, sourceNode);
                    }
                    else if (DragTargetPosition.HaveNodeAfter() && !sourceNodes.Contains(DragTargetPosition.NodeAfter))
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
                //if (!Focused) Focus();//?

                CTreeNode toggleNode = GetTogleeNode(e);

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

        private CTreeNode GetTogleeNode(MouseEventArgs e) {
            CTreeNode toggleNode = null;

            this.Nodes.TraverseNodes(node => node.Visible && node.Nodes.HasChildren, node =>
            {
                Point cursorLocation = e.Location;
                cursorLocation.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);

                if (node.PlusMinus != null && node.PlusMinus.IsUnderMouse(cursorLocation)) {
                    toggleNode = node;
                    return;
                }
            });

            return toggleNode;
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

            PaintLines(e);
            PaintDragAndDropDestinationAnimation(e);
            PaintSelection(e);
            PaintPlusMinusButtons(e);

            DrawStatusText(e);
            TestBounds(e);

            base.OnPaint(e);
        }
        
        private void PaintLines(PaintEventArgs e) {
            if (ShowLines) {
                //this.Nodes.TraverseNodes(node => node.IsExpanded, node => { DrawLines(e, node.Lines); } );
                this.Nodes.TraverseNodes(node => node.IsExpanded, node => { DrawLines2(e, node.colorLines); });

                DrawLines2(e, rootLines2);
            }
        }

        //private void DrawLines(PaintEventArgs e, List<CTreeNode.Line> lines) {
        //    if (lines != null)
        //        foreach (CTreeNode.Line line in lines)
        //            e.Graphics.DrawLine(LinesPen, line.Point1, line.Point2);
        //}

        private void DrawLines2(PaintEventArgs e, CTreeNode.ColorLines colorLines) {
            if (colorLines != null) {
                foreach (CTreeNode.Line line in colorLines.Parent)
                    e.Graphics.DrawLine(colorLines.Parent_Pen, line.Point1, line.Point2);

                foreach (CTreeNode.Line line in colorLines.Common)
                    e.Graphics.DrawLine(colorLines.Common_Pen, line.Point1, line.Point2);

                foreach (CTreeNode.Line line in colorLines.Child)
                    e.Graphics.DrawLine(colorLines.Child_Pen, line.Point1, line.Point2);
            }
        }

        private void PaintDragAndDropDestinationAnimation(PaintEventArgs e) {
            if (DragTargetPosition.Enabled) {
                if (dragDrop.HaveRectangle())
                    e.Graphics.FillRectangle(selectionBrush, dragDrop.Rectangle);

                if (dragDrop.HaveLines())
                    e.Graphics.DrawLine(dragDrop.LinePen, dragDrop.LinePoint1, dragDrop.LinePoint2);
            }
        }

        private void PaintSelection(PaintEventArgs e) {
            foreach (CTreeNode node in SelectedNodes) {
                Rectangle selectionRectangle = node.Bounds;
                selectionRectangle.Inflate(2, 2);

                if (!DragTargetPosition.Enabled)
                    e.Graphics.FillRectangle(selectionBrush, selectionRectangle);

                selectionRectangle.Width--; selectionRectangle.Height--;    //костыль
                e.Graphics.DrawRectangle(selectionPen, selectionRectangle);
            }
        }

        private void PaintPlusMinusButtons(PaintEventArgs e) {
            if (ShowPlusMinus) {
                this.Nodes.TraverseNodes(node => node.Visible && node.Nodes.HasChildren, node => {
                    if (node.PlusMinus != null) {
                        Bitmap image = (node.IsExpanded) ? PlusMinus.Minus : PlusMinus.Plus;
                        Point location = node.PlusMinus.Location;

                        e.Graphics.DrawImage(image, location);
                    }
                });
            }
        }

        private void DrawStatusText(PaintEventArgs e) {
            Graphics g = e.Graphics;

            int x = 200;
            int y = 5;

            string text = "Hello World!";

            Font font = new Font("Arial", 14);

            Brush textColor = Brushes.Red;

            g.DrawString(text, font, textColor, x, y);
        }

        private void TestBounds(PaintEventArgs e) {
            ////Test bounds
            //this.Nodes.TraverseNodes(node => node.Visible, node =>
            //{
            //    e.Graphics.DrawRectangle(new Pen(Color.Silver, 1.0F), node.BoundsSubtree);
            //});
        }

        #endregion

        #endregion
    }
}