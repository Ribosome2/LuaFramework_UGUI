using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace LuaVarWatcher
{
    public class LuaVarNodeParser
    {

        private static string cleanDoubleToNumber(IntPtr luaState, int idx)
        {
            return  LuaDLL.lua_tonumber(luaState, idx).ToString("G29");
        }

        public static LuaNode ParseLuaTable(IntPtr L, Dictionary<string, LuaNode> scanMap)
        {
            if (!LuaDLL.lua_istable(L, -1))
            {
                return null;
            }

            var ptr = LuaDLL.lua_topointer(L, -1);
            var tableAddress = ptr.ToString();
            if (scanMap.ContainsKey(tableAddress))
            {
                return null;
            }

            LuaNode luaNode = new LuaNode();
            luaNode.content.value = tableAddress;
            scanMap[tableAddress] = luaNode;

            LuaDLL.lua_pushnil(L);
            var top = LuaDLL.lua_gettop(L);
            while (LuaDLL.lua_next(L, -2) > 0)
            {
                var keyTypeStr = LuaDLL.luaL_typename(L, -1);
                var valueTypeStr = LuaDLL.luaL_typename(L, -2);

                var keyType = LuaDLL.lua_type(L, -2);
                var valueType = LuaDLL.lua_type(L, -1);
                LuaNodeItem childContents = new LuaNodeItem();
                if (keyType == LuaTypes.LUA_TNUMBER)
                {
                    childContents.key = cleanDoubleToNumber(L, -2);
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
                        childNode.content.isTable = true;
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
                        childContents.value = cleanDoubleToNumber(L, -1).ToString();
                    }
                    else if (valueType == LuaTypes.LUA_TSTRING)
                    {
                        childContents.value = LuaDLL.lua_tostring(L, -1);
                    }
                }

                LuaDLL.lua_pop(L, 1);
            }

            return luaNode;
        }
    }
}