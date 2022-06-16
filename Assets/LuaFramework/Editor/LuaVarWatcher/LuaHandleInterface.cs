using System;
using LuaFramework;
using LuaInterface;
using UnityEditor;
using UnityEngine;

namespace LuaVarWatcher
{
    public class LuaHandleInterface
    {
        public static IntPtr GetLuaPtr()
        {
            LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
            if (mgr != null)
            {
                return mgr.lua.LuaPtrForEditorOnly;
            }
            return IntPtr.Zero;
        }
    }
}