using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LuaVarWatcher
{

    public class LuaVarTreeView:TreeView
    {
        public LuaNode luaNodeRoot;
        public LuaVarTreeView(TreeViewState state) : base(state)
        {
        }

        public LuaVarTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }



        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1,-1,"VarNodeWatcher:");
            var queue =new  Queue<LuaNode>();
            queue.Enqueue(luaNodeRoot);
            int ID = 1;
            int depth = 0;
            while (queue.Count>0)
            {
                var node = queue.Dequeue();
                foreach (var childNode in node.childNodes)
                {
                    queue.Enqueue(childNode);
                }

                var contentNode = new TreeViewItem(ID++, depth, node.content.GetDisplayName());
                root.AddChild(contentNode);
                Debug.Log("Add "+contentNode.displayName);

                foreach (var childContent in node.childContents)
                {
                    var childContentItem = new TreeViewItem(ID++, depth, childContent.GetDisplayName());
                    root.AddChild(childContentItem);
                    Debug.Log("Add " + contentNode.displayName);
                }

                depth++;
            }

            return root;
        }
    }
}