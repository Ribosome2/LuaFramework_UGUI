using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace KyleDarkMagic
{
    public class TCPSocket
    {
        public TcpListener server ;
        public TcpClient acceptedClient ;
        public TcpClient localClient ;
        private int SessionCount = 0;
        public void StartServer(string serverIp,int port)
        {
            server = new TcpListener(IPAddress.Parse(serverIp), port);
            // we set our IP address as server's address, and we also set the port: 9999

            server.Start();  // this will start the server
            server.BeginAcceptTcpClient(NewConnetionCallback, server);
            Debug.Log("Server Started "+serverIp+ " "+ port);
              
        }
        
        public void StartAsClient(String server, int port ,String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                localClient= new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = localClient.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                Debug.Log(string.Format("Sent: {0}", message));

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Debug.Log(string.Format("Received: {0}", responseData));

                // Close everything.
                stream.Close();
            }
           

            catch (ArgumentNullException e)
            {
                Debug.Log(string.Format("ArgumentNullException: {0}", e));
            }
            catch (SocketException e)
            {
                Debug.Log(string.Format("SocketException: {0}", e));
            }

        }


        void NewConnetionCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

            // End the operation and display the received data on 
            // the console.
           acceptedClient = listener.EndAcceptTcpClient(ar);
            Debug.Log("Client connected completed");
            NetworkStream ns = acceptedClient.GetStream(); //networkstream is used to send/receive messages

            byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)
            hello = Encoding.Default.GetBytes("hello world");  //conversion string => byte array

            ns.Write(hello, 0, hello.Length);     //sending the message

        }


    }
}