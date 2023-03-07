using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LuaVarWatcher
{
    [System.Serializable]
    public class QuickActionsConfig
    {
        public QuickActionsConfig(string configPath)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    var config = JsonUtility.FromJson<QuickActionsConfig>(File.ReadAllText(configPath));
                    ActionList = config.ActionList;
                    foreach (var item in ActionList)
                    {
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public List<QuickActionsItem> ActionList = new List<QuickActionsItem>();
    }

    [System.Serializable]
    public class QuickActionsItem
    {
        public string Name;
        public string TargetCode;
        public int Height = 0;
    }
}