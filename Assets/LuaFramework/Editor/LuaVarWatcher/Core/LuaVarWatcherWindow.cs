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

        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }




        private Dictionary<string, LuaNode> scanMap = new Dictionary<string, LuaNode>();
        private string mTargetTablePath = "myTable";
        LuaVarTreeView mLuaVarTreeView;
        private SearchField mSearchField;
        private MultiColumnHeader mMultiColumnHeader;
        private bool mAutoRefresh;
        private float lastScanTime;
        private float scanInterval = 0.1f;

        private void OnEditorUpdate()
        {
            if (mAutoRefresh && EditorApplication.timeSinceStartup - lastScanTime > scanInterval &&
                LuaHandleInterface.GetLuaPtr() != IntPtr.Zero && !string.IsNullOrEmpty(mTargetTablePath))
            {
                ScanTargetTable(LuaHandleInterface.GetLuaPtr());
            }
        }

        void OnGUI()
        {
            var L = LuaHandleInterface.GetLuaPtr();
            if (L != IntPtr.Zero)
            {
                GUILayout.Label("目标table路径：");
                GUILayout.BeginHorizontal();
                mTargetTablePath = EditorGUILayout.TextField("", mTargetTablePath);
                mAutoRefresh=EditorGUILayout.Toggle("AutoRefresh", mAutoRefresh);
                GUILayout.EndHorizontal();
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


        void PushTargetTableToStack(IntPtr L,string dataPath)
        {
            var prefixes = dataPath.Split('.');
            if (prefixes.Length == 1)
            {
                LuaDLL.lua_getglobal(L, dataPath);
                if (!LuaDLL.lua_istable(L, -1))
                {
                    Debug.LogError("不存在table " + dataPath);
                }
            }
            else
            {
                LuaDLL.lua_getglobal(L, prefixes[0]);
                if (!LuaDLL.lua_istable(L, -1))
                {
                    Debug.LogError("不存在table "+prefixes[0]+ " top type "+ LuaDLL.luaL_typename(L,-1));
                    return;
                }

                var validPrefix = prefixes[0];
                for (int i = 1; i < prefixes.Length; i++)
                {
                    LuaDLL.lua_getfield(L, -1, prefixes[i]);
                    validPrefix += "." + prefixes[i];
                    if (LuaDLL.lua_type(L, -1) == LuaTypes.LUA_TNIL)
                    {
                        Debug.LogError(validPrefix + "是不存在的Table " + LuaDLL.lua_type(L, -1));
                        break;
                    }
                }
            }
        }

        private void ScanTargetTable(IntPtr L)
        {
            var startTime = EditorApplication.timeSinceStartup;
            var oldTop = LuaDLL.lua_gettop(L);
            PushTargetTableToStack(L,mTargetTablePath);
            scanMap.Clear();
            var rootNode = LuaVarNodeParser.ParseLuaTable(L, scanMap);
            LuaDLL.lua_settop(L, oldTop);
            mLuaVarTreeView.luaNodeRoot = rootNode;
            mLuaVarTreeView.RootNodeName = mTargetTablePath;
            mLuaVarTreeView.Reload();

            var scanTime = EditorApplication.timeSinceStartup-startTime;
            if (scanTime > 0.5 && mAutoRefresh)
            {
                mAutoRefresh = false;
                ShowNotification(new GUIContent("单次扫描太久，退出自动刷新"));
            }
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
