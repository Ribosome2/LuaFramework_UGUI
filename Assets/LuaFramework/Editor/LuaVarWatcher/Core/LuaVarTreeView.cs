using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LuaVarWatcher
{

    public class LuaVarTreeView:TreeView
    {
        public LuaNode luaNodeRoot;
        public string  RootNodeName;
        public LuaVarTreeView(TreeViewState state) : base(state)
        {
        }

        public LuaVarTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        int ID = 0;

        void AddVarNode(TreeViewItem parentItem, LuaNode node)
        {
            var contentNode = new TreeViewItem(ID++, parentItem.depth+1, node.content.GetDisplayName());
            parentItem.AddChild(contentNode);
            foreach (var childContent in node.childContents)
            {
                var childContentItem = new TreeViewItem(ID++, contentNode.depth, childContent.GetDisplayName());
                parentItem.AddChild(childContentItem);
            }
            foreach (var childNode in node.childNodes)
            {
                AddVarNode(contentNode,childNode);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1,-1,"VarNodeWatcher:");
            var firstShowNode = new TreeViewItem(ID++,0,RootNodeName);
            root.AddChild(firstShowNode);
            if (luaNodeRoot != null)
            {
                AddVarNode(firstShowNode, luaNodeRoot);
            }
            return root;
        }

    }
}