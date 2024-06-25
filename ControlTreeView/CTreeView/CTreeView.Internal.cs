using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Text;

namespace ControlTreeView {
    /// <summary>
    /// Calculates the coordinates (x1, y1)-(x2, y2) of the TreeView lines and (x, y) of the PlusMinus button.
    /// </summary>
    public partial class CTreeView {
        #region SuspendUpdate
        private int suspendUpdateCount;
        /// <summary>
        /// 
        /// </summary>
        internal bool SuspendUpdate {
            get { return (suspendUpdateCount > 0); }
        }
        #endregion

        #region list of lines for the CTreeView
        /// <summary>
        /// The list of lines for the CTreeView.
        /// </summary>
        private List<CTreeNode.Line> rootLines;
        #endregion

        #region Line_Coordinates struct: Delegates for Lines and PlusMinus coordinates calculus
        private struct Line_Coordinates_Sruct {
            public Func<CTreeNode, Point>                    plusMinusCalc;
            public Func<CTreeNode, CTreeNode.Line>           parentLineCalc;
            public Func<CTreeNodeCollection, CTreeNode.Line> commonLineCalc;
            public Func<CTreeNode, CTreeNode.Line>           childLineCalc;
        }

        /// <summary>Line Coordinates for PlusMinus button, Parent lines, Common Lines, Child Lines</summary>
        private Line_Coordinates_Sruct LC;
        #endregion

        #region Recalculate
        internal void Recalculate() {
            if (!SuspendUpdate) {
                const int endLineIndent = 2;
                bool showRootPlusMinus  = true;

                Calculate_Visible();

                switch (DrawStyle) {
                    case CTreeViewDrawStyle.LinearTree:
                                                                showRootPlusMinus = ShowRootLines;
                                                                Recalculate_LinearTree(endLineIndent);      break;

                    case CTreeViewDrawStyle.HorizontalDiagram:  Recalculate_HorizontalTree(endLineIndent);  break;

                    case CTreeViewDrawStyle.VerticalDiagram:    Recalculate_VerticalTree(endLineIndent);    break;
                }

                Calculate_PlusMinus(showRootPlusMinus);
                Calculate_Lines();
                Calculate_Bounds();
                Locate_Controls();

                this.AutoScrollMinSize = new Size(BoundsSubtree.Width  + Padding.Right,
                                                  BoundsSubtree.Height + Padding.Bottom);
                //Invalidate();
                //Update();
                Refresh();
            }
        }
        #endregion

        #region Calculate_Visible
        private void Calculate_Visible() {
            foreach (CTreeNode rootNode in Nodes) {
                rootNode.Nodes.TraverseNodes(node => { node.Visible = false; });

                rootNode.Nodes.TraverseNodes(node => node.ParentNode.IsExpanded, node => {
                    node.Visible = true;
                });
            }
        }
        #endregion

        #region Recalculating the coordinates of the tree lines

        #region Recalculate_LinearTree
        private void Recalculate_LinearTree(int endLineIndent) {
            Point startLocation = new Point(Padding.Left + 3, Padding.Top + 3);

            if (ShowRootLines)
                startLocation.X += IndentDepth;

            foreach (CTreeNode node in Nodes)
                startLocation.Y = node.NextLocation(startLocation).Y;

            LC.plusMinusCalc = new Func<CTreeNode, Point>(eachNode =>
                                   getPoint(eachNode, -IndentDepth + 5));

            LC.parentLineCalc = new Func<CTreeNode, CTreeNode.Line>(parent =>
                new CTreeNode.Line(getPoint(parent, 5, parent, endLineIndent),
                                   getPoint(parent, 5, parent.FirstNode)));

            LC.commonLineCalc = new Func<CTreeNodeCollection, CTreeNode.Line>(nodes =>
                new CTreeNode.Line(getPoint(nodes.FirstNode, -IndentDepth + 5, nodes.FirstNode),
                                   getPoint(nodes.FirstNode, -IndentDepth + 5, nodes.LastNode)));

            LC.childLineCalc = new Func<CTreeNode, CTreeNode.Line>(child =>
                new CTreeNode.Line(getPoint(child, -IndentDepth + 5),
                                   getPoint(child, -endLineIndent)));

        }
        #endregion

        #region Recalculate_HorizontalTree
        private void Recalculate_HorizontalTree(int endLineIndent) {
            int startX    = Padding.Left + 3;
            int startYMax = Padding.Top  + 3;

            if (ShowRootLines)
                startX += IndentDepth;

            foreach (CTreeNode node in Nodes)
                startYMax = node.NextYMax(startX, startYMax);

            LC.plusMinusCalc = new Func<CTreeNode, Point>(eachNode =>
                                    getHD_Point2(eachNode, PlusMinus.Size.Width / 2 + 2));

            LC.parentLineCalc = new Func<CTreeNode, CTreeNode.Line>(parent =>
                new CTreeNode.Line( getHD_Point2(parent, endLineIndent),
                                    getHD_Point2(parent, IndentDepth / 2)));

            LC.commonLineCalc = new Func<CTreeNodeCollection, CTreeNode.Line>(nodes =>
                new CTreeNode.Line( getHD_Point(nodes.FirstNode, -IndentDepth / 2),
                                    getHD_Point(nodes.FirstNode, -IndentDepth / 2, nodes.LastNode)));

            LC.childLineCalc = new Func<CTreeNode, CTreeNode.Line>(child =>
                new CTreeNode.Line( getHD_Point(child, -IndentDepth / 2),
                                    getHD_Point(child, -endLineIndent)));

        }
        #endregion

