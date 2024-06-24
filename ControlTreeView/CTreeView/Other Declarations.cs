using System;
using System.Drawing;

namespace ControlTreeView
{
    /// <summary>
    /// The draw style of CTreeView.
    /// </summary>
    public enum CTreeViewDrawStyle
    {
        /// <summary>Linear Tree Style</summary>
        LinearTree,
        /// <summary>Horizontal Tree Style</summary>
        HorizontalDiagram,
        /// <summary>Vertical Tree Style</summary>
        VerticalDiagram
    }

    /// <summary>
    /// The selection mode of CTreeView.
    /// </summary>
    public enum CTreeViewSelectionMode
    {
        /// <summary>Multi selection mode</summary>
        Multi,
        /// <summary>Multi Same Parent selection mode</summary>
        MultiSameParent,
        /// <summary>Single selection mode</summary>
        Single,
        /// <summary>No selection mode</summary>
        None
    }

    /// <summary>
    /// The DragAndDrop mode of CTreeView.
    /// </summary>
    public enum CTreeViewDragAndDropMode
    {
        /*CopyReplaceReorder,*/
        /// <summary>Replace Reorder DragAndDrop mode</summary>
        ReplaceReorder,
        /// <summary>Reorder DragAndDrop mode</summary>
        Reorder,
        /// <summary>No DragAndDrop mode</summary>
        Nothing
    }

    /// <summary>
    /// The bitmaps for plus and minus buttons of nodes.
    /// </summary>
    public struct CTreeViewPlusMinus
    {
        private Bitmap _Plus;
        /// <summary>Plus button bitmap</summary>
        public Bitmap Plus
        {
            get { return _Plus; }
        }

        private Bitmap _Minus;
        /// <summary>Minus button bitmap</summary>
        public Bitmap Minus
        {
            get { return _Minus; }
        }

        private Size _Size;
        internal Size Size
        {
            get { return _Size; }
        }

        /// <summary>Plus Minus buttons constructor</summary>
        public CTreeViewPlusMinus(Bitmap plus, Bitmap minus)
        {
            _Size = plus.Size;

            if (_Size != minus.Size)
                throw new ArgumentException("Images are of different sizes");

            _Plus  = plus;
            _Minus = minus;
        }
    }
}