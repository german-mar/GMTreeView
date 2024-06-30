using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ControlTreeView {
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    /// Geometry system to simplify operations between Horizontal and Vertical Diagrams
    /// </summary>
    // ---------------------------------------------------------------------------------------------
    internal class Geometry {
        // -------------------------------------------------------------
        // fields
        // -------------------------------------------------------------
        internal int x1;
        internal int y1;

        internal int x2;
        internal int y2;

        public Geometry(CTreeNode node)
            : this(node.Location.X, node.Location.Y, node.Bounds.Width, node.Bounds.Height) { }


        // -------------------------------------------------------------
        // constructors
        // -------------------------------------------------------------
        internal Geometry(Rectangle rect)
            : this(rect.X, rect.Y, rect.Right, rect.Height) { }

        internal Geometry(Geometry G)
            : this(G.x1, G.y1, G.x2, G.y2) { }

        internal Geometry(int x1, int y1, int x2, int y2) {
            this.x1 = x1;
            this.y1 = y1;

            this.x2 = x2;
            this.y2 = y2;
        }

        // -------------------------------------------------------------
        // Clone
        // -------------------------------------------------------------
        internal Geometry Clone() {
            return new Geometry(x1, y1, x2, y2);
        }

        // -------------------------------------------------------------
        // get points
        // -------------------------------------------------------------
        internal Point GetPoint1() {
            return new Point(x1, y1);
        }

        internal Point GetPoint2() {
            return new Point(x2, y2);
        }

        // -------------------------------------------------------------
        // Rotates a rectangle through the axis (x1, y1) - (x2, y2) 
        // -------------------------------------------------------------
        //          x1             x2               y1      y2
        //      y1  ┌───────────────┐ y1         x1 ┌────────┐ x1       <---- New y1
        //          |               │               │        │
        //          │               │   -------->   │        │
        //          |               │               │        │
        //      y2  └───────────────┘ y2            │        │
        //          x1             x2               │        │
        //                                          |        |
        //                                       x2 └────────┘ x2       <---- New y2
        //                          New x1 --->     y1      y2          <---- New x2
        // -------------------------------------------------------------
        internal void Rotate() {
            Swap(ref x1, ref y1);
            Swap(ref x2, ref y2);
        }

        // -------------------------------------------------------------
        // swap values
        // -------------------------------------------------------------
        private void Swap<T>(ref T x, ref T y) {
            T t = x;
            x   = y;
            y   = t;
        }
    }

    internal class Coordinates {
        // -------------------------------------------------------------------
        // coordinates over node (box around node)
        // -------------------------------------------------------------------
        internal static Geometry GetCoordinates(CTreeNode node, int offset, bool isVerticalDiagram) {
            Geometry G = new Geometry(node.BoundsSubtree);

            // ----------------------------------------------------
            // rotate rectangle (convert vertical diagram rectangle in horizontal diagram rectangle)
            // ----------------------------------------------------
            if (isVerticalDiagram) { G.Rotate(); }

            // --------------------------------------------------------
            // Update target position (for both Horizontal Diagram and Vertical Diagram rectangles) viewed as a Horizontal Diagram rectangle
            // --------------------------------------------------------
            //          ┌───────────┐ y1, y2 = YY + offset      for NodeAfter   YY = G.y1
            //          |           │
            //          │           │
            //          |           │
            //          └───────────┘ y1, y2 = YY + offset      for NodeBefore  YY = G.y2
            //          x1         x2
            // --------------------------------------------------------

            // YY = y1 for NodeAfter and YY = y2 for NodeBefore
            int YY = (offset < 0) ? G.y1 : G.y2;

            int x1 = G.x1; int y1 = YY + offset;
            int x2 = G.x2; int y2 = YY + offset;

            Geometry result = new Geometry(x1, y1, x2, y2);

            // back to vertical rectangle coordinates for Vertical Diagrams
            if (isVerticalDiagram) { result.Rotate(); }

            return result;
        }

        // -------------------------------------------------------------------
        // coordinates between nodes (line between nodes)
        // -------------------------------------------------------------------
        internal static Geometry GetCoordinates(CTreeNode nodeBefore,
                                                CTreeNode nodeAfter,
                                                int  IndentWidth,
                                                bool isVerticalDiagram) {

            Geometry geo_B = new Geometry(nodeBefore.BoundsSubtree);
            Geometry geo_A = new Geometry(nodeAfter.BoundsSubtree);

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
            if (isVerticalDiagram) {
                geo_B.Rotate();
                geo_A.Rotate();
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
            //int maxRight = Math.Max(rect_B.Right, rect_A.Right);
            //int offset = IndentWidth / 2;

            //int x1 = rect_B.X; int y1 = rect_B.Bottom + offset;
            //int x2 = maxRight; int y2 = y1;

            //dragDrop.setLinePoints(x1, y1, x2, y2);

            int maxRight = Math.Max(geo_B.x2, geo_A.x2);
            int offset = IndentWidth / 2;

            int x1 = geo_B.x1;
            int x2 = maxRight;

            int y1 = geo_B.y2 + offset;
            int y2 = geo_B.y2 + offset;

            // back to vertical rectangle coordinates for Vertical Diagrams
            Geometry result = new Geometry(x1, y1, x2, y2);
            
            if (isVerticalDiagram) { result.Rotate(); }

            return result;
        }




    }
}
