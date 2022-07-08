using System.Text;
using LuaInterface;
using UnityEngine;
using UnityEngine.Analytics;

namespace LuaVarWatcher
{
    public class CFunctionTrapUtility
    {
        static public void SetTrap(string functionPath)
        {
            var lastDot = functionPath.LastIndexOf('.');
            if (lastDot > 0)
            {
                var metaTable = functionPath.Substring(0, lastDot);
                var metaKey = functionPath.Substring(lastDot+1, functionPath.Length - lastDot-1);
                StringBuilder sb  = new StringBuilder();
                sb.AppendLine("if globalFunctionTrapMap==nil then globalFunctionTrapMap={} end)");
                sb.AppendLine(string.Format("globalFunctionTrapMap['{0}'] =getmetatable({1})['{2}']", functionPath,metaTable,metaKey));
                sb.AppendLine(string.Format("getmetatable({0})['{1}']']=function(...)", metaTable, metaKey));
                sb.AppendLine(string.Format("print('lua call--- ' ,'{0}',debug.traceback())", functionPath));
                sb.AppendLine(string.Format("return globalFunctionTrapMap['{0}'](...)",functionPath));
                sb.AppendLine(string.Format("end"));

                string cmdStr = sb.ToString();
                ExecuteTrapCmd(cmdStr);
                Debug.Log(cmdStr);
            }
            else
            {
                Debug.LogError("目标函数路径应该是xxx.xxx的格式，至少有一个'.'");
            }
        }

        private static void ExecuteTrapCmd(string cmdStr)
        {
            var L = LuaHandleInterface.GetLuaPtr();
            var oldTop = LuaDLL.lua_gettop(L);
            if (LuaDLL.luaL_dostring(L, "require('Logic.FunctionTrap.TrapTest')"))
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