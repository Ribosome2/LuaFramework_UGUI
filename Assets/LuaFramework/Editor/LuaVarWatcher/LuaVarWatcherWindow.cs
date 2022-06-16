using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LuaFramework;
using LuaInterface;
using UnityEditor;
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


        static LuaNode ParseLuaTable(IntPtr L, Dictionary<string, LuaNode> scanMap)
        {
            if (!LuaDLL.lua_istable(L, -1))
            {
                return null;
            }

            var ptr = LuaDLL.lua_topointer(L, -1);
            var tableStr = ptr.ToString();
            if (scanMap.ContainsKey(tableStr))
            {
                return null;
            }

            LuaNode luaNode = new LuaNode();
            scanMap[tableStr] = luaNode;

            LuaDLL.lua_pushnil(L);
            var top = LuaDLL.lua_gettop(L);
            while (LuaDLL.lua_next(L, -2) > 0)
            {
                var keyTypeStr = LuaDLL.luaL_typename(L, -1);
                var valueTypeStr = LuaDLL.luaL_typename(L, -2);

                var keyType = LuaDLL.lua_type(L, -2);
                var valueType = LuaDLL.lua_type(L, -1);
                LuaNodeItem childContents = luaNode.content;
                if (keyType == LuaTypes.LUA_TNUMBER)
                {
                    childContents.key = LuaDLL.lua_tonumber(L, -2).ToString();
                }
                else if (keyType == LuaTypes.LUA_TSTRING)
                {
                    childContents.key = LuaDLL.lua_tostring(L, -2);
                }

                if (valueType == LuaTypes.LUA_TTABLE)
                {
                    var childNode = ParseLuaTable(L, scanMap);
                    if (childNode != null)
                    {
                        childNode.content.key = childContents.key;
                        luaNode.childNodes.Add(childNode);
                    }
                }
                else
                {
                    childContents.keyType = keyTypeStr;
                    childContents.valueType = valueTypeStr;
                    luaNode.childContents.Add(childContents);
                    if (valueType == LuaTypes.LUA_TNUMBER)
                    {
                        childContents.value = LuaDLL.lua_tonumber(L, -1).ToString("f2");
                    }
                    else if (valueType == LuaTypes.LUA_TSTRING)
                    {
                        childContents.valueType = LuaDLL.lua_tostring(L, -1);
                    }
                }

                LuaDLL.lua_pop(L, 1);
            }

            Debug.Log("Ptr " + tableStr + "");
            return luaNode;
        }

        private Dictionary<string, LuaNode> scanMap = new Dictionary<string, LuaNode>();
        LuaVarTreeView mLuaVarTreeView ;
        void OnGUI()
        {
            LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
            if (mgr != null)
            {
                if (GUILayout.Button("Test"))
                {
                    var luaState = mgr.lua;
                    LuaDLL.lua_getglobal(luaState.L, "myTable");
                    scanMap.Clear();
                    var rootNode = ParseLuaTable(luaState.L, scanMap);
                    mLuaVarTreeView.luaNodeRoot = rootNode;
                    mLuaVarTreeView.Reload();
                }
            }
            else
            {
                GUILayout.Label("Can't find luaManager");
            }


            if (mLuaVarTreeView != null && mLuaVarTreeView.luaNodeRoot!=null)
            {
                mLuaVarTreeView.OnGUI(new Rect(0,100,position.width,position.height-100));
            }
            else
            {
                mLuaVarTreeView = new LuaVarTreeView(new LuaVarTreeViewState());
            }
        }
    }
}
