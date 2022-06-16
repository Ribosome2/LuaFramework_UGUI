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
                    var L = mgr.lua.L;
                    var oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_getglobal(L, "myTable");
                    scanMap.Clear();
                    var rootNode =LuaVarNodeParser.ParseLuaTable(luaState.L, scanMap);
                    LuaDLL.lua_settop(L,oldTop);
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
