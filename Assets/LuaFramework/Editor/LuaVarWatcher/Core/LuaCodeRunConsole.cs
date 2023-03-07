using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using LuaInterface;
using RemoteCodeControl;
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
        private LRUContentRecorder searchRecord;
        private GUIContent debugContent;
        private TCPCodeServer mCodeServer =new TCPCodeServer();
        private EditorWindow mOwnerWindow;
        Queue<string> mMessageQueue = new Queue<string>();
        private const int quickActionWidth = 100;
        private QuickActionsConfig quickActionsConfig;
        public bool showQuickAction=true;
        public Vector2 quickActionScroll;
        public LuaCodeRunConsole()
        {
            contentRecorder = new LRUContentRecorder("LuaDebugCache/RecentExecuteCode.json");
            reloadRecorder = new LRUContentRecorder("LuaDebugCache/RecentReloadCode.json");
            searchRecord = new LRUContentRecorder("LuaDebugCache/RecentSearchNode.json");
            quickActionsConfig = new QuickActionsConfig("Tools/LuaCodeTool/QuickAction.json");

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
            var consoleRect = drawArea;
            if (showQuickAction)
            {
                consoleRect.width = drawArea.width - quickActionWidth;
                var quickActionRect = consoleRect;
                quickActionRect.x = consoleRect.xMax;
                quickActionRect.width = quickActionWidth;
                DrawQuickActions(quickActionRect, L);
            }
            DrawMainContent( L, ownerWindow, consoleRect);
           
        }

        void DrawQuickActions(Rect drawRect, IntPtr L)
        {
            GUILayout.BeginArea(drawRect);
            GUILayout.Label("快捷菜单:");
            quickActionScroll=GUILayout.BeginScrollView(quickActionScroll);
            if (quickActionsConfig != null)
            {
                foreach (var actionsItem in quickActionsConfig.ActionList)
                {
                    if (string.IsNullOrEmpty(actionsItem.Name))
                    {
                        GUILayout.Space(actionsItem.Height);
                    }
                    else
                    {
                        if (GUILayout.Button(actionsItem.Name,
                            GUILayout.Height(actionsItem.Height)))
                        {
                            TryExecuteCode(L, actionsItem.TargetCode);
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        static void TryExecuteCode(IntPtr L,string codeContent)
        {
            if (L != IntPtr.Zero)
            {
                var oldTop = LuaDLL.lua_gettop(L);
                if (!LuaDLL.luaL_dostring(L, codeContent))
                {
                    Debug.LogError("执行错误: " + LuaDLL.lua_tostring(L, -1));
                    LuaDLL.lua_settop(L, oldTop);
                }
            }
        }


        private void DrawMainContent( IntPtr L, EditorWindow ownerWindow, Rect consoleRect)
        {
            GUILayout.BeginArea(consoleRect);
            GUILayout.BeginHorizontal();
            if (debugContent == null)
            {
                debugContent = new GUIContent();
                debugContent.text = "Debugger";
                debugContent.image = Resources.Load<Texture2D>("attachDebugIcon");
            }

          
            GUILayout.EndHorizontal();

            if (GUILayout.Button(debugContent, GUILayout.Width(100), GUILayout.Height(30)))
            {
                LuaHandleInterface.ConnectDebugger();
            }

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("重载路径：", reloadPath, GUILayout.Width(70));
            if (EditorGUILayout.DropdownButton(new GUIContent(EditorGUIUtility.FindTexture("Favorite Icon")), FocusType.Passive,
                GUILayout.Width(30), GUILayout.Height(35)))
            {
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
                reloadRecorder.ShowCodeExecuteDropDown(delegate(object content) { reloadPath = content as string; });
            }

            reloadPath = EditorGUILayout.TextField("", reloadPath);

            if (GUILayout.Button("Reload", GUILayout.Width(100)))
            {
                LuaHandleInterface.ReloadLuaFile(reloadPath);
                reloadRecorder.AddUseRecord(reloadPath);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("I Don't Care");
            if (EditorGUILayout.DropdownButton(new GUIContent(EditorGUIUtility.FindTexture("Favorite Icon")), FocusType.Passive,
                GUILayout.Width(30), GUILayout.Height(35)))
            {
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
                contentRecorder.ShowCodeExecuteDropDown(delegate(object content)
                {
                    executeCodeBlock = content as string;
                    ownerWindow.Repaint();
                });
            }

            if (GUILayout.Button("执行", GUILayout.Height(25)))
            {
                if (L != IntPtr.Zero)
                {
                    var oldTop = LuaDLL.lua_gettop(L);
                    if (LuaDLL.luaL_dostring(L, executeCodeBlock))
                    {
                        contentRecorder.AddUseRecord(executeCodeBlock);
                    }
                    else
                    {
                        Debug.LogError("执行错误: " + LuaDLL.lua_tostring(L, -1));
                        LuaDLL.lua_settop(L, oldTop);
                    }
                }
            }
            GUILayout.Label("快捷指令：",GUILayout.Width(50));
            showQuickAction =GUILayout.Toggle( showQuickAction,"", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            executeCodeBlock = EditorGUILayout.TextArea(executeCodeBlock, GUILayout.Height(consoleRect.height - 120));
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:", GUILayout.Width(50));
            mCodeServer.IP = GUILayout.TextField(mCodeServer.IP);
            if (EditorGUILayout.DropdownButton(new GUIContent("选IP"), FocusType.Passive, GUILayout.Width(70),
                GUILayout.Height(35)))
            {
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
                SelectStartIP(delegate(object content) { mCodeServer.IP = content as string; });
            }


            var serverButtonHeight = 20;
            if (!mCodeServer.IsServerStarted)
            {
                if (GUILayout.Button("StartServer", GUILayout.Height(serverButtonHeight)))
                {
                    mCodeServer.Start();
                    mCodeServer.SetClientMsgCallBack(this.ClientMsgCallBack);
                }
            }
            else
            {
                if (GUILayout.Button("ShutDown", GUILayout.Height(serverButtonHeight)))
                {
                    mCodeServer.ShutDown();
                }
            }

            if (GUILayout.Button("SendMsg", GUILayout.Height(serverButtonHeight)))
            {
                mCodeServer.SendMessage(JsonUtility.ToJson(new RemoteCodeControl.RemoteCodeControlMessage()
                {
                    Content = executeCodeBlock,
                    ID = (int) RemoteCodeControlMessageType.Command
                }));
            }

            if (GUILayout.Button("RemoteReload", GUILayout.Height(serverButtonHeight)))
            {
                TryReloadInRemote(reloadPath);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void TryReloadInRemote(string luaFileName)
        {
            var guids = AssetDatabase.FindAssets(luaFileName);
            List<string> targetList = new List<string>();
            foreach (string guid1 in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid1);
                if (Path.GetExtension(assetPath) == ".lua" &&
                    Path.GetFileNameWithoutExtension(assetPath) == luaFileName)
                {
                    targetList.Add(assetPath);
                }
            }

            if (targetList.Count == 0)
            {
                mOwnerWindow.ShowNotification(new GUIContent("没有找到文件"+luaFileName));
            }
            else
            {
                if (targetList.Count > 1)
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (var targetPath in targetList)
                    {
                        menu.AddItem(new GUIContent(targetPath), false, delegate(object obj)
                        {
                            SendReloadContentToRemote(obj as string);
                        }, targetPath);
                    }
                    menu.ShowAsContext();
                }
                else
                {
                    var targetPath = targetList[0];
                    SendReloadContentToRemote(targetPath);
                }
            }
        }

        private void SendReloadContentToRemote(string targetPath)
        {
            var fileContent = File.ReadAllText(targetPath);

            mCodeServer.SendMessage(JsonUtility.ToJson(new RemoteCodeControl.RemoteCodeControlMessage()
            {
                Content = fileContent,
                ID = (int) RemoteCodeControlMessageType.ReloadFileContent
            }));
        }

        static string GetTargetLuaPath(string relativePath, string luaFileName)
        {
            var guids = AssetDatabase.FindAssets(luaFileName);
            foreach (string guid1 in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid1);
                if (Path.GetExtension(assetPath) == ".lua" &&
                    Path.GetFileNameWithoutExtension(assetPath) == luaFileName
                    && assetPath.Contains(relativePath)
                )
                {
                    return assetPath;
                }
            }
            //todo use relativePath to choose  multiple files with same name
            return null;
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