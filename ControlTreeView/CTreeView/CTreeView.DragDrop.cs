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
    // Drag And Drop iplementation
    // -------------------------------------------------------------------------
    public partial class CTreeView
    {

        #region SendMessage <-- user32.dll
        [DllImport("user32.dll")]
        static extern int SendMessage(
               int hWnd,     // handle to destination window
               uint Msg,     // message
               long wParam,  // first message parameter
               long lParam   // second message parameter
               );
        #endregion

        #region scrollTimer_Tick
        private Timer scrollTimer;

        private void scrollTimer_Tick(object sender, EventArgs e)
        {
            int handle = this.Handle.ToInt32();

            if      (scrollDown) SendMessage(handle, 277, 1, 0);
            else if (scrollUp)   SendMessage(handle, 277, 0, 0);

            if      (scrollRigh) SendMessage(handle, 276, 1, 0);
            else if (scrollLeft) SendMessage(handle, 276, 0, 0);
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
            public CTreeNode NodeDirect
            {
                get { return _nodeDirect; }
            }
            
            private CTreeNode _nodeBefore;
            /// <summary>The upper node of drag target position.</summary>
            public CTreeNode NodeBefore
            {
                get { return _nodeBefore; }
            }
            
            private CTreeNode _nodeAfter;
            /// <summary>The lower node of drag target position.</summary>
            public CTreeNode NodeAfter
            {
                get { return _nodeAfter; }
            }
            #endregion
        }
        #endregion

        private Pen dragDropLinePen;
        private Point dragDropLinePoint1, dragDropLinePoint2;
        private Rectangle dragDropRectangle;

        #region SetScrollDirections
        private bool scrollUp, scrollDown, scrollRigh, scrollLeft;
        /// <summary>
        /// Sets the directions in which need scroll.
        /// </summary>
        /// <param name="scrollUp">true if need scroll up, otherwise, false.</param>
        /// <param name="scrollDown">true if need scroll down, otherwise, false.</param>
        /// <param name="scrollRigh">true if need scroll right, otherwise, false.</param>
        /// <param name="scrollLeft">true if need scroll left, otherwise, false.</param>
        internal void SetScrollDirections(bool scrollUp, bool scrollDown, bool scrollRigh, bool scrollLeft)
        {
            scrollTimer.Enabled = (scrollUp || scrollDown || scrollRigh || scrollLeft);

            this.scrollUp   = scrollUp;
            this.scrollDown = scrollDown;
            this.scrollRigh = scrollRigh;
            this.scrollLeft = scrollLeft;
        }
        #endregion

        #region updateDragTargetPosition (3 methods)

        #region updateDragTargetPosition()
        private void updateDragTargetPosition()
        {
            bool haveANode  =  (DragTargetPosition.NodeDirect != null || DragTargetPosition.NodeBefore != null || DragTargetPosition.NodeAfter != null);

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
        private void updateDragTargetPosition(CTreeNode node)
        {
            if (DragTargetPosition.NodeDirect != node)
            {
                DragTargetPosition = new DragTargetPositionClass(node, null, null);

                dragDropRectangle  = node.Bounds;
                dragDropRectangle.Inflate(2, 2);
                dragDropLinePoint1 = Point.Empty;
                dragDropLinePoint2 = Point.Empty;
                Refresh();
            }
        }
        #endregion

        #region updateDragTargetPosition(CTreeNode nodeBefore, CTreeNode nodeAfter)
        private void updateDragTargetPosition(CTreeNode nodeBefore, CTreeNode nodeAfter)
        {
            if (DragTargetPosition.NodeBefore != nodeBefore || DragTargetPosition.NodeAfter != nodeAfter)
            {
                DragTargetPosition = new DragTargetPositionClass(null, nodeBefore, nodeAfter);

                bool isVerticalDiagram  =  (DrawStyle == CTreeViewDrawStyle.VerticalDiagram);

                if (nodeBefore == null) {
                    setCoordinates_ForOneNode(nodeAfter, -2, isVerticalDiagram);

                } else if (nodeAfter == null) {
                    setCoordinates_ForOneNode(nodeBefore, 2, isVerticalDiagram);

                } else {
                    setCoordinates_ForTwoNodes(nodeBefore, nodeAfter, isVerticalDiagram);
                }

                dragDropRectangle = Rectangle.Empty;
                Refresh();
            }
        }

        // ------------------------------------------------------------------------------------
        // same method for nodeBefore and nodeAffter,
        // the only difference is the offset = -2 and +2 respectively
        // ------------------------------------------------------------------------------------
        private void setCoordinates_ForOneNode(CTreeNode node, int offset, bool isVerticalDiagram) {
            Rectangle rect = node.BoundsSubtree;

            if (isVerticalDiagram) {
                // ----------------------------------------------------
                // rotate rectangle (convert vertical diagram rectangle in horizontal diagram rectangle)
                // ----------------------------------------------------
                //          +-----------+ y1        +-----------+
                //          |           |           |           |
                //          |           | --------> |           |
                //          |           |           |           |
                //          +-----------+ y2        +-----------+ y1 y2
                //                     x1           x1         x2
                //                     x2
                // ----------------------------------------------------
                rect = rotateRectangle(rect);
            }

            // --------------------------------------------------------
            // update drag target position (for both, horizontal or vertical diagram rectangles)
            // --------------------------------------------------------
            //          +-----------+ y1, y2    for NodeAfter
            //          |           |
            //          |           |
            //          |           |
            //          +-----------+ y1, y2    for NodeBefore
            //          x1         x2
            // --------------------------------------------------------
            // offset < 0 for NodeAfter, offset > 0 for NodeBefore
            int YY = (offset < 0) ? rect.Y : rect.Bottom;

            int x1 = rect.X;        int y1 = YY + offset;
            int x2 = rect.Right;    int y2 = y1;

            dragDropLinePoint1 = new Point(x1, y1);
            dragDropLinePoint2 = new Point(x2, y2);
        }

        private void setCoordinates_ForTwoNodes(CTreeNode nodeBefore, CTreeNode nodeAfter, bool isVerticalDiagram) {
            Rectangle rect_A =  nodeAfter.BoundsSubtree;
            Rectangle rect_B = nodeBefore.BoundsSubtree;

            if (isVerticalDiagram) {
                // -----------------------------------------------------------------------------------------
                // rotate rectangles (convert vertical diagram rectangles in horizontal diagram rectangles)
                // -----------------------------------------------------------------------------------------
                //  +-----------+ y1                                +-----------+
                //  |nodeBefore |                                   |nodeBefore |
                //  |           +---------+                         |           |
                //  |           |         |                         |           |
                //  +-----------+         |    ----------------->   +----+------+ y1 y2
                //            x1          |                         x1   |
                //            x2          |                              |
                //                  +-----------+                        |          +-----------+
                //                  | nodeAfter |                        |          | nodeAfter |
                //                  |           |                        +----------+           |
                //                  |           |                                   |           |
                //                  +-----------+ y2 = maxBottom                    +-----------+
                //                                                                             x2 = maxRight
                // -----------------------------------------------------------------------------------------                                                                          x2 = maxRight
                rect_A = rotateRectangle(rect_A);
                rect_B = rotateRectangle(rect_B);
            }

            // ---------------------------------------------------------------------------------------------
            // update drag target position (for both, horizontal or vertical diagram rectangles)
            // ---------------------------------------------------------------------------------------------
            //          +-----------+ y1b
            //          |nodeBefore |
            //          |           |
            //          |           |
            //          +----+------+ y2b     y1, y2 = y2b + offset
            //          x1b  |    x2b
            //               |          +-----------+
            //               |          | nodeAfter |
            //               +----------+           |
            //                          |           |
            //                          +-----------+
            //                          x1a       x2a
            //
            //          x1 = x1b                  x2 = maxRight = Max(x2b, x2a)
            // ---------------------------------------------------------------------------------------------
            int maxRight = Math.Max(rect_B.Right, rect_A.Right);
            int offset   = IndentWidth / 2;

            int x1 = rect_B.X;      int y1 = rect_B.Bottom + offset;
            int x2 = maxRight;      int y2 = y1;
            
            dragDropLinePoint1 = new Point(x1, y1);
            dragDropLinePoint2 = new Point(x2, y2);
        }

        /// <summary>
        /// Rotate rectngle
        /// </summary>
        /// <param name="rect">The rectangle to rotate.</param>
        /// <returns>A new rectangle rotated.</returns>
        private Rectangle rotateRectangle(Rectangle rect) {
            // -------------------------------------------------------------
            // Rotates a rectangle through the axis (x1, y1) - (x2, y2) 
            // -------------------------------------------------------------
            //          x1             x2               y1      y2
            //      y1  *---------------+ y1         x1 *--------+ x1
            //          |               |               |        |
            //          |               |   -------->   |        |
            //          |               |               |        |
            //      y2  +---------------* y2            |        |
            //          x1             x2               |        |
            //                                          |        |
            //                                       x2 +--------* x2
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

            updateDragTargetPosition();
        }
        #endregion

        #region SetDragTargetPosition
        /// <summary>Sets the drag destination nodes according to specified cursor position.</summary>
        /// <param name="dragPosition">The position of mouse cursor during drag.</param>
        internal void SetDragTargetPosition(Point dragPosition)
        {
            CTreeNode destinationNode = null;
            CTreeNodeCollection destinationCollection = Nodes;

            Nodes.TraverseNodes(node => node.Visible && node.BoundsSubtree.Contains(dragPosition), node =>
            {
                destinationNode = node;
                destinationCollection = node.Nodes;
            });

            if (destinationNode != null && destinationNode.Bounds.Contains(dragPosition)) //Drag position within node
            {
                //Find drag position within node
                int delta, coordinate, firstBound, secondBound;

                if (DrawStyle == CTreeViewDrawStyle.VerticalDiagram)
                {
                    // ----------------------------------------------------
                    //          Destination Node  delta     Drag Node
                    //          +-----------+ ya1   |       +-----------+ yb1
                    //          |           |       V       |           |
                    //          |           |     <-------- |           |
                    //          |           |               |           |
                    //          +-----------+ ya2           +-----------+ yb2
                    //          xa1       xa2               xb1       xb2
                    //     firstBound   secondBound
                    // ----------------------------------------------------
                    coordinate = dragPosition.X;                    // coordinate  = xb1

                    delta = destinationNode.Bounds.Width / 4;       // delta       = (xa2 - xa1) / 4
                    firstBound  = destinationNode.Bounds.Left;      // firstBound  = xa1
                    secondBound = destinationNode.Bounds.Right;     // secondBound = xa2
                }
                else
                    // ----------------------------------------------------
                    //          Destination Node
                    //          +-----------+ ya1   firstBound
                    //          |           |
                    //          |           |
                    //          |           |
                    //          +-----------+ ya2   secondBound
                    //          xa1   ^   xa2
                    //                |         <-- delta
                    //                |
                    //            Drag Node
                    //          +-----------+ yb1
                    //          |           |
                    //          |           |
                    //          |           |
                    //          +-----------+ yb2
                    //          xb1       xb2
                    // ----------------------------------------------------
                    coordinate = dragPosition.Y;

                    delta = destinationNode.Bounds.Height / 4;      // delta       = (ya2 - ya1) / 4
                    firstBound  = destinationNode.Bounds.Top;       // firstBound  = ya1
                    secondBound = destinationNode.Bounds.Bottom;    // secondBound = ya2
                }

                if (coordinate >= firstBound + delta && coordinate <= secondBound - delta)
                {
                    updateDragTargetPosition(destinationNode);
                    return;
                }
                else if (coordinate < firstBound + delta) //before
                {
                    updateDragTargetPosition(destinationNode.PrevNode, destinationNode);
                    return;
                }
                else if (coordinate > secondBound - delta) //after
                {
                    updateDragTargetPosition(destinationNode, destinationNode.NextNode);
                    return;
                }
            }
            else //Drag position out of the nodes
            {
                //Check drag position between two nodes
                CTreeNode upperNode = null, lowerNode = null;
                bool isBetween = false;

                for (int count = 0; count <= destinationCollection.Count - 2; count++)
                {
                    upperNode = destinationCollection[count];
                    lowerNode = destinationCollection[count + 1];

                    Point betweenLocation = Point.Empty;
                    Size  betweenSize     = Size.Empty;

                    if (DrawStyle == CTreeViewDrawStyle.VerticalDiagram)
                    {
                        betweenLocation = new Point(upperNode.BoundsSubtree.Right, upperNode.BoundsSubtree.Top);
                        betweenSize     = new Size(lowerNode.BoundsSubtree.Left - upperNode.BoundsSubtree.Right, Math.Max(upperNode.BoundsSubtree.Height, lowerNode.BoundsSubtree.Height));
                    }
                    else
                    {
                        betweenLocation = new Point(upperNode.BoundsSubtree.Left, upperNode.BoundsSubtree.Bottom);
                        betweenSize     = new Size(Math.Max(upperNode.BoundsSubtree.Width, lowerNode.BoundsSubtree.Width), lowerNode.BoundsSubtree.Top - upperNode.BoundsSubtree.Bottom);
                    }

                    Rectangle betweenRectangle = new Rectangle(betweenLocation, betweenSize);
                    
                    if (betweenRectangle.Contains(dragPosition))
                    {
                        isBetween = true;
                        break;
                    }
                }

                if (isBetween) //Drag position between two nodes
                {
                    updateDragTargetPosition(upperNode, lowerNode);
                    return;
                }
                else if (destinationNode != null)
                {
                    Rectangle ownerBounds = destinationNode.Bounds;
                    bool isAbove, isBelow;
                    if (DrawStyle == CTreeViewDrawStyle.VerticalDiagram)
                    {
                        isAbove = (dragPosition.X <= ownerBounds.Left);
                        isBelow = (dragPosition.X >= ownerBounds.Right);
                    }
                    else
                    {
                        isAbove = (dragPosition.Y <= ownerBounds.Top);
                        isBelow = (dragPosition.Y >= ownerBounds.Bottom);
                    }

                    if (isAbove) //before
                    {
                        updateDragTargetPosition(destinationNode.PrevNode, destinationNode);
                        return;
                    }
                    else if (isBelow) //after
                    {
                        updateDragTargetPosition(destinationNode, destinationNode.NextNode);
                        return;
                    }
                }
            }

            updateDragTargetPosition();
        }
        #endregion

        #region CheckValidDrop
        /// <summary>Checking a valid of drop operation in current destination.</summary>
        /// <param name="sourceNodes">The source nodes of drag and drop operation.</param>
        /// <returns>true if drop of source nodes is allowed to current destination, otherwise, false.</returns>
        internal bool CheckValidDrop(List<CTreeNode> sourceNodes)
        {
            if (!DragTargetPosition.Enabled)
                return false;

            bool isValid = true;

            CTreeNode NodeBefore = DragTargetPosition.NodeBefore;
            CTreeNode NodeAfter  = DragTargetPosition.NodeAfter;

            if (DragTargetPosition.NodeDirect != null)
            {
                if (DragAndDropMode == CTreeViewDragAndDropMode.Reorder)
                {
                    return false;
                }
                else
                {
                    //Check that destination node is not descendant of source nodes
                    foreach (CTreeNode sourceNode in sourceNodes)
                    {
                        sourceNode.TraverseNodes(node => isValid, node =>
                        {
                            if (node == DragTargetPosition.NodeDirect)
                                isValid = false;
                        });

                        if (!isValid)
                            return false;
                    }
                }
            }
            else if (NodeBefore != null || NodeAfter != null)
            {
                //Check that source nodes are not moved relative themselves
                if (sourceNodes.Contains(NodeBefore) && sourceNodes.Contains(NodeAfter)) return false;
                if (sourceNodes.Contains(NodeBefore) && NodeAfter == null) return false;
                if (sourceNodes.Contains(NodeAfter)  && NodeBefore == null) return false;

                if (DragAndDropMode == CTreeViewDragAndDropMode.Reorder)
                {
                    //Check that source and destination nodes have same parent
                    if (NodeBefore != null && NodeBefore.Parent != sourceNodes[0].Parent) return false;
                    if (NodeAfter  != null && NodeAfter.Parent  != sourceNodes[0].Parent) return false;
                }
                else
                {
                    //Check that destination nodes is not descendants of source nodes
                    foreach (CTreeNode sourceNode in sourceNodes)
                    {
                        sourceNode.Nodes.TraverseNodes(node => isValid, node =>
                        {
                            if (NodeBefore == node || NodeAfter == node) isValid = false;
                        });

                        if (!isValid) return false;
                    }
                }
            }

            return true;
        }
    }
    #endregion
}
