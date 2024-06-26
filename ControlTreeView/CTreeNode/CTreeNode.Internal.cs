﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using static ControlTreeView.CTreeNode;

namespace ControlTreeView
{
    public partial class CTreeNode {
        #region Properties

        #region Location and Size
        /// <summary>The location of the node.</summary>
        internal Point Location { get; set; }

        //private Size _size;
        /// <summary>The size of the node.</summary>
        internal Size Size {
            get {
                return (Control is NodeControl) ? ((NodeControl)Control).Area.Size : Control.Size;
            }

            //set { _size = value; }
        }
        #endregion

        #region Lines
        /// <summary>The list of lines for the node.</summary>
        //internal List<Line> Lines { get; set; }

        internal ColorLines  colorLines;

        internal class ColorLines {
            // ---------------------------------------------------------
            // type of line
            // ---------------------------------------------------------
            internal enum Type {
                //ROOT,               // not used
                PARENT,
                CHILD,
                COMMON
            }

            // ---------------------------------------------------------
            // lines
            // ---------------------------------------------------------
            //internal List<Line> Root   { get; set; }
            internal List<Line> Parent { get; set; }
            internal List<Line> Child  { get; set; }
            internal List<Line> Common { get; set; }

            // ---------------------------------------------------------
            // pens
            // ---------------------------------------------------------
            private Pen /*_rootPen,*/ _parentPen, _childPen, _commonPen;

            // ---------------------------------------------------------
            // Constructor
            // ---------------------------------------------------------
            public ColorLines() {
                //Root   = new List<Line>();
                Parent = new List<Line>();
                Child  = new List<Line>();
                Common = new List<Line>();

                //_rootPen   = GetPen(Color.Brown);
                _parentPen = GetPen(Color.Red);
                _childPen  = GetPen(Color.Green);
                _commonPen = GetPen(Color.Blue);
            }

            private static Pen GetPen(Color color) {
                Pen pen = new Pen(color, 4.0F);
                //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                return pen;
            }

            // ---------------------------------------------------------
            // methods
            // ---------------------------------------------------------
            internal List<Line> getLine(Type type) {
                switch (type) {
                    //case Type.ROOT:   return Root;
                    case Type.PARENT: return Parent;
                    case Type.CHILD:  return Child;
                    case Type.COMMON: return Common;
                    default:          return null;
                }
            }

            internal Pen getPen(Type type) {
                switch (type) {
                    //case Type.ROOT:   return _rootPen;
                    case Type.PARENT: return _parentPen;
                    case Type.CHILD:  return _childPen;
                    case Type.COMMON: return _commonPen;
                    default:          return null;
                }
            }

            //// ---------------------------------------------------------
            //// Pen for each line type
            //// ---------------------------------------------------------
            //private Pen _rootPen, _parentPen, _childPen, _commonPen;

            //internal Pen Root_Pen   { get { return _rootPen;   }   set { SetPen(_rootPen,   value); } }
            //internal Pen Parent_Pen { get { return _parentPen; }   set { SetPen(_parentPen, value); } }
            //internal Pen Child_Pen  { get { return _childPen;  }   set { SetPen(_childPen,  value); } }
            //internal Pen Common_Pen { get { return _commonPen; }   set { SetPen(_commonPen, value); } }

            //private void SetPen(Pen pen, Pen value) {
            //    pen.Dispose();
            //    pen = value;
            //}
        }

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
        /// <summary>The plus-sign (+) minus-sign (-) button's class to determine is mouse is over.</summary>
        internal NodePlusMinus PlusMinus { get; set; }

        /// <summary>The plus-sign (+) minus-sign (-) button's class to determine is mouse is over.</summary>
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
            //int nextYMax = currentYMax;

            if (Nodes.HasChildren && (IsExpanded || !OwnerCTreeView.MinimizeCollapsed))
            {
                foreach (CTreeNode child in Nodes)
                {
                    currentYMax = child.NextYMax(currentX + Bounds.Width + OwnerCTreeView.IndentDepth, currentYMax);
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
                Location  = new Point(currentX, currentYMax);
                currentYMax += Size.Height + OwnerCTreeView.IndentWidth;
            }

            return currentYMax;
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

                    Rectangle rect = new Rectangle(locationPlusMinus, OwnerCTreeView.PlusMinus.Size);
                    PlusMinus      = new NodePlusMinus(rect);
                }
                
                foreach (CTreeNode child in Nodes)
                    child.CalculatePlusMinus(plusMinusCalc, true);
            }
        }

        /// <summary>
        /// Calculates coordinats for lines of this node and all child nodes.
        /// </summary>
        /// <param name="LC">Delegates for Lines and PlusMinus coordinates calculus</param>
        /// <remarks>
        /// LC is a struct that contains Functions to calculate node lines and PlusMinus address.
        /// Here we use parentLineCalc, commonLineCalc, childLineCalc functions to calculate lines
        /// for each node in the collection.
        /// </remarks>

        ///// <param name="parentLineCalc"></param>
        ///// <param name="commonLineCalc"></param>
        ///// <param name="childLineCalc"></param>
        internal void CalculateLines(CTreeView.Line_Coordinates_Sruct LC)
        {
            if (Visible && IsExpanded)
            {
                if (Nodes.HasChildren)
                {
                    //Lines = new List<Line>();
                                colorLines = new ColorLines();

                    //Lines.Add(LC.parentLineCalc(this));
                                colorLines.Parent.Add(LC.parentLineCalc(this));

                    //Lines.AddRange( Nodes.GetLines(LC) );
                                ColorLines A = Nodes.GetLines2(LC);

                                colorLines.Common.AddRange(A.Common);
                                colorLines.Child.AddRange(A.Child);

                    foreach (CTreeNode child in Nodes)
                        child.CalculateLines(LC);
                }
                else
                {
                    //Lines = null;//?
                    colorLines = null;
                }
            }
        }

        /// <summary>
        /// Calculates fullBounds of this node and all child nodes.
        /// </summary>
        internal void CalculateBounds()
        {
            BoundsSubtree = new Rectangle(Location, Size);

            foreach (CTreeNode child in Nodes)
            {
                if (child.Visible)
                {
                    child.CalculateBounds();
                    BoundsSubtree = Rectangle.Union(child.BoundsSubtree, BoundsSubtree);
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