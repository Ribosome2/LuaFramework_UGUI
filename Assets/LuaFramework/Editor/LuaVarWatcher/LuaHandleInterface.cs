﻿using System;
using System.IO;
using LuaFramework;
using LuaInterface;
using UnityEditor;
using UnityEngine;

namespace LuaVarWatcher
{
    public class LuaHandleInterface
    {
        public static LuaState GetLuaStateInstance()
        {
            LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
            if (mgr != null)
            {
                return mgr.lua;
            }
            return null;
        }

        public static IntPtr GetLuaPtr()
        {
            var lusState = GetLuaStateInstance();
            if (lusState != null)
            {
                return lusState.LuaPtrForEditorOnly;
            }
            return IntPtr.Zero;
        }


        public static void ReloadLuaFile(string luaPath)
        {
            //replace with implementation in your project
        }

        public static void ConnectDebugger()
        {

        }

        public static void ExecuteStringCmd(string cmdStr)
        {
            var L = LuaHandleInterface.GetLuaPtr();
            var oldTop = LuaDLL.lua_gettop(L);
            if (LuaDLL.luaL_dostring(L, cmdStr))
            {

            }
            else
            {
                Debug.LogError("执行错误: " + LuaDLL.lua_tostring(L, -1));
                LuaDLL.lua_settop(L, oldTop);
            }
        }
    }
}