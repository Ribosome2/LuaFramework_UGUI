using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LuaVarWatcher
{
    [System.Serializable]
    public class DropDownItem
    {
        public string Content;
        public string Desc;
    }

    [System.Serializable]
    public class  DropDownConfigs{
        public List<DropDownItem> list=new List<DropDownItem>();
    }

    public class CommonDropDownList
    {
        private DropDownConfigs mConfigs=new DropDownConfigs();
        public CommonDropDownList(string configPath)
        {
            if (File.Exists(configPath))
            {
                mConfigs= JsonUtility.FromJson<DropDownConfigs>(File.ReadAllText(configPath));
            }
        }

        public void ShowDropDown( GenericMenu.MenuFunction2 func)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var config in mConfigs.list)
            {
                menu.AddItem(new GUIContent(config.Content), false, func, config.Content);
            }
            menu.ShowAsContext();
        }
    }
}