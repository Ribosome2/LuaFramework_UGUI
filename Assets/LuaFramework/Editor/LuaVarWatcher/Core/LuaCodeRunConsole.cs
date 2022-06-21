using System;
using System.IO;
using LuaInterface;
using UnityEditor;
using UnityEngine;

namespace LuaVarWatcher
{
    public class LuaCodeRunConsole
    {
        private string reloadPath = "";
        private string executeCodeBlock = "";
        Vector2 scroll;

        private LRUContentRecorder contentRecorder;
        private LRUContentRecorder reloadRecorder;
        private GUIContent debugContent;
        public LuaCodeRunConsole()
        {
            contentRecorder = new LRUContentRecorder("LuaDebugCache/RecentExecuteCode.json");
            reloadRecorder = new LRUContentRecorder("LuaDebugCache/RecentReloadCode.json");

            var lastReload = reloadRecorder.GetLastUseContent();
            if (string.IsNullOrEmpty(lastReload) == false)
            {
                reloadPath = lastReload;
            }

            var lastExecute = contentRecorder.GetLastUseContent();
            if (string.IsNullOrEmpty(lastExecute) == false)
            {
                executeCodeBlock = lastExecute;
            }
        }
        public void OnGUI(Rect drawArea,IntPtr L )
        {
            GUILayout.BeginArea(drawArea);
            if (debugContent == null)
            {
                debugContent = new GUIContent();
                debugContent.text = "Debugger";
                debugContent.image = Resources.Load<Texture2D>("attachDebugIcon");
            }
            if (GUILayout.Button(debugContent, GUILayout.Width(100),GUILayout.Height(30)))
            {
                LuaHandleInterface.ConnectDebugger();
            }
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("重置路径：", reloadPath,GUILayout.Width(70));
            if (EditorGUILayout.DropdownButton(new GUIContent(EditorGUIUtility.FindTexture("Favorite Icon")), FocusType.Passive, GUILayout.Width(30), GUILayout.Height(35)))
            {
                ShowCodeExecuteDropDown(reloadRecorder, delegate (object content) { reloadPath = content as string; });
            }

            reloadPath = EditorGUILayout.TextField("", reloadPath);

            if (GUILayout.Button("Reload", GUILayout.Width(100)))
            {
                LuaHandleInterface.ReloadLuaFile(reloadPath);
                reloadRecorder.AddUseRecord(reloadPath);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (EditorGUILayout.DropdownButton(new GUIContent(EditorGUIUtility.FindTexture("Favorite Icon")), FocusType.Passive, GUILayout.Width(30), GUILayout.Height(35)))
            {
                ShowCodeExecuteDropDown(contentRecorder,delegate(object content) { executeCodeBlock = content as string; });
            }
          
            if (GUILayout.Button("执行", GUILayout.Height(35)))
            {
                var oldTop = LuaDLL.lua_gettop(L);
                if (LuaDLL.luaL_dostring(L, executeCodeBlock))
                {
                    contentRecorder.AddUseRecord(executeCodeBlock);
                }
                else
                {
                    Debug.LogError("执行错误 " +executeCodeBlock);
                    LuaDLL.lua_settop(L,oldTop);
                }
            }
            GUILayout.EndHorizontal();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            executeCodeBlock = EditorGUILayout.TextArea(executeCodeBlock, GUILayout.Height(drawArea.height - 130));
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

        }


        public void ShowCodeExecuteDropDown(LRUContentRecorder recorder,GenericMenu.MenuFunction2 func )
        {
            GenericMenu menu = new GenericMenu();
            foreach (var content in recorder.GetContentList())
            {
                menu.AddItem(new GUIContent(content), false, func, content);
            }
            menu.ShowAsContext();
        }

    }
}