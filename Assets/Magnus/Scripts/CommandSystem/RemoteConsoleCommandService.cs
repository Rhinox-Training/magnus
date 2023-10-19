using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Rhinox.Utilities;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class SampleServer
    {
        private List<TcpClient> _connectedClients = new List<TcpClient>(new TcpClient[0]);
        private TcpListener _tcpServer;

        /// <summary>
        /// Background thread for TcpServer workload.
        /// </summary>  
        private Thread _tcpListenerThread;

        /// <summary>  
        /// Create handle to connected tcp client.  
        /// </summary>  
        private TcpClient _connectedTcpClient;

        private void ListenForIncommingRequests()
        {
            _tcpServer = new TcpListener(IPAddress.Any, 53659);
            _tcpServer.Start();
            ThreadPool.QueueUserWorkItem(this.ListenerWorker, null);
        }

        private void ListenerWorker(object token)
        {
            while (_tcpServer != null)
            {
                Debug.Log("Its here");
                _connectedTcpClient = _tcpServer.AcceptTcpClient();
                _connectedClients.Add(_connectedTcpClient);
                // Thread thread = new Thread(HandleClientWorker);
                // thread.Start(connectedTcpClient);
                ThreadPool.QueueUserWorkItem(this.HandleClientWorker, _connectedTcpClient);
            }
        }

        private string GetLocalIP()
        {
            string localIp = "127.0.0.1";
            //Get the local IP
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                    break;
                } //if
            } //foreach

            return localIp;
        }

        private void HandleClientWorker(object token)
        {
            try
            {
                Byte[] bytes = new Byte[1024];
                using (var client = token as TcpClient)
                using (var stream = client.GetStream())
                {
                    Debug.Log("Nouveau Client connecté");
                    SendMessage(client, $@"Magnus@{GetLocalIP()}>");
                    int length;
                    // Read incoming stream into byte array.    
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.                          

                        if (bytes.Any(x => x > (byte) 239))
                            continue;

                        string clientMessage = Encoding.ASCII.GetString(incomingData);

                        clientMessage = clientMessage.Trim().Trim(' ', '\n', '\r', '\t');
                        
                        if (clientMessage.Length == 0)
                            continue;
                        
                        ParseNormal(client, clientMessage);
                    }

                    if (_connectedTcpClient == null)
                    {
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void ParseNormal(TcpClient client, string clientMessage)
        {
            //SendMessage(client, clientMessage);
            ManagedCoroutine.Dispatch(() => ConsoleCommandManager.Instance.ExecuteCommand(clientMessage));
            SendMessage(client, $@"Magnus@{GetLocalIP()}>");
        }
        
        // private void ParseAbnormalWindows()
        // {
        //     if (clientMessage == "\r\n")
        //     {
        //         isNewLine = true;
        //     }
        //     else
        //         currentCommandBuilder.Append(clientMessage);
        //
        //     SendMessage(client, clientMessage);
        //     if (isNewLine)
        //     {
        //         string command = currentCommandBuilder.ToString();
        //         currentCommandBuilder.Clear();
        //         ConsoleCommandManager.Instance.ExecuteCommand(command);
        //         SendMessage(client, $@"Magnus@{GetLocalIP()}>");
        //         isNewLine = false;
        //     }
        // }

        private void SendMessage(object token, string msg)
        {
            if (_connectedTcpClient == null)
            {
                Debug.Log("Problem connectedTCPClient null");
                return;
            }

            var client = token as TcpClient;
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    if (stream.CanWrite)
                    {
                        // Get a stream object for writing.    
                        // Convert string message to byte array.              
                        byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
                        // Write byte array to socketConnection stream.            
                        stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                        Debug.Log($"Server sent his message {msg} - should be received by client");
                    }
                }
                catch (SocketException socketException)
                {
                    Debug.Log("Socket exception: " + socketException);
                    return;
                }
            }
        }

        public void RunServer()
        {
            _tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
            _tcpListenerThread.IsBackground = true;
            _tcpListenerThread.Start();
        }

        // public void openCamera()
        // {
        //     if (_connectedClients != null)
        //     {
        //         print("Length " + _connectedClients.Count);
        //     }
        //     //SendMessage( "open_camera");
        //     foreach (TcpClient item in _connectedClients)
        //     {
        //         SendMessage(item, "open_camera");
        //     }
        // }
    }

    [ServiceLoader(disabledByDefault: true)]
    public class RemoteConsoleCommandService : AutoService<RemoteConsoleCommandService>
    {
        private SampleServer _tcpServer;

        protected override void Start()
        {
            base.Start();
            _tcpServer = new SampleServer();
            _tcpServer.RunServer();
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}