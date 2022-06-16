using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace LuaFramework
{
    public class TestIP:MonoBehaviour
    {
        public Text txt;

        void Start()
        {
            txt.text = GetLocalIPv4();
        }
        public string GetLocalIPv4()
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            return hostEntry.AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

        }

    }
}