using System;
using System.IO;
using System.Reflection;
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
        private static IntPtr s_LuaPointer = IntPtr.Zero;
        public static IntPtr GetLuaPtr()
        {
            var lusState = GetLuaStateInstance();
            if (lusState != null && s_LuaPointer == IntPtr.Zero)
            {
                Type myClassType = typeof(LuaState);
                FieldInfo myProtectedVarInfo =
                    myClassType.GetField("L", BindingFlags.NonPublic | BindingFlags.Instance);
                object myProtectedVarValue = myProtectedVarInfo.GetValue(lusState);
                s_LuaPointer = (IntPtr)myProtectedVarValue;
            }

            return s_LuaPointer;
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