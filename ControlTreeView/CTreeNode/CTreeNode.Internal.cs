using System;
using System.Collections.Generic;
using System.Drawing;

namespace ControlTreeView
{
    public partial class CTreeNode
    {
        #region Properties

        #region Location and Size
        private Point _location;
        /// <summary>
        /// The location of this node.
        /// </summary>
        internal Point Location
        {
            get { return _location;  }
            set { _location = value; }
        }

        //private Size _size;
        /// <summary>
        /// The size of this node.
        /// </summary>
        internal Size Size
        {
            get
            {
                return (Control is NodeControl) ? ((NodeControl)Control).Area.Size : Control.Size;
            }

            //set { _size = value; }
        }
        #endregion

        #region Lines
        /// <summary>
        /// The list of lines for this node.
        /// </summary>
        internal List<Line> Lines { get; set; }

        internal struct Line
        {
            internal Line(Point p1, Point p2)
            {
                Point1 = p1;
                Point2 = p2;
            }

            internal Point Point1;
            internal Point Point2;
        }
        #endregion

        #region PlusMinus
        /// <summary>
        /// The plus-sign (+) or minus-sign (-) button's area for this node.
        /// </summary>
        internal NodePlusMinus PlusMinus { get; set; }

        internal class NodePlusMinus
        {
            private  const int MinCursorDistance = 3;
            private  Rectangle underMouseArea;
            internal Point     Location { get; private set; }

            internal NodePlusMinus(Rectangle plusMinusArea)
            {
                Location       = plusMinusArea.Location;
                underMouseArea = plusMinusArea;
                underMouseArea.Inflate(MinCursorDistance, MinCursorDistance);
            }

            internal bool IsUnderMouse(Point cursorLocation)
            {
                return underMouseArea.Contains(cursorLocation);
            }
        }
        #endregion

        #endregion

        #region Methods

        #region NextLocation
        /// <summary>
        /// Calculate locations of this node and all child nodes for the CTreeViewDrawStyle.LinearTree.
        /// </summary>
        /// <param name="currentLocation"></param>
        /// <returns></returns>
        internal Point NextLocation(Point currentLocation)
        {
            if (Visible || !OwnerCTreeView.MinimizeCollapsed)
            {
                Location = currentLocation;

                int offsetX = OwnerCTreeView.IndentDepth;
                int offsetY = OwnerCTreeView.IndentWidth + Control.Height;

                currentLocation.Offset(offsetX, offsetY);

                foreach (CTreeNode child in Nodes)
                {
                    currentLocation.Y = child.NextLocation(currentLocation).Y;
                }
            }

            return currentLocation;
        }
        #endregion

        #region NextYMax, NextXMax
        /// <summary>
        /// Calculate locations of this node and all child nodes for the CTreeViewDrawStyle.HorizontalDiagram.
        /// </summary>
        /// <param name="currentX"></param>
        /// <param name="currentYMax"></param>
        /// <returns></returns>
        internal int NextYMax(int currentX, int currentYMax)
        {
            int nextYMax = currentYMax;

            if (Nodes.HasChildren && (IsExpanded || !OwnerCTreeView.MinimizeCollapsed))
            {
                foreach (CTreeNode child in Nodes)
                {
                    nextYMax = child.NextYMax(currentX + Bounds.Width + OwnerCTreeView.IndentDepth, nextYMax);
                }

                int minY = FirstNode.Location.Y + FirstNode.Size.Height / 2;
                int maxY = LastNode.Location.Y  + LastNode.Size.Height  / 2;

                //if (nextYMax - currentYMax - OwnerCTreeView.IndentWidth < Bounds.Height)
                //{
                //    //
                //}

                Location = new Point(currentX, (minY + maxY) / 2 - Size.Height / 2);
            }
            else
            {
                Location  = new Point(currentX, nextYMax);
                nextYMax += Size.Height + OwnerCTreeView.IndentWidth;
            }

            return nextYMax;
        }

