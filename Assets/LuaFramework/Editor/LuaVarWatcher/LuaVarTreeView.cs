using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace LuaVarWatcher
{
    public class LuaNodeItem
    {
        public string keyType;
        public string key;
        public string valueType;
        public string value;
    };

    public class LuaNode
    {
        public LuaNodeItem content = new LuaNodeItem();
        public int ID;
        public List<LuaNodeItem> childContents = new List<LuaNodeItem>();
        public List<LuaNode> childNodes = new List<LuaNode>();
    };





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

                foreach (var childContent in node.childContents)
                {
                    root.AddChild(new TreeViewItem(ID++,depth,childContent.key+": "+childContent.value));
                }

                depth++;
            }

            return root;
        }
    }
}