using System.Collections.Generic;

namespace LuaVarWatcher
{
    public class LuaNodeItem
    {
        public string keyType;
        public string key;
        public string valueType;
        public string value;
        public bool isTable = false;

        public string GetDisplayName()
        {
            if (isTable)
            {
                return string.Format("{0}:table: &{1}", key, value);
            }
            else
            {
                return string.Format("[{0}]:{1}", key, value);
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