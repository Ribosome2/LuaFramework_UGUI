using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = System.Object;

namespace LuaVarWatcher
{
    public class LuaVarWatcherWindow : EditorWindow
    {
        [MenuItem("KyleKit/LuaVarWatcher")]
        public static void OpenWindow()
        {
            GetWindow<LuaVarWatcherWindow>();
        }


        private Dictionary<string, LuaNode> scanMap = new Dictionary<string, LuaNode>();
        private string mTargetTablePath = "myTable";
        LuaVarTreeView mLuaVarTreeView;
        private SearchField mSearchField;
        private MultiColumnHeader mMultiColumnHeader;
        void OnGUI()
        {
            var luaHandle = LuaHandleInterface.GetLuaPtr();
            if (luaHandle != IntPtr.Zero)
            {
                GUILayout.Label("目标table路径：");
                mTargetTablePath = EditorGUILayout.TextField("", mTargetTablePath);
                if (GUILayout.Button("ShowMe"))
                {
                    var L = luaHandle;
                    var oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_getglobal(L, mTargetTablePath);
                    scanMap.Clear();
                    var rootNode = LuaVarNodeParser.ParseLuaTable(L, scanMap);
                    LuaDLL.lua_settop(L, oldTop);
                    mLuaVarTreeView.luaNodeRoot = rootNode;
                    mLuaVarTreeView.RootNodeName = mTargetTablePath;
                    mLuaVarTreeView.Reload();
                }
            }
            else
            {
                GUILayout.Label("Can't find luaManager");
            }


            if (mLuaVarTreeView != null && mLuaVarTreeView.luaNodeRoot != null)
            {
                mLuaVarTreeView.searchString = mSearchField.OnGUI(new Rect(0, 60, position.width, 30), mLuaVarTreeView.searchString);
                mLuaVarTreeView.OnGUI(new Rect(0, 100, position.width, position.height - 100));
            }
            else
            {
                if (mLuaVarTreeView == null)
                {
                    mLuaVarTreeView = new LuaVarTreeView(new LuaVarTreeViewState());
                }

                if (mSearchField == null)
                {
                    mSearchField = new SearchField();
                }
            }
        }
    }
}
