using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlTreeView
{
    /// <summary>
    /// The container for CTreeNodeCollection.
    /// </summary>
    public interface INodeContainer
    {
        #region Properties

        #region Nodes
        /// <summary>
        /// Gets the collection of tree nodes that are assigned to this INodeContainer.
        /// </summary>
        CTreeNodeCollection Nodes { get; }
        #endregion

        #region FirstNode, LastNode
        /// <summary>
        /// Gets the first child tree node in the tree node collection.
        /// </summary>
        CTreeNode FirstNode { get; }

        /// <summary>
        /// Gets the last child tree node in the tree node collection.
        /// </summary>
        CTreeNode LastNode { get; }
        #endregion

        #region OwnerCTreeView
        /// <summary>
        /// Gets the TreeView that the CTreeNodeCollection of this INodeContainer is assigned to.
        /// </summary>
        CTreeView OwnerCTreeView { get; }
        #endregion

        #region BoundsSubtree
        /// <summary>
        /// Gets the bounds of this INodeContainer includes all child tree nodes.
        /// </summary>
        Rectangle BoundsSubtree { get; }
        #endregion

        #region Name
        /// <summary>
        /// Gets or sets the name of the INodeContainer.
        /// </summary>
        string Name { get; set; }
        #endregion

        #endregion

        #region Methods

        #region ExpandAll, CollapseAll
        /// <summary>
        /// Expands all the tree nodes of this INodeContainer.
        /// </summary>
        void ExpandAll();

        /// <summary>
        /// Collapses all the tree nodes of this INodeContainer.
        /// </summary>
        void CollapseAll();
        #endregion

        #region GetNodeCount
        /// <summary>
        /// Returns the number of child tree nodes.
        /// </summary>
        /// <param name="includeSubTrees">true  if the resulting count includes all tree nodes indirectly rooted at the tree node collection; otherwise, false.</param>
        /// <returns>The number of child tree nodes assigned to the CTreeNodeCollection.</returns>
        int GetNodeCount(bool includeSubTrees);
        #endregion

        #endregion
    }
}