        #region Recalculate_VerticalTree
        private void Recalculate_VerticalTree(int endLineIndent) {
            int startXMax = Padding.Left + 3;
            int startY    = Padding.Top  + 3;

            if (ShowRootLines)
                startY += IndentDepth;

            foreach (CTreeNode node in Nodes)
                startXMax = node.NextXMax(startXMax, startY);

            LC.plusMinusCalc = new Func<CTreeNode, Point>(eachNode =>
                                    getVD_Point2(eachNode, PlusMinus.Size.Width / 2 + 2));

            LC.parentLineCalc = new Func<CTreeNode, CTreeNode.Line>(parent =>
                new CTreeNode.Line( getVD_Point2(parent, endLineIndent),
                                    getVD_Point2(parent, IndentDepth / 2)));

            LC.commonLineCalc = new Func<CTreeNodeCollection, CTreeNode.Line>(nodes =>
                new CTreeNode.Line( getVD_Point(nodes.FirstNode, -IndentDepth / 2),
                                    getVD_Point(nodes.LastNode, -IndentDepth / 2, nodes.FirstNode)));

            LC.childLineCalc = new Func<CTreeNode, CTreeNode.Line>(child =>
                new CTreeNode.Line( getVD_Point(child, -IndentDepth / 2),
                                    getVD_Point(child, -endLineIndent)));
        }
        #endregion

        #region getPoint  CTreeViewDrawStyle LinearTree, HorizontalTree, VerticalTree
        // -------------------- LinearTree -------------------------------------------------
        private Point getPoint(CTreeNode nodeX, int offsetX) {
            return getPoint(nodeX, offsetX, nodeX);
        }
        private Point getPoint(CTreeNode nodeX, int offsetX, CTreeNode nodeY) {
            return getPoint(nodeX, offsetX, nodeY, 0);
        }

        private Point getPoint(CTreeNode nodeX, int offsetX, CTreeNode nodeY, int offsetY) {
            return new Point(nodeX.Location.X + offsetX,
                             nodeY.Location.Y + offsetY + nodeY.Bounds.Height / 2);
        }
        // -------------------- HorizontalTree ---------------------------------------------
        private Point getHD_Point(CTreeNode node, int offsetX) {
            return getHD_Point(node, offsetX, node);
        }

        private Point getHD_Point(CTreeNode node, int offsetX, CTreeNode node2) {
            return new Point( node.Location.X + offsetX,
                             node2.Location.Y + node2.Bounds.Height / 2);
        }

        private Point getHD_Point2(CTreeNode node, int offsetX) {
            return new Point(node.Location.X  + node.Bounds.Width + offsetX,
                             node.Location.Y  + node.Bounds.Height / 2);
        }
        // -------------------- VerticalTree -----------------------------------------------
        private Point getVD_Point(CTreeNode node, int offsetY) {
            return getVD_Point(node, offsetY, node);
        }

        private Point getVD_Point(CTreeNode node, int offsetY, CTreeNode node2) {
            return new Point( node.Location.X + node.Bounds.Width / 2,
                             node2.Location.Y + offsetY);
        }

        private Point getVD_Point2(CTreeNode node, int offsetY) {
            return new Point(node.Location.X  + node.Bounds.Width / 2,
                             node.Location.Y  + node.Bounds.Height + offsetY);
        }
        #endregion

        #endregion

        #region Calculate lines and locations

        #region Calculate_PlusMinus
        private void Calculate_PlusMinus(Boolean showRootPlusMinus) {
            if (ShowPlusMinus) {
                foreach (CTreeNode node in Nodes)
                    node.CalculatePlusMinus(LC.plusMinusCalc, showRootPlusMinus);
            }
        }
        #endregion

        #region Calculate_Lines
        private void Calculate_Lines() {

            if (ShowLines) {
                foreach (CTreeNode node in Nodes)
                    node.CalculateLines(LC.parentLineCalc, LC.commonLineCalc, LC.childLineCalc);

                rootLines = (ShowRootLines) ? Nodes.GetLines(LC.commonLineCalc, LC.childLineCalc) : null;
            }
        }
        #endregion

        #region Calculate Bounds
        private void Calculate_Bounds() {
            //Calculate Bounds
            BoundsSubtree = Rectangle.Empty;

            foreach (CTreeNode node in Nodes) {
                node.CalculateBounds();
                BoundsSubtree = Rectangle.Union(node.BoundsSubtree, BoundsSubtree);
            }
        }
        #endregion

        #region Locate controls
        private void Locate_Controls() {
            //Locate controls
            this.SuspendLayout();

            this.Nodes.TraverseNodes(node => {
                node.Control.Visible  = node.Visible;
                Point controlLocation = node.Location;
                controlLocation.Offset(AutoScrollPosition);

                if (node.Control is NodeControl) {
                    Rectangle area = ((NodeControl) node.Control).Area;
                    controlLocation.Offset(-area.X, -area.Y);
                }

                node.Control.Location = controlLocation;
            });

            this.ResumeLayout(false);
        }
        #endregion

        #endregion

    }
}