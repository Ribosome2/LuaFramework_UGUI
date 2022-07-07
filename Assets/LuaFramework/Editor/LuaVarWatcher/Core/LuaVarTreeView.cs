using System.Collections.Generic;
using LuaInterface;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LuaVarWatcher
{
    public class LuaVarTreeView : TreeView
    {
        public LuaNode luaNodeRoot;
        public string RootNodeName;
        public bool IgnoreFunction = true;
        int ID = 0;
        Dictionary<LuaNode, int> mUniqueNodeMap = new Dictionary<LuaNode, int>();

        public string GetSingleSelectItemPath()
        {
            if (this.state.selectedIDs.Count == 1)
            {
                var id = this.state.selectedIDs[0];
                var selectedItem = FindItem(id, rootItem);
                if (selectedItem != null)
                {
                    var path = "";
                    int index = 0;
                    while (selectedItem != null && selectedItem is LuaVarTreeViewItem)
                    {
                        var luaNodeItem = selectedItem as LuaVarTreeViewItem;
                        if (index == 0)
                        {
                            path = luaNodeItem.luaData.key;
                        }
                        else
                        {
                            path = string.Format("{0}.{1}", luaNodeItem.luaData.key, path);
                        }
                        selectedItem = selectedItem.parent;
                        index++;
                    }
                    path = string.Format("{0}.{1}", RootNodeName, path);
                    return path;
                }
            }

            return string.Empty;
        }

        private GUIContent cycleRefJumpBtnContent;
        public LuaVarTreeView(TreeViewState state) : base(state)
        {
           
        }

        public LuaVarTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            cycleRefJumpBtnContent = new GUIContent(Resources.Load<Texture2D>("theSnakeCycle"));
            cycleRefJumpBtnContent.text = "↑";
            cycleRefJumpBtnContent.tooltip = "跳转到引用节点";
        }


        void AddVarNode(TreeViewItem parentItem, LuaNode node)
        {
            foreach (var childContent in node.childContents)
            {
                if (IgnoreFunction && childContent.luaValueType == LuaTypes.LUA_TFUNCTION)
                {
                    continue;
                }

                var childContentItem =
                    new LuaVarTreeViewItem(ID++, parentItem.depth + 1, childContent.GetDisplayName());
                childContentItem.luaData = childContent;
                parentItem.AddChild(childContentItem);
            }
            mUniqueNodeMap[node] = parentItem.id;
            foreach (var childNode in node.childNodes)
            {
                if (mUniqueNodeMap.ContainsKey(childNode))
                {
                    var childContentItem =
                        new LuaVarTreeViewItem(ID++, parentItem.depth + 1, "ref table" + childNode.content.value);
                    childContentItem.luaData = childNode.content;
                    childContentItem.cycleRefNode = childNode;
                    parentItem.AddChild(childContentItem);
                }
                else
                {
                    var childContentItem =
                        new LuaVarTreeViewItem(ID++, parentItem.depth + 1, childNode.content.GetDisplayName());
                    childContentItem.luaData = childNode.content;
                    parentItem.AddChild(childContentItem);
                    mUniqueNodeMap[childNode] = childContentItem.id;
                    AddVarNode(childContentItem, childNode);

                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1, "VarNodeWatcher:");
            ID = 0;
            var firstShowNode = new TreeViewItem(ID++, 0, RootNodeName);
            SetExpanded(firstShowNode.id, true);
            root.AddChild(firstShowNode);
            if (luaNodeRoot != null)
            {
                firstShowNode.displayName = firstShowNode.displayName + " :0x" + luaNodeRoot.content.value;
                AddVarNode(firstShowNode, luaNodeRoot);
            }

            return root;
        }


        void ExpandAllParent(int id)
        {
            var item = FindItem(id, rootItem);
            while (item != null && item.parent != null)
            {
                this.SetExpanded(item.parent.id, true);
                item = item.parent;
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            searchString = "";
            SetSelection(new List<int>() {id});
            this.SetExpanded(id, true);
            ExpandAllParent(id);
            SetFocusAndEnsureSelectedItem();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as LuaVarTreeViewItem;
            if (item != null)
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, (LuaVarColumns) args.GetColumn(i), ref args);
                }
            }
            else
            {
                base.RowGUI(args);
            }
        }

        void CellGUI(Rect cellRect, LuaVarTreeViewItem item, LuaVarColumns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case LuaVarColumns.VarName:
                {
                    cellRect.x += GetContentIndent(item);
                    if (item.cycleRefNode != null)
                    {
                        var jumpBtnRect = new Rect(cellRect.x, cellRect.y, 40, cellRect.height);
                        if (GUI.Button(jumpBtnRect, cycleRefJumpBtnContent))
                        {
                            DoubleClickedItem(mUniqueNodeMap[item.cycleRefNode]);
                        }

                        cellRect.x += jumpBtnRect.width;
                        GUI.Label(cellRect, item.luaData.GetDisplayName().ToString());
                       
                    }
                    else
                    {
                        GUI.Label(cellRect, item.luaData.GetDisplayName().ToString());
                    }

                    break;
                }
                case LuaVarColumns.VarType:
                {
                    GUI.Label(cellRect, item.luaData.luaValueType.ToString());
                    break;
                }
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item != null)
            {
                var luaItem = item as LuaVarTreeViewItem;
                if (luaItem != null && luaItem.luaData.luaValueType==LuaTypes.LUA_TFUNCTION)
                {
                    GenericMenu menu =new GenericMenu();
                    menu.AddItem(new GUIContent("监测调用"), false,delegate()
                    {

                    });
                    menu.ShowAsContext();
                }
            }
            base.ContextClickedItem(id);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
        }
    }
}