﻿using System;
using System.Net;
using System.Net.Sockets;

namespace JARVIS.Shared.Services.Socket
{
    /// <summary>
    /// Socket client
    /// </summary>
    public partial class SocketClient : SocketBase
    {
         private Listener listener = null;

        /// <summary>
        /// Gets the sender.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public Sender Sender => listener?.Sender;

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>
        /// The remote end point.
        /// </value>
        public EndPoint RemoteEndPoint => socket.RemoteEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        public SocketClient()
        {
        }
    

        /// <summary>
        /// Connects the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public void Connect(string host, int port)
        {
            try
            {
                if (Connected)
                {
                    return;
                }

                SetHost(host);

                SetPort(port);

                // Make our socket
                socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect that shit up
                socket.Connect(Host, Port);

                listener?.Stop();

                listener = new Listener(this, socket, Listener.ListenerType.Client);

                SetOnConnectedFun();
            }
            catch (Exception ex)
            {
                Log.Error("Socket", ex.Message);
            }
        }

        
        /// <summary>
        /// Start Socket client.
        /// </summary>
        public override void Start()
        {
            try
            {
                if (listener?.Running==true)
                {
                    return;
                }

                CheckHostAndPort();

                if (!Connected)
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }

                listener.Start();
            }
            catch (Exception ex)
            {
                Log.Error("Socket", ex.Message);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public override void Stop()
        {
            if (listener != null && listener.Running)
            {
                listener.Stop();
            }
        }
        
        /// <summary>
        /// Sets the on connected fun.
        /// </summary>
        private void SetOnConnectedFun()
        {
            if (Connected)
            {
                OnConnected?.Invoke(listener.Sender);
            }
        }
    }
}