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
              

                var childContents = ParseKey(L);
                ParseValue(L, scanMap, childContents, luaNode, keyTypeStr);

                LuaDLL.lua_pop(L, 1);
            }

            return luaNode;
        }

        private static void ParseValue(IntPtr L, Dictionary<string, LuaNode> scanMap, LuaNodeItem childContents, LuaNode luaNode,
            string keyTypeStr)
        {
            var valueTypeStr = LuaDLL.luaL_typename(L, -2);
            var valueType = LuaDLL.lua_type(L, -1);
            childContents.luaValueType = valueType;
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
                    childContents.value = cleanDoubleToNumber(L, -1).ToString();
                }
                else if (valueType == LuaTypes.LUA_TSTRING)
                {
                    childContents.value = LuaDLL.lua_tostring(L, -1);
                }

                else if (valueType == LuaTypes.LUA_TSTRING)
                {
                    childContents.value = LuaDLL.lua_tostring(L, -1);
                }else if (valueType == LuaTypes.LUA_TFUNCTION)
                {
                    childContents.value = LuaDLL.lua_tocfunction(L, -1).ToString();
                }
                else if (valueType == LuaTypes.LUA_TUSERDATA)
                {
                    childContents.value = LuaDLL.lua_touserdata(L, -1).ToString();
                }
                else if (valueType == LuaTypes.LUA_TBOOLEAN)
                {
                    childContents.value = LuaDLL.lua_toboolean(L, -1).ToString();
                }
            }
        }

        private static LuaNodeItem ParseKey(IntPtr L)
        {
            var keyType = LuaDLL.lua_type(L, -2);

            LuaNodeItem childContents = new LuaNodeItem();
            if (keyType == LuaTypes.LUA_TNUMBER)
            {
                childContents.key = cleanDoubleToNumber(L, -2);
            }
            else if (keyType == LuaTypes.LUA_TSTRING)
            {
                childContents.key = LuaDLL.lua_tostring(L, -2);
            }

            return childContents;
        }
    }
}