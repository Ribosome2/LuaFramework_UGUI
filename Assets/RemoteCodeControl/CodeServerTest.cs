using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

namespace KyleDarkMagic
{
    public class CodeServerTest : MonoBehaviour
    {
        public enum eCommandType
        {
            FilePath = 1,
            SelectContent = 2,
            ConnectDebugger = 3,
            CSharpFunc = 4,
            FileNameReload = 5,
        }

        public class MessageUnit
        {
            public int Id;
            public string Content;
        }

        public int port= 27000;
        public string MsgContent = "";
        private string hostName = "";

        public string serverIp = "10.18.3.97";
        static Queue<MessageUnit> messageQuque = new Queue<MessageUnit>();
        static private UDPSocket curSocket = null;
        static private UDPSocket clientSocket = null;
        public string ServerStartIp = "127.0.0.1";

        void OnGUI()
        {
            hostName = Dns.GetHostName();
            GUILayout.Label("HostName: "+ hostName);
            foreach (var ipAddress in Dns.GetHostEntry(hostName).AddressList)
            {
                GUILayout.Label("myIP: " + ipAddress);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Port:");
            int.TryParse(GUILayout.TextField(port.ToString()),out port);
            GUILayout.EndHorizontal();
            ServerStartIp = GUILayout.TextField(ServerStartIp);
            if (curSocket == null)
            {
                if (GUILayout.Button("StartServer",GUILayout.Height(50),GUILayout.Width(100)))
                {
                    StartKyleServer();
                }
            }
            else
            {
                GUILayout.Label("Server is on");
                GUILayout.Label(MsgContent);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("ServerIp");
            serverIp=GUILayout.TextField(serverIp);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("SendMsg", GUILayout.Height(50), GUILayout.Width(100)))
            {
                var tempSocket = new UDPSocket();
                Debug.Log("PassIp:"+ IPAddress.Parse(serverIp));
                tempSocket.Client(serverIp, port);
                clientSocket = tempSocket;
                clientSocket.Send(JsonUtility.ToJson(new MessageUnit { Content = "DDDDD", Id = 2 }));
            }

        }

        void Update()
        {
            if (messageQuque.Count > 0)
            {
                var data = messageQuque.Dequeue();
                var msg = "GotData" + data.Id + "  " + data.Content;
                MsgContent += msg;
                Debug.Log(msg);
            }
        }

        void StartKyleServer()
        {
            try
            {
                if (curSocket == null)
                {
                    var tempSocket = new UDPSocket();
                    tempSocket.Server("127.0.0.1", port, OnMsgCalllBack);
                    curSocket = tempSocket;
                    Debug.Log("Magic server start !");
                }
            }
            catch (Exception e)
            {
                Debug.Log("Ooops ,Magic server start fail ," + e);
            }
        }

        static void OnMsgCalllBack(byte[] bytes)
        {
            var allContent = UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var messageData = JsonUtility.FromJson<MessageUnit>(allContent);
            messageQuque.Enqueue(messageData);
        }

      
}
}
