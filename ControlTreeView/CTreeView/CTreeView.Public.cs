using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace ControlTreeView
{
    public partial class CTreeView : Panel, INodeContainer
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the CTreeView class.
        /// </summary>
        public CTreeView() : base()
        {
            suspendUpdateCount = 0;
            //InitializeComponent();
            BeginUpdate();

            Nodes = new CTreeNodeCollection(this);

            PathSeparator=@"\";

            //AutoScroll = true;
            //AllowDrop = true;

            ShowPlusMinus = true;
            ShowLines = true;
            ShowRootLines = true;

            _selectedNodes = new List<CTreeNode>();
            SelectionMode = CTreeViewSelectionMode.Multi;

            MinimizeCollapsed = true;
            
            DragAndDropMode = CTreeViewDragAndDropMode.ReplaceReorder;

            IndentDepth = 30;
            IndentWidth = 10;

            selectionPen = new Pen(Color.Black, 1.0F);
            selectionPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            selectionBrush = new SolidBrush(SystemColors.Highlight);

            _LinesPen = new Pen(Color.Black, 1.0F);
            _LinesPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            Bitmap imagePlus  = GetBitmap("plus.bmp");
            Bitmap imageMinus = GetBitmap("minus.bmp");
            PlusMinus = new CTreeViewPlusMinus(imagePlus, imageMinus);

            scrollTimer = new Timer();
            scrollTimer.Tick += new EventHandler(ScrollTimer_Tick);
            scrollTimer.Interval = 1;
                        
            GraphicsPath path = new GraphicsPath();
            path.AddLines(new Point[] { new Point(0, 0), new Point(1, 1), new Point(-1, 1), new Point(0, 0) });
            CustomLineCap cap = new CustomLineCap(null, path);
            cap.WidthScale = 1.0f;

            dragDrop.LinePen = new Pen(Color.Black, 2.0F);
            dragDrop.LinePen.CustomStartCap = cap;
            dragDrop.LinePen.CustomEndCap = cap;

            this.DoubleBuffered = true;
            //this.ResizeRedraw = true;
            //this.AutoScrollMinSize = new Size(0, 0);
            EndUpdate();
        }

        private Bitmap GetBitmap(string name) {
            string resource = "ControlTreeView.Resources." + name;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            return new Bitmap(stream);
        }

        #endregion

        #region Properties

        #region OwnerCTreeView
        /// <summary>Owner TreeView</summary>
        [BrowsableAttribute(false)]
        public CTreeView OwnerCTreeView
        {
            get { return this; }
        }
        #endregion

        #region Get Nodes collection
        /// <summary>
        /// Gets the collection of tree nodes that are assigned to the CTreeView control.
        /// </summary>
        //[EditorAttribute(typeof(CTreeViewEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [BrowsableAttribute(false)]
        public CTreeNodeCollection Nodes { get; private set; }
        #endregion

        #region PathSeparator
        /// <summary>
        /// Gets or sets the delimiter string that the tree node path uses.
        /// </summary>
        /// <value>The delimiter string that the tree node CTreeNode.FullPath property uses. The default is the backslash character (\).</value>
        [DefaultValue(@"\")]
        public string PathSeparator { get; set; }
        #endregion

        #region FirstNode, LastNode
        /// <summary>
        /// Gets the first child tree node in the tree node collection.
        /// </summary>
        /// <value>The first child CTreeNode in the Nodes collection.</value>
        [BrowsableAttribute(false)]
        public CTreeNode FirstNode
        {
            get { return Nodes.FirstNode; }
        }
        
        /// <summary>
        /// Gets the last child tree node in the tree node collection.
        /// </summary>
        /// <value>A CTreeNode that represents the last child tree node.</value>
        [BrowsableAttribute(false)]
        public CTreeNode LastNode
        {
            get { return Nodes[Nodes.LastNodeIndex]; }
        }
        #endregion

        #region SelectedNodes
        internal List<CTreeNode> _selectedNodes;
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public ReadOnlyCollection<CTreeNode> SelectedNodes
        {
            get { return _selectedNodes.AsReadOnly(); }
        }
        #endregion

        internal Pen        selectionPen;
        internal SolidBrush selectionBrush;

        #region DrawStyle
        private CTreeViewDrawStyle _DrawStyle;
        /// <summary>
        /// Gets or sets way that will draw the CTreeView.
        /// </summary>
        public CTreeViewDrawStyle DrawStyle
        {
            get { return _DrawStyle; }
            set
            {
                if (_DrawStyle != value)
                {
                    _DrawStyle = value;
                    //this.AutoScrollPosition = new Point(0, 0);
                    this.Recalculate();
                }
            }
        }
        #endregion

        #region PlusMinus
        private CTreeViewPlusMinus _PlusMinus;
        /// <summary>
        /// Gets or sets a bitmaps for plus-sign (+) and minus-sign (-) buttons of this CTreeView.
        /// </summary>
        [Browsable(false)]
        public CTreeViewPlusMinus PlusMinus
        {
            get { return _PlusMinus; }
            set
            {
                _PlusMinus = value;
                Refresh();
            }
        }
        #endregion

        #region ShowPlusMinus, ShowLines, ShowRootLines
        private bool _ShowPlusMinus;
        /// <summary>
        /// Gets or sets a value indicating whether plus-sign (+) and minus-sign (-) buttons are displayed next to tree nodes that contain child tree nodes.
        /// </summary>
        /// <value>true  if plus sign and minus sign buttons are displayed next to tree nodes that contain child tree nodes; otherwise, false. The default is true.</value>
        [DefaultValue(true)]
        public bool ShowPlusMinus
        {
            get { return _ShowPlusMinus; }
            set
            {
                if (_ShowPlusMinus != value)
                {
                    _ShowPlusMinus = value;
                    Recalculate();
                }
            }
        }
        
        private bool _ShowLines;
        /// <summary>
        /// Gets or sets a value indicating whether lines are drawn between tree nodes.
        /// </summary>
        /// <value>true  if lines are drawn between tree nodes; otherwise, false. The default is true.</value>
        [DefaultValue(true)]
        public bool ShowLines
        {
            get { return _ShowLines; }
            set
            {
                if (_ShowLines != value)
                {
                    _ShowLines = value;
                    Recalculate();
                }
            }
        }

        private bool _ShowRootLines;
        /// <summary>
        /// Gets or sets a value indicating whether lines are drawn for the root nodes of the CTreeView.
        /// </summary>
        /// <value>true  if lines are drawn for the root nodes of the CTreeView; otherwise, false. The default is true.</value>
        [DefaultValue(true)]
        public bool ShowRootLines
        {
            get { return _ShowRootLines; }
            set
            {
                if (_ShowRootLines != value)
                {
                    _ShowRootLines = value;
                    Recalculate();
                }
            }
        }
        #endregion

        #region MinimizeCollapsed
        private bool _MinimizeCollapsed;
        /// <summary>
        /// Gets or sets a value indicating whether position of nodes recalculated when collapsing for diagram style of this CTreeView.
        /// </summary>
        [DefaultValue(true)]
        public bool MinimizeCollapsed
        {
            get { return _MinimizeCollapsed; }
            set
            {
                if (_MinimizeCollapsed != value)
                {
                    _MinimizeCollapsed = value;
                    Recalculate();
                }
            }
        }
        #endregion

        #region SelectionMode
        private CTreeViewSelectionMode _SelectionMode;
        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(typeof(CTreeViewSelectionMode), "Multi")]
        public CTreeViewSelectionMode SelectionMode
        {
            get { return _SelectionMode; }
            set
            {
                if (_SelectionMode != value)
                {
                    _SelectionMode = value;
                    ClearSelection();
                }
            }
        }
        #endregion

        #region DragAndDropMode
        private CTreeViewDragAndDropMode _DragAndDropMode;

        /// <summary>
        /// Gets or sets DragAndDrop mode
        /// </summary>
        [DefaultValue(typeof(CTreeViewDragAndDropMode), "ReplaceReorder")]
        public CTreeViewDragAndDropMode DragAndDropMode
        {
            get { return _DragAndDropMode; }
            set
            {
                if (_DragAndDropMode != value)
                {
                    _DragAndDropMode = value;
                    Recalculate();
                }
            }
        }
        #endregion

        #region IndentDepth
        private int _indentDepth;
        /// <summary>
        /// Gets or sets the distance to indent each child tree node level.
        /// </summary>
        [DefaultValue(30)]
        public int IndentDepth
        {
            get { return _indentDepth; }
            set
            {
                if (_indentDepth != value)
                {
                    _indentDepth = value;
                    Recalculate();
                }
            }
        }
        #endregion

        #region IndentWidth
        private int _indentWidth;
        /// <summary>
        /// Gets or sets the minimal distance between child tree nodes.
        /// </summary>
        [DefaultValue(10)]
        public int IndentWidth
        {
            get { return _indentWidth; }
            set
            {
                if (_indentWidth != value)
                {
                    _indentWidth = value;
                    Recalculate();
                }
            }
        }
        #endregion

        #region LinesPen
        private Pen _LinesPen;
        /// <summary>
        /// Gets or sets the Pen for drawing lines of this CTreeView
        /// </summary>
        [Browsable(false)]
        public Pen LinesPen
        {
            get { return _LinesPen; }
            set
            {
                _LinesPen.Dispose();
                _LinesPen = value;
                Refresh();
            }
        }
        #endregion

        #region BoundsSubtree
        /// <summary>
        /// The union of all child nodes bounds.
        /// </summary>
        [BrowsableAttribute(false)]
        public Rectangle BoundsSubtree { get; internal set; }

        /// <summary>
        /// Contains destination of dragged nodes.
        /// </summary>
        [BrowsableAttribute(false)]
        public DragTargetPositionClass DragTargetPosition { get; private set; }
        #endregion

        #endregion

        #region Methods

        #region CollapseAll, ExpandAll
        /// <summary>
        /// Collapses all the tree nodes.
        /// </summary>
        public void CollapseAll()
        {
            BeginUpdate();
            foreach (CTreeNode node in Nodes) node.CollapseAll();
            EndUpdate();
        }

        /// <summary>
        /// Expands all the tree nodes.
        /// </summary>
        public void ExpandAll()
        {
            BeginUpdate();
            foreach (CTreeNode node in Nodes) node.ExpandAll();
            EndUpdate();
        }
        #endregion

        #region GetNodeCount
        /// <summary>
        /// Retrieves the number of tree nodes, optionally including those in all subtrees, assigned to the CTreeView control.
        /// </summary>
        /// <param name="includeSubTrees">true  to count the CTreeNode items that the subtrees contain; otherwise, false.</param>
        /// <returns>The number of tree nodes, optionally including those in all subtrees, assigned to the CTreeView control.</returns>
        public int GetNodeCount(bool includeSubTrees)
        {
            int result = Nodes.Count;

            if (includeSubTrees)
            {
                foreach (CTreeNode node in Nodes) result += node.GetNodeCount(true);
            }

            return result;
        }
        #endregion

        #region GetNodeAt (2 methods)
        /// <summary>
        /// Retrieves the tree node that is at the specified point.
        /// </summary>
        /// <param name="pt">The Point to evaluate and retrieve the node from.</param>
        /// <returns>The CTreeNode at the specified point, in client coordinates, or null if there is no node at that location.</returns>
        public CTreeNode GetNodeAt(Point pt)
        {
            bool success = false;
            CTreeNode nodeAtPoint = null;

            Nodes.TraverseNodes(node => node.Visible && !success, node =>
            {
                if (node.Bounds.Contains(pt))
                {
                    nodeAtPoint = node;
                    success     = true;
                }
            });

            return nodeAtPoint;
        }

        /// <summary>
        /// Retrieves the tree node at the point with the specified coordinates.
        /// </summary>
        /// <param name="x">The X position to evaluate and retrieve the node from.</param>
        /// <param name="y">The Y position to evaluate and retrieve the node from.</param>
        /// <returns>The CTreeNode at the specified location, in CTreeView (client) coordinates, or null if there is no node at that location.</returns>
        public CTreeNode GetNodeAt(int x, int y)
        {
            return GetNodeAt(new Point(x,y));
        }
        #endregion

        #region ClearSelection
        /// <summary>
        /// 
        /// </summary>
        public void ClearSelection()
        {
            _selectedNodes.Clear();
            //Invalidate();
            //Update();
            Refresh();
        }
        #endregion

        #region BeginUpdate, EndUpdate
        /// <summary>
        /// Disables recalculating of the CTreeView.
        /// </summary>
        public void BeginUpdate()
        {
            this.SuspendLayout();
            suspendUpdateCount++;
        }

        /// <summary>
        /// Enables the recalculating of the CTreeView.
        /// </summary>
        public void EndUpdate()
        {
            this.ResumeLayout(false);
            if (suspendUpdateCount > 0)
            {
                suspendUpdateCount--;
                if (suspendUpdateCount == 0) Recalculate();
            }
        }
        #endregion

        #endregion

        #region Events
        /// <summary>
        /// Occurs when the tree node is collapsed.
        /// </summary>
        public event EventHandler<CTreeViewEventArgs> CollapseNode;

        /// <summary>
        /// Occurs when the tree node is expanded.
        /// </summary>
        public event EventHandler<CTreeViewEventArgs> ExpandNode;

        /// <summary>
        /// Occurs when the tree node is selected.
        /// </summary>
        public event EventHandler<CTreeViewEventArgs> SelectNode;
        #endregion
    }
}