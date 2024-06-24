using System;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Collections.Generic;

namespace ControlTreeView
{
    /// <summary>
    /// Implements specific selection logic and DragAndDrop logic.
    /// </summary>
    public class NodeControl : UserControl, INodeControl
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the NodeControl class.
        /// </summary>
        public NodeControl()
        {
            DoubleBuffered = true;
        }
        #endregion

        #region OwnerNode
        /// <summary>
        /// Owner Node of the node
        /// </summary>
        public CTreeNode OwnerNode { get; set; }
        #endregion

        #region Area
        /// <summary>
        /// Experimental property for changing control's position relative to lines
        /// </summary>
        public virtual Rectangle Area
        {
            get { return new Rectangle(Point.Empty,Size); }
        }
        #endregion

        private Point mouseDownPosition;
        private bool unselectAfterMouseUp, unselectOtherAfterMouseUp; //Flags that indicates what need to do on MouseUp

        #region OnMouseDown
        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //Set selected nodes depends on selection mode
            unselectAfterMouseUp =unselectOtherAfterMouseUp= false;
            if (OwnerNode.OwnerCTreeView.SelectionMode != CTreeViewSelectionMode.None)
            {
                if (((Control.ModifierKeys & Keys.Control) == Keys.Control)&&
                    (OwnerNode.OwnerCTreeView.SelectionMode == CTreeViewSelectionMode.Multi ||
                    (OwnerNode.OwnerCTreeView.SelectionMode == CTreeViewSelectionMode.MultiSameParent &&
                    (OwnerNode.OwnerCTreeView.SelectedNodes.Count == 0 || OwnerNode.OwnerCTreeView.SelectedNodes[0].ParentNode == OwnerNode.ParentNode))))
                {
                    if (!OwnerNode.IsSelected) OwnerNode.IsSelected = true;
                    else unselectAfterMouseUp = true;
                }
                else
                {
                    if (!OwnerNode.IsSelected)
                    {
                        OwnerNode.OwnerCTreeView.ClearSelection();
                        OwnerNode.IsSelected = true;
                    }
                    else unselectOtherAfterMouseUp = true;
                }
            }
            //Set handlers that handle start or not start dragging
            mouseDownPosition = this.OwnerNode.OwnerCTreeView.PointToClient(Cursor.Position);//mouseDownPosition = e.Location;
            this.MouseUp += new MouseEventHandler(NotDragging);
            this.MouseMove += new MouseEventHandler(StartDragging);

            base.OnMouseDown(e);
        }
        #endregion

        #region StartDragging
        //Start dragging if mouse was moved
        private void StartDragging(object sender, MouseEventArgs e)
        {
            Point movePoint = this.OwnerNode.OwnerCTreeView.PointToClient(Cursor.Position);
            if (Math.Abs(mouseDownPosition.X - movePoint.X) + Math.Abs(mouseDownPosition.Y - movePoint.Y) > 5)
            //if (Math.Abs(mouseDownPosition.X - e.Location.X) + Math.Abs(mouseDownPosition.Y - e.Location.Y)>5)
            {
                this.MouseUp -= NotDragging;
                this.MouseMove -= StartDragging;

                OwnerNode.Drag();
            }
        }
        #endregion

        #region NotDragging
        //Do not start dragging if mouse was up
        private void NotDragging(object sender, MouseEventArgs e)
        {
            this.MouseMove -= StartDragging;
            this.MouseUp -= NotDragging;
            if (unselectAfterMouseUp)
            {
                OwnerNode.IsSelected = false;
            }
            if (unselectOtherAfterMouseUp)
            {
                List<CTreeNode> nodesToUnselect = new List<CTreeNode>(OwnerNode.OwnerCTreeView.SelectedNodes);
                nodesToUnselect.Remove(OwnerNode);
                foreach (CTreeNode node in nodesToUnselect) node.IsSelected = false;
            }
        }
        #endregion

        #region InitializeComponent
        // NOT USED XXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //private void InitializeComponent()
        //{
        //    this.SuspendLayout();
        //    // 
        //    // NodeControl
        //    // 
        //    this.Name = "NodeControl";
        //    this.ResumeLayout(false);

        //}
        #endregion
    }
}