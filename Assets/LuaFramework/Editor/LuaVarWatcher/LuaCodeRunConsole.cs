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
        public void OnGUI(Rect drawArea,IntPtr L )
        {
            GUILayout.BeginArea(drawArea);
            if (GUILayout.Button("Debug", GUILayout.Width(100)))
            {
            }
            GUILayout.BeginHorizontal();

            reloadPath = EditorGUILayout.TextField("重置路径：", reloadPath);

            if (GUILayout.Button("Reload", GUILayout.Width(100)))
            {
                LuaHandleInterface.ReloadLuaFile(reloadPath);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("执行", GUILayout.Height(50)))
            {
                LuaDLL.luaL_dostring(L, executeCodeBlock);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            executeCodeBlock = EditorGUILayout.TextArea(executeCodeBlock, GUILayout.Height(drawArea.height - 130));
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

        }
    }
}