        /// <summary>
        /// Calculate locations of this node and all child nodes for the CTreeViewDrawStyle.VerticalDiagram.
        /// </summary>
        /// <param name="currentXMax"></param>
        /// <param name="currentY"></param>
        /// <returns></returns>
        internal int NextXMax(int currentXMax, int currentY)
        {
            if (Nodes.HasChildren && (IsExpanded || !OwnerCTreeView.MinimizeCollapsed))
            {
                foreach (CTreeNode child in Nodes)
                {
                    currentXMax = child.NextXMax(currentXMax, currentY + Bounds.Height + OwnerCTreeView.IndentDepth);
                }

                int minX = FirstNode.Location.X + FirstNode.Size.Width / 2;
                int maxX = LastNode.Location.X  + LastNode.Size.Width  / 2;

                Location = new Point((minX + maxX) / 2 - Size.Width / 2, currentY);
            }
            else
            {
                Location = new Point(currentXMax, currentY);
                currentXMax += Size.Width + OwnerCTreeView.IndentWidth;
            }

            return currentXMax;
        }
        #endregion

        #region CalculatePlusMinus, CalculateLines, CalculateBounds
        /// <summary>
        /// Calculates coordinats for PlusMinus button of this node and all child nodes.
        /// </summary>
        /// <param name="plusMinusCalc"></param>
        /// <param name="needRootPlusMinus"></param>
        internal void CalculatePlusMinus(Func<CTreeNode, Point> plusMinusCalc, bool needRootPlusMinus)
        {
            if (Visible && Nodes.HasChildren)
            {
                PlusMinus = null;

                if (needRootPlusMinus)
                {
                    int offset = -OwnerCTreeView.PlusMinus.Size.Width / 2;

                    Point locationPlusMinus = plusMinusCalc(this);
                    locationPlusMinus.Offset(offset, offset);

                    PlusMinus = new NodePlusMinus(new Rectangle(locationPlusMinus, OwnerCTreeView.PlusMinus.Size));
                }
                
                foreach (CTreeNode child in Nodes)
                    child.CalculatePlusMinus(plusMinusCalc, true);
            }
        }

        /// <summary>
        /// Calculates coordinats for lines of this node and all child nodes.
        /// </summary>
        /// <param name="parentLineCalc"></param>
        /// <param name="commonLineCalc"></param>
        /// <param name="childLineCalc"></param>
        internal void CalculateLines(Func<CTreeNode, Line> parentLineCalc, Func<CTreeNodeCollection, Line> commonLineCalc, Func<CTreeNode, Line> childLineCalc)
        {
            if (Visible && IsExpanded)
            {
                if (Nodes.HasChildren)
                {
                    Lines = new List<Line>();
                    Lines.Add(parentLineCalc(this));
                    Lines.AddRange( Nodes.GetLines(commonLineCalc, childLineCalc) );

                    foreach (CTreeNode child in Nodes)
                        child.CalculateLines(parentLineCalc, commonLineCalc, childLineCalc);
                }
                else
                {
                    Lines = null;//?
                }
                    
            }
        }

        /// <summary>
        /// Calculates fullBounds of this node and all child nodes.
        /// </summary>
        internal void CalculateBounds()
        {
            _boundsSubtree = new Rectangle(Location, Size);

            foreach (CTreeNode child in Nodes)
            {
                if (child.Visible)
                {
                    child.CalculateBounds();
                    _boundsSubtree = Rectangle.Union(child.BoundsSubtree, _boundsSubtree);
                }
            }
        }
        #endregion

        #region BeginUpdateCTreeView, EndUpdateCTreeView
        private void BeginUpdateCTreeView()
        {
            if (OwnerCTreeView != null) OwnerCTreeView.BeginUpdate();
        }

        private void EndUpdateCTreeView()
        {
            if (OwnerCTreeView != null) OwnerCTreeView.EndUpdate();
        }
        #endregion

        #endregion
    }
}