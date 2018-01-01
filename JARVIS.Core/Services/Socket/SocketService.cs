﻿using System.Collections.Generic;
using JARVIS.Shared.Services.Socket;
using JARVIS.Shared.Protocol;
using Grapevine.Interfaces.Server;

namespace JARVIS.Core.Services.Socket
{
    public class SocketService : IService
    {
        public Dictionary<Sender, SocketUser> AuthenticatedUsers = new Dictionary<Sender, SocketUser>();
        Dictionary<Sender, List<byte>> Buffers = new Dictionary<Sender, List<byte>>();

        public int BufferCount 
        {
            get 
            {
                return Buffers.Count;
            }
        }

        // TODO: Add ability to sub to events that get rebroadcasted
        // TODO: Add REAUTH/AUTH
        SocketServer Server;
        JCP Protocol;

        public SocketService(string Host, int SocketPort, bool socketEncryption, string socketKey)
        {
            Server = new SocketServer();

            // Setup Parser
            Protocol = new JCP(socketEncryption, socketKey);

            if (socketEncryption)
            {
                Shared.Log.Message("socket", "Encryption Enabled");
            }
            else
            {
                Shared.Log.Message("socket", "Encryption DISABLED");
            }


            // Setup handlers
            Server.OnConnected += Server_OnConnected;
            Server.OnClosed += Server_OnClosed;
            Server.OnException += Server_OnException;
            Server.OnData += Server_OnData;

            // Set port
            Server.Host = Host;
            Server.Port = SocketPort;
        }

        ~SocketService()
        {
            Server = null;
        }

        public void Tick()
        {
            
        }

        void Server_OnClosed(Sender session)
        {
            Shared.Log.Message("socket", "Closing connection from " + session.RemoteEndPoint);

            // Remove buffer for session
            Buffers.Remove(session);

            // Remove if authenticated from the authenticated list
            if ( AuthenticatedUsers.ContainsKey(session) )
            {
                AuthenticatedUsers.Remove(session);
            }
        }

        void Server_OnConnected(Sender session)
        {
            Shared.Log.Message("socket", "New connection from " + session.RemoteEndPoint);
            Buffers.Add(session, new List<byte>());

            SendToSession(session, Instruction.OpCode.INFO, new Dictionary<string, string> { { "message", "Welcome to JARVIS." } });
            SendToSession(session, Instruction.OpCode.AUTH);
        }

        void Server_OnException(Sender session, System.Exception e)
        {
            Shared.Log.Message("socket", "Exception from " + session.RemoteEndPoint + " of " + e.Message);
        }


        void Server_OnData(Sender session, byte[] data)
        {
            // Standardized data processor
            DataHandler.ProcessData(session, Buffers[session], data, Protocol, new CommandFactory());
        }

        public string GetName() 
        {
            return "Socket";   
        }

        public void Start()
        {
            Server.Listen();
            Server.Start();
            Shared.Log.Message("socket", "Listening on " + Server.Port.ToString());
        }

        public void Stop()
        {
            Server.Stop();
        }

        public void SendToAllSessions(Instruction.OpCode type, Dictionary<string, string> arguments, bool authRequired = true)
        {
            // Send to sessions
            foreach(Sender session in Server.Clients)
            {
                if ( authRequired && AuthenticatedUsers.ContainsKey(session)) {
                    SendToSession(session, type, arguments);
                } else {
                    SendToSession(session, type, arguments);                    
                }
            }
        }

        public void SendToSession(Sender session, Instruction.OpCode type)
        {
            SendToSession(session, type, new Dictionary<string, string> { });
        }

        public void SendToSession(Sender session, Instruction.OpCode type, Dictionary<string, string> arguments)
        {
            Shared.Log.Message("socket", "Sending " + type.ToString() + " to " + session.RemoteEndPoint);
            session.Send(Protocol.GetBytes(new Packet(type, arguments)));
        }

        public void HandleCallbackAsync(IHttpRequest request)
        {
           
        }
    }
}