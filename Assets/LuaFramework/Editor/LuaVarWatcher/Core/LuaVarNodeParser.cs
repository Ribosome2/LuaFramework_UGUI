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
//            return  LuaDLL.lua_tonumber(luaState, idx).ToString("G29");//用G29会导致小数点不准确
            return  LuaDLL.lua_tonumber(luaState, idx).ToString();
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
            while (LuaDLL.lua_next(L, -2) > 0)
            {
                var childContents = ParseKey(L);
                var valueType = LuaDLL.lua_type(L, -1);
                if (valueType == LuaTypes.LUA_TTABLE)
                {
                    var childNode = ParseLuaTable(L, scanMap);
                    if (childNode != null)
                    {
                        childNode.content.key = childContents.key;
                        childNode.content.luaValueType = childContents.luaValueType;
                        luaNode.childNodes.Add(childNode);
                    }
                }
                else
                {
                    ParseNoneTableValue(L, scanMap, childContents, luaNode);
                }
                LuaDLL.lua_pop(L, 1);
            }

            return luaNode;
        }

        private static void ParseNoneTableValue(IntPtr L, Dictionary<string, LuaNode> scanMap, LuaNodeItem childContents, LuaNode luaNode)
        {
            var keyTypeStr = LuaDLL.luaL_typename(L, -1);
            var valueTypeStr = LuaDLL.luaL_typename(L, -2);
            var valueType = LuaDLL.lua_type(L, -1);
            childContents.luaValueType = valueType;
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
            }else if (keyType == LuaTypes.LUA_TTABLE)
            {
                childContents.key = LuaDLL.lua_topointer(L, -2).ToString();
            }

            return childContents;
        }
    }
}