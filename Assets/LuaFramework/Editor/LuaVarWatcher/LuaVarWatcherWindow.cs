using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
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
            var L = LuaHandleInterface.GetLuaPtr();
            if (L != IntPtr.Zero)
            {
                GUILayout.Label("目标table路径：");
                mTargetTablePath = EditorGUILayout.TextField("", mTargetTablePath);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("扫描输入内容"))
                {
                    ScanTargetTable(L);
                }
                if (GUILayout.Button("全局表"))
                {
                    mTargetTablePath = "_G";
                    ScanTargetTable(L);
                }
                if(GUILayout.Button("执行输入指令"))
                {
                    LuaDLL.luaL_dostring(L, mTargetTablePath);
                }
                GUILayout.EndHorizontal();
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
                CheckInit();
            }
        }

        private void ScanTargetTable(IntPtr L)
        {
            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, mTargetTablePath);
            scanMap.Clear();
            var rootNode = LuaVarNodeParser.ParseLuaTable(L, scanMap);
            LuaDLL.lua_settop(L, oldTop);
            mLuaVarTreeView.luaNodeRoot = rootNode;
            mLuaVarTreeView.RootNodeName = mTargetTablePath;
            mLuaVarTreeView.Reload();
        }

        private void CheckInit()
        {

            bool firstInit = m_MultiColumnHeaderState == null;
            var headerState = LuaVarMultiColumnState.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            m_MultiColumnHeaderState = headerState;

            var multiColumnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            if (mLuaVarTreeView == null)
            {
                mLuaVarTreeView = new LuaVarTreeView(new LuaVarTreeViewState(), multiColumnHeader);
            }

            if (mSearchField == null)
            {
                mSearchField = new SearchField();
            }
        }

        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        SearchField m_SearchField;
        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }
  
    }
}
