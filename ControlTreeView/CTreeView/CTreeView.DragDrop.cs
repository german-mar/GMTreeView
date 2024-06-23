using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ControlTreeView
{
    // -------------------------------------------------------------------------
    /// <summary>
    /// Drag And Drop implementation
    /// </summary>
    // -------------------------------------------------------------------------
    public partial class CTreeView
    {
        #region drag and drop properties
        // ---------------------------------------------------
        // CTreeView line pen
        // ---------------------------------------------------
        private Pen dragDropLinePen;

        // ---------------------------------------------------
        // drag and drop target position in a drag and drop operation
        // ---------------------------------------------------
        // line indicating the drag target position
        private Point dragDropLinePoint1, dragDropLinePoint2;

        // rectangle indicating the drag target position
        private Rectangle dragDropRectangle;

        // ---------------------------------------------------
        // scrolling
        // ---------------------------------------------------
        // scrolling timer
        private Timer scrollTimer;

        // indicates what type of scrolling the CTreeView requires
        private bool scrollUp, scrollDown, scrollRight, scrollLeft;
        #endregion

        #region scroll management:  scrollTimer_Tick event handler, SetScrollDirections
        [DllImport("user32.dll")]
        static extern int SendMessage(
               int hWnd,     // handle to destination window
               uint Msg,     // message
               long wParam,  // first message parameter
               long lParam   // second message parameter
        );

        // -------------------------------------------------------------------------
        // Timer tick event handler. Used to scroll the CTreeView
        // -------------------------------------------------------------------------
        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            // scroll left = -1, scroll right = 1
            // scroll down = -1, scroll up   = -1
            //this.HorizontalScroll.Value = (scrollLeft) ? -1 : (scrollRight) ? 1 : 0;
            //this.VerticalScroll.Value   = (scrollDown) ? -1 : (scrollUp)    ? 1 : 0;

            // ------------------------------------------------------------------
            //int top  = (scrollDown) ? -1 : (scrollUp)    ? 1 : 0;
            //int left = (scrollLeft) ? -1 : (scrollRight) ? 1 : 0;

            //if (top != 0 || left != 0) {
            //    foreach (Control control in this.Controls) {
            //        control.Top  += top;
            //        control.Left += left;
            //    }
            //}
            // ------------------------------------------------------------------

            // ------------------------------------------------------------------
            int handle = this.Handle.ToInt32();

            if      (scrollDown)  SendMessage(handle, 277, 1, 0);
            else if (scrollUp)    SendMessage(handle, 277, 0, 0);

            if      (scrollRight) SendMessage(handle, 276, 1, 0);
            else if (scrollLeft)  SendMessage(handle, 276, 0, 0);
            // ------------------------------------------------------------------
        }

        /// <summary>
        /// Sets the directions in which need scroll.
        /// </summary>
        /// <param name="scrollUp">true if need scroll up, otherwise, false.</param>
        /// <param name="scrollDown">true if need scroll down, otherwise, false.</param>
        /// <param name="scrollRigh">true if need scroll right, otherwise, false.</param>
        /// <param name="scrollLeft">true if need scroll left, otherwise, false.</param>
        internal void SetScrollDirections(bool scrollUp, bool scrollDown, bool scrollRigh, bool scrollLeft) {
            scrollTimer.Enabled = (scrollUp || scrollDown || scrollRigh || scrollLeft);

            this.scrollUp    = scrollUp;
            this.scrollDown  = scrollDown;
            this.scrollRight = scrollRigh;
            this.scrollLeft  = scrollLeft;
        }
        #endregion

        #region struct DragTargetPositionClass
        /// <summary>Struct that stores nodeDirect, nodeBefore and nodeAfter.</summary>
        public struct DragTargetPositionClass
        {
            #region Constructor
            internal DragTargetPositionClass(CTreeNode nodeDirect, CTreeNode nodeBefore, CTreeNode nodeAfter)
            {
                _nodeDirect = nodeDirect;
                _nodeBefore = nodeBefore;
                _nodeAfter  = nodeAfter;
            }
            #endregion

            #region Ebabled
            /// <summary>Gets a value indicating whether drag destination nodes are not empty.</summary>
            public bool Enabled
            {
                get { return (_nodeDirect != null || _nodeBefore != null || _nodeAfter != null); }
            }
            #endregion

            #region get nodes of drag target position: NodeDirect, NodeBefore, NodeAfter
            private CTreeNode _nodeDirect;
            /// <summary>The direct node of drag target position.</summary>
            public  CTreeNode NodeDirect    {  get { return _nodeDirect; }  }
            
            private CTreeNode _nodeBefore;
            /// <summary>The upper node of drag target position.</summary>
            public  CTreeNode NodeBefore    {  get { return _nodeBefore; }  }
            
            private CTreeNode _nodeAfter;
            /// <summary>The lower node of drag target position.</summary>
            public  CTreeNode NodeAfter     {  get { return _nodeAfter;  }  }
            #endregion
        }
        #endregion

        #region updateDragTargetPosition (3 methods + utility methods)

        #region updateDragTargetPosition()
        private void UpdateDragTargetPosition()
        {
            bool haveANode  =  (DragTargetPosition.NodeDirect != null ||
                                DragTargetPosition.NodeBefore != null ||
                                DragTargetPosition.NodeAfter  != null);

            if (haveANode)
            {
                DragTargetPosition = new DragTargetPositionClass(null, null, null);

                dragDropLinePoint1 = Point.Empty;
                dragDropLinePoint2 = Point.Empty;

                dragDropRectangle  = Rectangle.Empty;

                Refresh();
            }
        }
        #endregion

        #region updateDragTargetPosition(CTreeNode node)
        private void UpdateDragTargetPosition(CTreeNode node)
        {
            if (DragTargetPosition.NodeDirect != node)
            {
                DragTargetPosition = new DragTargetPositionClass(node, null, null);

                dragDropLinePoint1 = Point.Empty;
                dragDropLinePoint2 = Point.Empty;

                dragDropRectangle  = node.Bounds;
                dragDropRectangle.Inflate(2, 2);
                
                Refresh();
            }
        }
        #endregion

        #region updateDragTargetPosition(CTreeNode nodeBefore, CTreeNode nodeAfter)
        private void UpdateDragTargetPosition(CTreeNode nodeBefore, CTreeNode nodeAfter)
        {
            if (DragTargetPosition.NodeBefore != nodeBefore || DragTargetPosition.NodeAfter != nodeAfter)
            {
                DragTargetPosition = new DragTargetPositionClass(null, nodeBefore, nodeAfter);

                bool isVerticalDiagram  =  (DrawStyle == CTreeViewDrawStyle.VerticalDiagram);

                int offset = 2; // It can never be 0 (for now). I don't know if it can have a value different from 2.

                if      (nodeBefore == null) { SetCoordinates(nodeAfter, -offset,    isVerticalDiagram); }
                else if (nodeAfter  == null) { SetCoordinates(nodeBefore, offset,    isVerticalDiagram); } 
                else                         { SetCoordinates(nodeBefore, nodeAfter, isVerticalDiagram); }

                dragDropRectangle = Rectangle.Empty;

                Refresh();
            }
        }
        #endregion

        #region set oordinates for one or tuo nodes (2 methods)
        // ------------------------------------------------------------------------------------
        // same method for nodeBefore and nodeAffter,
        // the only difference is the offset = -2 and +2 respectively
        // ------------------------------------------------------------------------------------
        private void SetCoordinates(CTreeNode node, int offset, bool isVerticalDiagram) {
            //Rectangle rect = (!isVerticalDiagram) ? node.BoundsSubtree : rotateRectangle(node.BoundsSubtree);
            Rectangle rect = node.BoundsSubtree;

            if (isVerticalDiagram) {
                // ----------------------------------------------------
                // rotate rectangle (convert vertical diagram rectangle in horizontal diagram rectangle)
                // ----------------------------------------------------
                //          ┌───────────┐ y1        ┌───────────┐
                //          │           │           |           │
                //          │           │ --------> │           │
                //          |           │           |           │
                //          └───────────┘ y2        └───────────┘ y1 y2
                //                     x1           x1         x2
                //                     x2
                // ----------------------------------------------------
                rect = RotateRectangle(rect);
            }

            // --------------------------------------------------------
            // update drag target position (for both, horizontal or vertical diagram rectangles)
            // --------------------------------------------------------
            //          ┌───────────┐ y1, y2    for NodeAfter   YY = rect.Y
            //          |           │
            //          │           │
            //          |           │
            //          └───────────┘ y1, y2    for NodeBefore  YY = rect.Bottom
            //          x1         x2
            // --------------------------------------------------------
            // offset < 0 for NodeAfter, offset > 0 for NodeBefore
            int YY = (offset < 0) ? rect.Y : rect.Bottom;

            int x1 = rect.X;        int y1 = YY + offset;
            int x2 = rect.Right;    int y2 = y1;

            dragDropLinePoint1 = new Point(x1, y1);
            dragDropLinePoint2 = new Point(x2, y2);
        }

        private void SetCoordinates(CTreeNode nodeBefore, CTreeNode nodeAfter, bool isVerticalDiagram) {
            Rectangle rect_A =  nodeAfter.BoundsSubtree;
            Rectangle rect_B = nodeBefore.BoundsSubtree;

            if (isVerticalDiagram) {
                // -----------------------------------------------------------------------------------------
                // rotate rectangles (convert vertical diagram rectangles in horizontal diagram rectangles)
                // -----------------------------------------------------------------------------------------
                //  ┌───────────┐ y1                                ┌───────────┐
                //  │nodeBefore │                                   │nodeBefore │
                //  |           ├────────-┐                         │           │
                //  │           │         |                         │           │
                //  └───────────┘         │    ----------------->   └────┬──────┘ y1 y2
                //            x1          │                         x1   |
                //            x2          |                              │
                //                  ┌─────┴─────┐                        │          ┌───────────┐
                //                  │ nodeAfter │                        |          │ nodeAfter │
                //                  │           │                        └─────────-┤           |
                //                  │           │                                   │           │
                //                  └───────────┘ y2 = maxBottom                    └───────────┘
                //                                                                             x2 = maxRight
                // -----------------------------------------------------------------------------------------                                                                          x2 = maxRight
                rect_A = RotateRectangle(rect_A);
                rect_B = RotateRectangle(rect_B);
            }

            // ---------------------------------------------------------------------------------------------
            // update drag target position (for both, horizontal or vertical diagram rectangles)
            // ---------------------------------------------------------------------------------------------
            //          ┌───────────┐ y1b
            //          |nodeBefore |
            //          │           │
            //          │           │
            //          └────┬──────┘ y2b     y1, y2 = y2b + offset
            //          x1b  |    x2b
            //               │          ┌───────────┐
            //               |          | nodeAfter │
            //               └─────────-┤           |
            //                          │           │
            //                          └───────────┘
            //                          x1a       x2a
            //
            //          x1 = x1b                  x2 = maxRight = Max(x2b, x2a)
            // ---------------------------------------------------------------------------------------------
            int maxRight = Math.Max(rect_B.Right, rect_A.Right);
            int offset   = IndentWidth / 2;

            int x1 = rect_B.X;  int y1 = rect_B.Bottom + offset;
            int x2 = maxRight;  int y2 = y1;
            
            dragDropLinePoint1 = new Point(x1, y1);
            dragDropLinePoint2 = new Point(x2, y2);
        }
        #endregion

        #region rotate rectangle (convert vertical diagram coordinates to horizontal diagram coordinates)
        /// <summary>
        /// Rotate rectngle
        /// </summary>
        /// <param name="rect">The rectangle to rotate.</param>
        /// <returns>A new rectangle rotated.</returns>
        private Rectangle RotateRectangle(Rectangle rect) {
            // -------------------------------------------------------------
            // Rotates a rectangle through the axis (x1, y1) - (x2, y2) 
            // -------------------------------------------------------------
            //          x1             x2               y1      y2
            //      y1  ┌───────────────┐ y1         x1 ┌────────┐ x1
            //          |               │               │        │
            //          │               │   -------->   │        │
            //          |               │               │        │
            //      y2  └───────────────┘ y2            │        │
            //          x1             x2               │        │
            //                                          |        |
            //                                       x2 └────────┘ x2
            //                                          y1      y2
            // -------------------------------------------------------------
            int x1 = rect.X;
            int y1 = rect.Y;
            int x2 = rect.Right;
            int y2 = rect.Bottom;

            int width  = x2 - x1;
            int height = y2 - y1;

            return new Rectangle(y1, x1, height, width);
        }
        #endregion

        #endregion

        #region ResetDragTargetPosition
        internal void ResetDragTargetPosition()
        {
            scrollTimer.Enabled = false;

            UpdateDragTargetPosition();
        }
        #endregion

        #region SetDragTargetPosition
        /// <summary>Sets the drag destination nodes according to specified cursor position.</summary>
        /// <param name="dragPosition">The position of mouse cursor during drag.</param>
        internal void SetDragTargetPosition(Point dragPosition)
        {
            // ------------------------------------------------------------
            // get one destination node and a list of possible drag-and-drop target nodes
            // ------------------------------------------------------------
            CTreeNode destinationNode = null;
            CTreeNodeCollection destinationCollection = Nodes;

            Nodes.TraverseNodes(node => node.Visible && node.BoundsSubtree.Contains(dragPosition), node =>
            {
                destinationNode       = node;
                destinationCollection = node.Nodes;
            });


            bool match = false;

            bool destinationNode_exists = (destinationNode != null);

            // ------------------------------------------------------------
            // if Drag position inside of destination node
            // ------------------------------------------------------------
            bool Drag_position_within_node  =  destinationNode_exists && destinationNode.Bounds.Contains(dragPosition);

            if (Drag_position_within_node)
            {
                match = Process_Dragging_InsideDestinationNode(dragPosition, destinationNode);
                if (match) return;
            }
            // ------------------------------------------------------------
            // if Drag position outside of destination node
            // ------------------------------------------------------------
            else //Drag position out of the nodes
            {
                match = Process_DragPosition_Between_Two_Nodes(dragPosition, destinationCollection);
                if (match) return;

                if (destinationNode_exists) {
                    // dragging on sibling nodes of the destination node
                    match = Process_Dragging_to_sibling_nodes(dragPosition, destinationNode);
                    if (match) return;
                }
            }

            UpdateDragTargetPosition();
        }
        #endregion

        #region Process_Dragging_InsideDestinationNode
        // -----------------------------------------------------------------------------------------------
        // Drag position is inside of destination node
        // -----------------------------------------------------------------------------------------------
        private bool Process_Dragging_InsideDestinationNode(Point dragPosition, CTreeNode destinationNode) {
            bool match = false;

            //Find drag position within node
            int delta, coordinate, firstBound, secondBound;

            if (DrawStyle == CTreeViewDrawStyle.VerticalDiagram) {
                // ----------------------------------------------------------------------
                // VERTICAL DIAGRAM
                // ----------------------------------------------------------------------
                //                                 coordinate = MousePosition.X
                //                                      |
                //              Destination Node        |        Drag Node
                //              ┌───────────┐ ya1       |       ┌───────────┐ yb1
                //              │           │           ↓       │           │
                //              |           |       ←---------- |           |      Drag node is being dragged to the left
                //              │           │                   │           │
                //              └───────────┘ ya2               └───────────┘ yb2
                // firstBound = xa1       xa2 = secondBound     xb1       xb2
                //                 ↑     ↑
                //                 |     |
                //                 |     |
                // firstBound + delta   secondBound - delta
                // ----------------------------------------------------------------------
                coordinate = dragPosition.X;                    // coordinate  = MousePosition.X

                delta = destinationNode.Bounds.Width / 4;       // delta       = (xa2 - xa1) / 4
                firstBound = destinationNode.Bounds.Left;       // firstBound  = xa1
                secondBound = destinationNode.Bounds.Right;     // secondBound = xa2
            } else {
                // ----------------------------------------------------------------------
                // HORIZONTAL DIAGRAM
                // ----------------------------------------------------------------------
                //          Destination Node
                //          ┌───────────┐ ya1 firstBound
                //          |           |   ←-------------- firstBound  + delta
                //          │           │
                //          |           |   ←-------------- secondBound - delta
                //          └───────────┘ ya2 secondBound
                //          xa1       xa2
                //                ↑
                //                |     ←--- coordinate = MousePosition.Y
                //                |
                //            Drag Node
                //          ┌───────────┐ yb1
                //          |           |
                //          │           │
                //          |           |
                //          └───────────┘ yb2
                //          xb1       xb2
                // ----------------------------------------------------------------------
                coordinate = dragPosition.Y;                    // coordinate  = MousePosition.Y

                delta = destinationNode.Bounds.Height / 4;      // delta       = (ya2 - ya1) / 4
                firstBound = destinationNode.Bounds.Top;        // firstBound  = ya1
                secondBound = destinationNode.Bounds.Bottom;    // secondBound = ya2
            }

            // ----------------------------------------------------------------------
            // Now we treat both vertical and horizontal diagrams in the same way, as vertical diagrams.
            // ----------------------------------------------------------------------
            //                                         coordinate = MousePosition.X
            //                                              |
            //                      Destination Node        |        Drag Node
            //                      ┌───────────┐ ya1       |       ┌───────────┐ yb1
            //                      │           │           ↓       |           |       Drag node is being dragged to the left
            //                      │           │       ←---------- │           │       or to the right when you are to the left
            //                      │           │                   |           |       of the destination node
            //                      └───────────┘ ya2               └───────────┘ yb2
            //         firstBound = xa1       xa2 = secondBound     xb1       xb2
            //                         ↑     ↑
            //                         |     |
            //                         |     |
            //   firstBound + delta = x1     x2 = secondBound - delta
            // ----------------------------------------------------------------------
            int x1 = firstBound + delta;
            int x2 = secondBound - delta;

            // if I am inside the destination node deltas
            if (coordinate >= x1 && coordinate <= x2) {
                UpdateDragTargetPosition(destinationNode);
                match = true;
            }
            // if I am to the left of the destination node delta
            else if (coordinate < x1) //before
            {
                UpdateDragTargetPosition(destinationNode.PrevNode, destinationNode);
                match = true;
            }
            // if I am to the right of the destination node delta
            else if (coordinate > x2) //after
            {
                UpdateDragTargetPosition(destinationNode, destinationNode.NextNode);
                match = true;
            }

            return match;
        }
        #endregion

        #region Process_DragPosition_Between_Two_Nodes
        // ---------------------------------------------------------------------------------------------
        // Drag position is outside of destination node
        // ---------------------------------------------------------------------------------------------
        // drag position could be between two nodes
        // ---------------------------------------------------------------------------------------------
        //
        // Vertical Diagram View
        //
        //                  MousePosition = dragPosition
        //                                |
        //          ┌───────────┐ y1=Top  |     ┌───────────┐ y1
        //          │ upperNode |         |     | lowerNode │
        //          │           │         ↓     │           │
        //          |           |               |           |
        //          └───────────┘ y2            └───────────┘ y2
        //          x1         x2               x1         x2
        //                      ↑               ↑
        //                      |               |
        //                      |               |
        //                  Right               left
        //                      ←-----width----->           width = Left - Right
        // ---------------------------------------------------------------------------------------------
        private bool Process_DragPosition_Between_Two_Nodes(Point dragPosition, CTreeNodeCollection destinationCollection) {
            bool match = false;

            // 
            for (int count = 0; count <= destinationCollection.Count - 2; count++) {
                // a pair of consecutive nodes to see if the mouse position is between them
                CTreeNode upperNode = destinationCollection[count];
                CTreeNode lowerNode = destinationCollection[count + 1];

                // 
                Rectangle upperBounds = upperNode.BoundsSubtree;
                Rectangle lowerBounds = lowerNode.BoundsSubtree;

                Point betweenLocation = Point.Empty;
                Size betweenSize      = Size.Empty;

                // --------------------------------------------------------
                // VERTICAL DIAGRAM
                // --------------------------------------------------------
                if (DrawStyle == CTreeViewDrawStyle.VerticalDiagram) {
                    int x = upperBounds.Right;
                    int y = upperBounds.Top;

                    int width  = lowerBounds.Left - upperBounds.Right;
                    int height = Math.Max(upperBounds.Height, lowerBounds.Height);

                    betweenLocation = new Point(x, y);
                    betweenSize     = new Size(width, height);

                // --------------------------------------------------------
                // HORIZONTAL DIAGRAM
                // --------------------------------------------------------
                } else {
                    int x = upperBounds.Left;
                    int y = upperBounds.Bottom;

                    int width  = Math.Max(upperBounds.Width, lowerBounds.Width);
                    int height = lowerBounds.Top - upperBounds.Bottom;

                    betweenLocation = new Point(x, y);
                    betweenSize     = new Size(width, height);
                }
                // --------------------------------------------------------

                Rectangle betweenRectangle = new Rectangle(betweenLocation, betweenSize);

                // --------------------------------------------------------
                // Drag position between two nodes
                // --------------------------------------------------------
                if (betweenRectangle.Contains(dragPosition)) {
                    UpdateDragTargetPosition(upperNode, lowerNode);
                    match = true;
                }
            }

            return match;
        }
        #endregion

        #region dragging on sibling nodes of the destination node
        // -------------------------------------------------------------------------------------
        // dragging on sibling nodes of the destination node (same tree level level nodes)
        // -------------------------------------------------------------------------------------
        private bool Process_Dragging_to_sibling_nodes(Point dragPosition, CTreeNode destinationNode) {
            bool match = false;

            Rectangle destination = destinationNode.Bounds;
            bool isAbove, isBelow;

            // --------------------------------------------------------
            // determine if drag position (mouse position) is adjacent to the destination mode
            // --------------------------------------------------------
            if (DrawStyle == CTreeViewDrawStyle.VerticalDiagram) {
                isAbove = (dragPosition.X <= destination.Left);
                isBelow = (dragPosition.X >= destination.Right);
            } else {
                isAbove = (dragPosition.Y <= destination.Top);
                isBelow = (dragPosition.Y >= destination.Bottom);
            }

            // --------------------------------------------------------
            // if is adjacento process it
            // --------------------------------------------------------
            // --------- above or before ----------
            if (isAbove) {
                UpdateDragTargetPosition(destinationNode.PrevNode, destinationNode);
                match = true;

            // --------- below or after -----------
            } else if (isBelow) {
                UpdateDragTargetPosition(destinationNode, destinationNode.NextNode);
                match = true;
            }
            // --------------------------------------------------------

            return match;
        }
        #endregion

        #region CheckValidDrop and utilities

        #region CheckValidDrop
        /// <summary>Checking a valid of drop operation in current destination.</summary>
        /// <param name="sourceNodes">The source nodes of drag and drop operation.</param>
        /// <returns>true if drop of source nodes is allowed to current destination, otherwise, false.</returns>
        internal bool CheckValidDrop(List<CTreeNode> sourceNodes)
        {
            if (!DragTargetPosition.Enabled)
                return false;

            CTreeNode NodeBefore = DragTargetPosition.NodeBefore;
            CTreeNode NodeAfter  = DragTargetPosition.NodeAfter;
            CTreeView NodeDirect = DragTargetPosition.NodeDirect;

            // ----------------------------------------------------------------------------
            // have node direct
            // ----------------------------------------------------------------------------
            if (NodeDirect != null)
            {
                if (DragAndDropMode == CTreeViewDragAndDropMode.Reorder)
                {
                    return false;
                }
                else
                {
                    // --------------------------------------------------------------------
                    // Check that destination node is not descendant of source nodes
                    // test to draw or not a box around a node
                    // --------------------------------------------------------------------
                    if ( isDescendant(sourceNodes, NodeDirect) ) return false;
                }
            }
            // ----------------------------------------------------------------------------
            // have node before or node after
            // ----------------------------------------------------------------------------
            else if (NodeBefore != null || NodeAfter != null)
            {
                // test to draw or not a line between two nodes
                if ( ! isValidPositionToDrop(sourceNodes, NodeBefore, NodeAfter) ) return false;
            }
            // ----------------------------------------------------------------------------
            
            return true;
        }
        #endregion

        #region utilities: isValidPositionToDrop, haveSameParent, isDescendant
        private bool isValidPositionToDrop(List<CTreeNode> sourceNodes, CTreeNode NodeBefore, CTreeNode NodeAfter) {
            // ------------------------------------------------------------------
            // Check that source nodes are not moved relative themselves
            // ------------------------------------------------------------------
            bool containsNodeBefore = sourceNodes.Contains(NodeBefore);
            bool containsNodeAfter  = sourceNodes.Contains(NodeAfter);

            if (containsNodeBefore && containsNodeAfter ) return false;
            if (containsNodeBefore && NodeAfter  == null) return false;
            if (containsNodeAfter  && NodeBefore == null) return false;

            // ------------------------------------------------------------------
            // more checks
            // ------------------------------------------------------------------
            if (DragAndDropMode == CTreeViewDragAndDropMode.Reorder) {
                // Check that source and destination nodes have same parent
                if ( ! haveSameParent(sourceNodes, NodeBefore) ) return false;
                if ( ! haveSameParent(sourceNodes, NodeAfter ) ) return false;

            } else {
                // Check that destination nodes are not descendants of source nodes
                if ( isDescendant(sourceNodes, NodeBefore) ) return false;
                if ( isDescendant(sourceNodes, NodeAfter ) ) return false;
            }
            // ------------------------------------------------------------------

            return true;
        }

        // Check that a node and destination nodes have same parent
        private bool haveSameParent(List<CTreeNode> sourceNodes, CTreeNode node) {
            return ! ( node != null  &&  node.Parent != sourceNodes[0].Parent );
        }
        
        // Check that a node is not descendant of source nodes
        private bool isDescendant(List<CTreeNode> sourceNodes, CTreeNode nodeToTest) {
            bool descendant = false;

            foreach (CTreeNode sourceNode in sourceNodes) {
                sourceNode.TraverseNodes(node => true, node => {
                    if (node == nodeToTest) {
                        descendant = true;
                        return;
                    }
                });

                if (descendant) { break; }
            }

            return descendant;
        }
        #endregion

        #endregion
    }
#endregion


}
