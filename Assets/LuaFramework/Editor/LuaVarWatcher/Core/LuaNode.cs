using System.Collections.Generic;
using LuaInterface;

namespace LuaVarWatcher
{
    public class LuaNodeItem
    {
        public string keyType;
        public string key;
        public string valueType;
        public string value;
        public LuaTypes luaValueType;
        public string GetDisplayName()
        {
            if (luaValueType==LuaTypes.LUA_TTABLE)
            {
                return string.Format("{0}:table: &{1}", key, value);
            }else if (luaValueType == LuaTypes.LUA_TFUNCTION)
            {
                return string.Format("{0}:function: &{1}", key, value);
            }
            else
            {
                return string.Format("type:{0} [{1}]:{2}",luaValueType, key, value);
            }
        }
    };

    public class LuaNode
    {
        public LuaNodeItem content = new LuaNodeItem();
        public int ID;
        public List<LuaNodeItem> childContents = new List<LuaNodeItem>();
        public List<LuaNode> childNodes = new List<LuaNode>();
    };

}