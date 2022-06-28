using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace LuaVarWatcher
{
    public class TCPCodeServer
    {

		private TcpListener tcpListener;
		/// <summary> 
		/// Background thread for TcpServer workload. 	
		/// </summary> 	
		private Thread tcpListenerThread;
		/// <summary> 	
		/// Create handle to connected tcp client. 	
		/// </summary> 	
		private TcpClient connectedTcpClient;

        private bool mServerStarted = false;
        public  string  IP = "127.0.0.1";
        public int Port = 8052;


        public bool IsServerStarted
        {
            get { return mServerStarted; }
        }

        public delegate void ClientMsgCallBack(string mas);

        private ClientMsgCallBack mClientMsgCallBack;

        public void SetClientMsgCallBack(ClientMsgCallBack callBack)
        {
            mClientMsgCallBack = callBack;
        }

		public void Start()
		{
			// Start TcpServer background thread 		
			tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
			tcpListenerThread.IsBackground = true;
			tcpListenerThread.Start();
            mServerStarted = true;
        }

        public void ShutDown()
        {
            if (mServerStarted && tcpListener!=null)
            {
                mServerStarted = false;
				tcpListener.Server.Close();
                tcpListenerThread.Abort();
				Debug.Log("ShutDown Code Server");
            }
        }


		/// <summary> 	
		/// Runs in background TcpServerThread;
		/// </summary> 	
		private void ListenForIncomingRequests()
		{
			try
			{
				// Create listener on localhost port 8052. 			
				tcpListener = new TcpListener(IPAddress.Parse(IP), Port);
				tcpListener.Start();
				Debug.Log("Server is listening");
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					using (connectedTcpClient = tcpListener.AcceptTcpClient())
					{
						// Get a stream object for reading 					
						using (NetworkStream stream = connectedTcpClient.GetStream())
						{
							int length;
							while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
							{
								var incommingData = new byte[length];
								Array.Copy(bytes, 0, incommingData, 0, length);
								// Convert byte array to string message. 							
								string clientMessage = Encoding.ASCII.GetString(incommingData);
                                Debug.Log("client message received as: " + clientMessage);

								if (mClientMsgCallBack != null)
                                {
									mClientMsgCallBack.Invoke(clientMessage);
                                }
							}
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("SocketException " + socketException.ToString());
			}
		}
		/// <summary> 	
		/// Send message to client using socket connection. 	
		/// </summary> 	
		public void SendMessage(string msg)
		{
			if (connectedTcpClient == null)
			{
				Debug.LogError(" no client connected !!!");
				return;
			}

			try
			{
				// Get a stream object for writing. 			
				NetworkStream stream = connectedTcpClient.GetStream();
				if (stream.CanWrite)
				{
					byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
					// Write byte array to socketConnection stream.               
					stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
					Debug.Log("Server sent his message - should be received by client. SendBytes:"+serverMessageAsByteArray.Length);
                }
                else
                {
					Debug.Log("Can't write now ");
                }
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}
		}
	}
}