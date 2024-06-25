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
        /// <summary>Initializes a new instance of the NodeControl class.</summary>
        public NodeControl()
        {
            DoubleBuffered = true;
        }
        #endregion

        #region OwnerNode
        /// <summary>Owner Node of the node</summary>
        public CTreeNode OwnerNode { get; set; }
        #endregion

        #region Area
        /// <summary>Experimental property for changing control's position relative to lines</summary>
        public virtual Rectangle Area
        {
            get { return new Rectangle(Point.Empty, Size); }
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
            // ----------------------------------------------------------
            // Set selected nodes depends on selection mode
            // ----------------------------------------------------------
            unselectAfterMouseUp = unselectOtherAfterMouseUp = false;

            CTreeView ownerTreeView = OwnerNode.OwnerCTreeView;

            if (ownerTreeView.SelectionMode != CTreeViewSelectionMode.None)
            {
                bool isControlKeyPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                
                if ( isControlKeyPressed  &&  IsSelectionModeActive(ownerTreeView) )
                {
                    if (OwnerNode.IsSelected)
                        unselectAfterMouseUp = true;
                    //else
                    //    OwnerNode.IsSelected = true;
                }
                else
                {
                    if (OwnerNode.IsSelected)
                        unselectOtherAfterMouseUp = true;
                    else {
                        ownerTreeView.ClearSelection();
                        //OwnerNode.IsSelected = true;
                    }
                }

                OwnerNode.IsSelected = true;
            }

            // ----------------------------------------------------------
            // Set handlers that handle start or not start dragging
            // ----------------------------------------------------------
            mouseDownPosition = this.OwnerNode.OwnerCTreeView.PointToClient(Cursor.Position);//mouseDownPosition = e.Location;
            
            this.MouseUp   += new MouseEventHandler(NotDragging);
            this.MouseMove += new MouseEventHandler(StartDragging);

            // ----------------------------------------------------------
            // 
            // ----------------------------------------------------------
            base.OnMouseDown(e);
        }

        /// <summary>Determine if a selection mode is active</summary>
        private bool IsSelectionModeActive(CTreeView treeView) {
            bool isSelectionModeMulti       = treeView.SelectionMode == CTreeViewSelectionMode.Multi;

            bool isSelectionModeSameParent  = treeView.SelectionMode == CTreeViewSelectionMode.MultiSameParent;
            bool isSelectedNodesEmpty       = treeView.SelectedNodes.Count == 0;
            bool isSameParent               = treeView.SelectedNodes[0].ParentNode == OwnerNode.ParentNode;

            bool isValidSelectionModeMultiSameParent = isSelectionModeSameParent && (isSelectedNodesEmpty || isSameParent);

            return isSelectionModeMulti || isValidSelectionModeMultiSameParent;
        }
        #endregion

        #region StartDragging
        /// <summary>Start dragging if mouse was moved</summary>
        private void StartDragging(object sender, MouseEventArgs e)
        {
            Point movePoint = this.OwnerNode.OwnerCTreeView.PointToClient(Cursor.Position);

            int distanceX = Math.Abs(mouseDownPosition.X - movePoint.X);
            int distanceY = Math.Abs(mouseDownPosition.Y - movePoint.Y);

            if (distanceX + distanceY > 5)
            //if (Math.Abs(mouseDownPosition.X - movePoint.X) + Math.Abs(mouseDownPosition.Y - movePoint.Y) > 5)
            //if (Math.Abs(mouseDownPosition.X - e.Location.X) + Math.Abs(mouseDownPosition.Y - e.Location.Y)>5)
            {
                this.MouseUp   -= NotDragging;
                this.MouseMove -= StartDragging;

                OwnerNode.Drag();
            }
        }
        #endregion

        #region NotDragging
        /// <summary>Do not start dragging if mouse was up</summary>
        private void NotDragging(object sender, MouseEventArgs e)
        {
            this.MouseMove -= StartDragging;
            this.MouseUp   -= NotDragging;

            if (unselectAfterMouseUp)
            {
                OwnerNode.IsSelected = false;
            }

            if (unselectOtherAfterMouseUp)
            {
                UnselectNodes();
            }
        }

        /// <summary>Unselect nodes if unselectOtherAfterMouseUp</summary>
        private void UnselectNodes() {
            List<CTreeNode> nodesToUnselect = new List<CTreeNode>(OwnerNode.OwnerCTreeView.SelectedNodes);

            nodesToUnselect.Remove(OwnerNode);

            foreach (CTreeNode node in nodesToUnselect)
                node.IsSelected = false;
        }
        #endregion

        #region InitializeComponent NOT USED
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