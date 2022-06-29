using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
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
        private TCPCodeServer mCodeServer =new TCPCodeServer();
        private EditorWindow mOwnerWindow;
        Queue<string> mMessageQueue = new Queue<string>();
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

        void OnDestroy()
        {
            Debug.Log("destroy");
        }
        public void OnGUI(Rect drawArea,IntPtr L ,EditorWindow ownerWindow)
        {
            mOwnerWindow = ownerWindow;
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
                reloadRecorder.ShowCodeExecuteDropDown( delegate (object content) { reloadPath = content as string; });
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
                contentRecorder.ShowCodeExecuteDropDown(delegate(object content) { executeCodeBlock = content as string; });
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
                    Debug.LogError("执行错误: " + LuaDLL.lua_tostring(L,-1));
                    LuaDLL.lua_settop(L,oldTop);
                }
            }
            GUILayout.EndHorizontal();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            executeCodeBlock = EditorGUILayout.TextArea(executeCodeBlock, GUILayout.Height(drawArea.height - 120));
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:",GUILayout.Width(50));
            mCodeServer.IP= GUILayout.TextField(mCodeServer.IP);
            if (EditorGUILayout.DropdownButton(new GUIContent("选IP"), FocusType.Passive, GUILayout.Width(70), GUILayout.Height(35)))
            {
                SelectStartIP(delegate (object content) { mCodeServer.IP = content as string; });
            }

            if (!mCodeServer.IsServerStarted)
            {
                if (GUILayout.Button("StartServer",GUILayout.Height(30)))
                {
                    mCodeServer.Start();
                    mCodeServer.SetClientMsgCallBack(this.ClientMsgCallBack);
                }
            }
            else
            {
                if (GUILayout.Button("ShutDown", GUILayout.Height(30)))
                {
                    mCodeServer.ShutDown();
                }
            }
           
            if (GUILayout.Button("SendMsg", GUILayout.Height(30)))
            {
                mCodeServer.SendMessage(executeCodeBlock);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        public void Update()
        {
            if (EditorApplication.isCompiling)
            {
                mCodeServer.ShutDown();
            }

            while (mMessageQueue.Count>0)
            {
                var msg = mMessageQueue.Dequeue();
                if (mOwnerWindow != null)
                {
                    mOwnerWindow.ShowNotification(new GUIContent(msg));
                }
            }
        }

        void ClientMsgCallBack(string msg)
        {
            mMessageQueue.Enqueue(msg);
           
        }



        void SelectStartIP(GenericMenu.MenuFunction2 func)
        {
            GenericMenu menu = new GenericMenu();
            List<string> ipList = new List<string>();
            var hostName = Dns.GetHostName();
            var regex = new Regex("[\\d]+\\.[\\d]+\\.[\\d]+\\.[\\d]+");
            foreach (var ipAddress in Dns.GetHostEntry(hostName).AddressList)
            {
                var ipStr = ipAddress.ToString();
                if (regex.IsMatch(ipStr))
                {
                    ipList.Add(ipStr);
                }
            }
            ipList.Add("127.0.0.1");

            foreach (var content in ipList)
            {
                menu.AddItem(new GUIContent(content), false, func, content);
            }
            menu.ShowAsContext();
        }
    }
}