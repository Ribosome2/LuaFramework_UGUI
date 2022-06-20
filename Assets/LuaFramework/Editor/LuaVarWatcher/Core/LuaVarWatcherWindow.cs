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

        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        SearchField m_SearchField;
        private CommonDropDownList mCommonDropDownList;
        private LuaCodeRunConsole mCodeRunConsole=new LuaCodeRunConsole();

        private float luaVarWidth = 400;
        Rect luaVarUiRect
        {
            get { return new Rect(0, 100, luaVarWidth, position.height - 100); }
        }

        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }

        Rect codeExecuteRect
        {
            get { return new Rect(luaVarUiRect.width+20, 100, position.width - luaVarWidth- 40, position.height - 60); }
        }



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
                GUILayout.BeginHorizontal();
                GUILayout.Label("目标table：",GUILayout.Width(80));

                mTargetTablePath = EditorGUILayout.TextField("", mTargetTablePath);
                if (EditorGUILayout.DropdownButton(new GUIContent(EditorGUIUtility.FindTexture("Favorite Icon")), FocusType.Passive, GUILayout.Width(30)))
                {
                    mCommonDropDownList= new CommonDropDownList("LuaVarCommonTableConfig.json");
                    mCommonDropDownList.ShowDropDown(delegate(object data) { mTargetTablePath = (string)data; });
                }
                mAutoRefresh =EditorGUILayout.Toggle("AutoRefresh", mAutoRefresh);
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
                if (GUILayout.Button("执行输入指令"))
                {
                    LuaDLL.luaL_dostring(L, mTargetTablePath);
                }
                GUILayout.EndHorizontal();


                mCodeRunConsole.OnGUI(codeExecuteRect,L);
            }
            else
            {
                GUILayout.Label("Can't find luaManager");
            }


            if (mLuaVarTreeView != null && mLuaVarTreeView.luaNodeRoot != null)
            {
                var searchLabelRect = new Rect(0, 60, 80, 30);
                GUI.Label(searchLabelRect, "搜索节点名：" );
                mLuaVarTreeView.searchString = mSearchField.OnGUI(new Rect(searchLabelRect.width, searchLabelRect.y, position.width, 30), mLuaVarTreeView.searchString);
                mLuaVarTreeView.OnGUI(luaVarUiRect);
            }
            else
            {
                CheckInit();
            }
        }




        private void ScanTargetTable(IntPtr L)
        {
            var startTime = EditorApplication.timeSinceStartup;
            var oldTop = LuaDLL.lua_gettop(L);
            LuaVarNodeParser.PushTargetTableToStack(L,mTargetTablePath);
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


  
    }
}
