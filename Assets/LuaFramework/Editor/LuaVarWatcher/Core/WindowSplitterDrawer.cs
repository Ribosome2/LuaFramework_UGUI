using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LuaVarWatcher
{
    public class GuiStyleHelper
    {

        public GUIStyle bottomBarBg = (GUIStyle)"ProjectBrowserBottomBarBg";
        public GUIStyle topBarBg = (GUIStyle)"ProjectBrowserTopBarBg";
        public GUIStyle selectedPathLabel = (GUIStyle)"Label";
        public GUIStyle lockButton = (GUIStyle)"IN LockButton";
        public GUIStyle foldout = (GUIStyle)"AC RightArrow";
        public GUIContent m_FilterByLabel = new GUIContent((Texture)EditorGUIUtility.FindTexture("FilterByLabel"), "Search by Label");
        public GUIContent m_FilterByType = new GUIContent((Texture)EditorGUIUtility.FindTexture("FilterByType"), "Search by Type");
        public GUIContent m_CreateDropdownContent = new GUIContent("Create");
        public GUIContent m_SaveFilterContent = new GUIContent((Texture)EditorGUIUtility.FindTexture("Favorite"), "Save search");
        public GUIContent m_EmptyFolderText = new GUIContent("This folder is empty");
        public GUIContent m_SearchIn = new GUIContent("Search:");

        private static GUIStyle GetStyle(string styleName)
        {
            return (GUIStyle)styleName;
        }
    }
    [System.Serializable]
    public class WindowSplitterDrawer
    {
        private EditorWindow _window;

        public void SetOwnerWindow(EditorWindow window)
        {
            _window = window;
        }

        private float m_LastListWidth;
        private float k_MinDirectoriesAreaWidth = 110f;
        public float m_DirectoriesAreaWidth = 215f;
        private float m_ToolbarHeight = 100f;
        public Action onSplitSizeChange;

        [NonSerialized] public Rect CodeExecuteRect;
        [NonSerialized] public Rect TreeViewRect;

        private GuiStyleHelper _guiStyleHelper;
        public void OnGUI()
        {
            if (_guiStyleHelper == null)
            {
                _guiStyleHelper =new GuiStyleHelper();
            }
            CalculateRects();
            ResizeHandling(_window, _window.position.height - this.m_ToolbarHeight);
            SplitterGuiUtil.DrawHorizontalSplitter(new Rect(this.CodeExecuteRect.x, this.m_ToolbarHeight, 1f,
                this.TreeViewRect.height));
        }

        public void ResizeHandling(EditorWindow window, float height)
        {

            Rect dragRect = new Rect(this.m_DirectoriesAreaWidth, this.m_ToolbarHeight, 5f, height);
            dragRect = SplitterGuiUtil.HandleHorizontalSplitter(dragRect, window.position.width,
                this.k_MinDirectoriesAreaWidth, 230f - this.k_MinDirectoriesAreaWidth);
            this.m_DirectoriesAreaWidth = dragRect.x;
            float num = window.position.width - this.m_DirectoriesAreaWidth;
            if (Math.Abs((double) num - (double) this.m_LastListWidth) > Mathf.Epsilon)
            {
                if (onSplitSizeChange != null)
                {
                    onSplitSizeChange();
                }
            }

            this.m_LastListWidth = num;
        }

        private void CalculateRects()
        {
            float bottomBarHeight = 0;

            float width = _window.position.width - this.m_DirectoriesAreaWidth;
            this.CodeExecuteRect = new Rect(this.m_DirectoriesAreaWidth, 5 , width,_window.position.height   -bottomBarHeight);
            this.TreeViewRect = new Rect(0.0f, this.m_ToolbarHeight, this.m_DirectoriesAreaWidth,_window.position.height - this.m_ToolbarHeight);
           
        }
      
      
    }
}