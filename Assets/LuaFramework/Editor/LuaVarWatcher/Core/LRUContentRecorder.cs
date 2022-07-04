using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LuaVarWatcher
{
    [System.Serializable]
    public class LRUContentConfig
    {
        public List<string> ContentList = new List<string>();
    }

    public class LRUContentRecorder
    {
        public int MaxRecord = 30;
        private string mConfigPath;
        LRUContentConfig  mConfig = new LRUContentConfig();
        public LRUContentRecorder(string configPath)
        {
            mConfigPath = configPath;
            if (File.Exists(configPath))
            {
                mConfig = JsonUtility.FromJson<LRUContentConfig>(File.ReadAllText(configPath));
            }
        }
        public string GetLastUseContent()
        {
            if (mConfig != null && mConfig.ContentList.Count > 0)
            {
                return mConfig.ContentList[0];
            }
            return "";
        }


       public List<string> GetContentList()
        {
            return mConfig.ContentList;
        }




        public void AddUseRecord(string content)
        {

            content = content.Trim();

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            if (mConfig.ContentList.Contains(content))
            {
                mConfig.ContentList.Remove(content);
            }
            mConfig.ContentList.Insert(0,content);
            var dirPath = Path.GetDirectoryName(mConfigPath);
            if (Directory.Exists(dirPath) == false)
            {
                if (dirPath != null) Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(mConfigPath,JsonUtility.ToJson(mConfig));
        }

        public void ShowCodeExecuteDropDown(GenericMenu.MenuFunction2 func,EditorWindow window=null)
        {
            if (mConfig.ContentList==null || mConfig.ContentList.Count == 0)
            {
                if (window!=null)
                {
                    window.ShowNotification(new GUIContent("暂无记录"));
                }
                else
                {
                    Debug.Log("暂无最近记录");
                }
                return;
            }
            GenericMenu menu = new GenericMenu();
            foreach (var content in GetContentList())
            {
                menu.AddItem(new GUIContent(content), false, func, content.Trim());
            }
            menu.ShowAsContext();
        }
    }
}