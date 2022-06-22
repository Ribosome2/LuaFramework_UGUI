﻿using System.Collections.Generic;
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
        public bool IgnoreFunction=true;

        public LuaVarTreeView(TreeViewState state) : base(state)
        {
        }

        public LuaVarTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        int ID = 0;

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

            foreach (var childNode in node.childNodes)
            {
                var childContentItem =
                    new LuaVarTreeViewItem(ID++, parentItem.depth + 1, childNode.content.GetDisplayName());
                childContentItem.luaData = childNode.content;
                parentItem.AddChild(childContentItem);
                AddVarNode(childContentItem, childNode);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1, "VarNodeWatcher:");
            ID = 0;
            var firstShowNode = new TreeViewItem(ID++, 0, RootNodeName);
            SetExpanded(firstShowNode.id,true);
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
            var item = FindItem(id,rootItem);
            while (item!=null && item.parent!=null)
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
            var item =  args.item as LuaVarTreeViewItem;
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
                    GUI.Label(cellRect, item.luaData.GetDisplayName().ToString());
                    break;
                }
                case LuaVarColumns.VarType:
                {
                    GUI.Label(cellRect,item.luaData.luaValueType.ToString());
                    break;
                }
            }
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