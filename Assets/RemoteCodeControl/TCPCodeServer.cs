using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using KyleDarkMagic;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class TCPCodeServer : MonoBehaviour
    {
        TCPSocket tcpSocket = new TCPSocket();
        public Text textInfo;
        public InputField inputServerIp;
        public InputField inputPort;


        void SelectStartIP()
        {
            var hostName = Dns.GetHostName();
            var regex = new Regex("[\\d]+\\.[\\d]+\\.[\\d]+\\.[\\d]+");
            foreach (var ipAddress in Dns.GetHostEntry(hostName).AddressList)
            {
                var ipStr = ipAddress.ToString();
                if (regex.IsMatch(ipStr))
                {
                    inputServerIp.text = ipStr;
                }
            }
        }


        void Start()
        {
            SelectStartIP();
            var hostName = Dns.GetHostName();
            textInfo.text += "HostName" + hostName;
            foreach (var ipAddress in Dns.GetHostEntry(hostName).AddressList)
            {
                textInfo.text += "\nIP " + ipAddress;
            }
        }

        public void StartServer()
        {
            tcpSocket.StartServer(inputServerIp.text,int.Parse(inputPort.text));
        }


        public void StartAsClient()
        {
            tcpSocket.StartAsClient(inputServerIp.text, int.Parse(inputPort.text),"DDDDD");
        }
    }

}
