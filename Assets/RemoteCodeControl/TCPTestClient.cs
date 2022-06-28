using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LuaFramework;
using LuaInterface;
using UnityEngine;

namespace RemoteCodeControl
{
	public class TCPTestClient : MonoBehaviour
	{

		public static TCPTestClient Instance 
        {
            get
            {
                if (mInstance == null)
                {
					GameObject clientGo = new GameObject("TestClient");
                    mInstance = clientGo.AddComponent<TCPTestClient>();
                }

                return mInstance;
            }
        }

        private static TCPTestClient mInstance;

		#region private members 	
		private TcpClient socketConnection;
		private Thread clientReceiveThread;
		#endregion

		private int port = 8052;
		public string IP = "127.0.0.1";
		Queue<string> mMessageQueue = new Queue<string>();

		private string content = "";


		void Update()
		{
			while (mMessageQueue.Count > 0)
			{
				var msg = mMessageQueue.Dequeue();

				ExecuteLuaCode(msg);
			}
		}

		private static void ExecuteLuaCode(string msg)
		{
			var L = LuaHandleInterface.GetLuaPtr();
			if (L != IntPtr.Zero)
			{
				var oldTop = LuaDLL.lua_gettop(L);
				if (LuaDLL.luaL_dostring(L, msg))
				{
				}
				else
				{
					Debug.LogError("执行错误: " + LuaDLL.lua_tostring(L, -1));
					LuaDLL.lua_settop(L, oldTop);
				}
			}
		}

		void OnGUI()
		{
			IP = GUILayout.TextField(IP);
			if (GUILayout.Button("Connect To Sever"))
			{
				ConnectToTcpServer(IP,port);
			}

			if (GUILayout.Button("Send msg"))
			{
				SendMessageToServer("DDD");
			}
			GUILayout.Label(content);
		}

		public void ConnectToTcpServer(string ip,int port)
        {
            this.IP = ip;
            this.port = port;
			try
			{
				clientReceiveThread = new Thread(new ThreadStart(ListenForSererData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.Log("On client connect exception " + e);
			}
		}

		private void ListenForSererData()
		{
			try
			{
				socketConnection = new TcpClient(IP, port);
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					// Get a stream object for reading 				
					using (NetworkStream stream = socketConnection.GetStream())
					{
						int length;
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							string serverMessage = Encoding.ASCII.GetString(incommingData);
							content += serverMessage;
							mMessageQueue.Enqueue(serverMessage);
							Debug.Log("server message received as: " + serverMessage);
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}
		}
		/// <summary> 	
		/// Send message to server using socket connection. 	
		/// </summary> 	
		private void SendMessageToServer(string msg)
		{
			if (socketConnection == null)
			{
				Debug.LogError("Not connected to sever ,yet");
				return;
			}
			try
			{
				// Get a stream object for writing. 			
				NetworkStream stream = socketConnection.GetStream();
				if (stream.CanWrite)
				{
					byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
					Debug.Log("Client sent bytes "+ clientMessageAsByteArray.Length);
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}
		}
	}

}
