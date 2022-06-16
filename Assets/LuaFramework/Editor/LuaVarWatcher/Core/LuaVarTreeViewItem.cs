using UnityEditor.IMGUI.Controls;

namespace LuaVarWatcher
{
    public class LuaVarTreeViewItem:TreeViewItem
    {
        public LuaVarTreeViewItem(int id, int depth, string displayName)
        {
            this.id = id;
            this.depth = depth;
            this.displayName = displayName;
        }

        public LuaNodeItem luaData;
    }
}