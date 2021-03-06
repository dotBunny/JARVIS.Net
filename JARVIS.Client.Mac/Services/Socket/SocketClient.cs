﻿using System;
using System.Collections.Generic;
using AppKit;
using JARVIS.Shared.Protocol;
using JARVIS.Shared.Services.Socket;

namespace JARVIS.Client.Mac.Services.Socket
{
    public class SocketClient : ISocketClient
    {
        public JCP Protocol;

        public bool IsConnected
        {
            get { return Connection.Connected; }
        }

        Shared.Services.Socket.SocketClient Connection;
        AppDelegate Application;

        public SocketClient(AppDelegate application)
        {
            // Assign reference to application delegate
            Application = application;

            // Create Client
            Connection = new Shared.Services.Socket.SocketClient();

            // Setup event handlers
            Connection.OnClosed += Connection_OnClosed;
            Connection.OnConnected += Connection_OnConnected;
            Connection.OnException += Connection_OnException;
            Connection.OnData += Connection_OnData;
        }

        void Connection_OnClosed(Sender session)
        {
            Application.OnDisconnected();
            Shared.Log.Message("socket", "Disconnected from " + Settings.ServerAddress + ":" + Settings.ServerPort.ToString());
        }
        void Connection_OnConnected(Sender session)
        {
            Application.OnConnected();
            Shared.Log.Message("socket", "Connected to " + Settings.ServerAddress + ":" + Settings.ServerPort.ToString());
        }
        void Connection_OnException(Sender session, Exception e)
        {
            Shared.Log.Error("socket", "An error occured. " + e.Message);
            Shared.Log.Error("socket", e.StackTrace);
        }

        // We do not need to handle sessions on the client because we know the session that is sending it, it must be the server.
        List<byte> Buffer = new List<byte>();
        void Connection_OnData(Sender session, byte[] data)
        {
            DataHandler.ProcessData(session, Buffer, data, Protocol, new CommandFactory(this));
        }


        public void Start()
        {
            // Initialize Protocol
            Protocol = new JCP(Settings.EncryptionUseEncryptedProtocol, Settings.EncryptionServerEncryptionKey);
            if (Settings.EncryptionUseEncryptedProtocol)
            {
                Shared.Log.Message("socket", "Encryption Enabled");
            }
            else
            {
                Shared.Log.Message("socket", "Encryption DISABLED");
            }


            Application.OnConnecting();
            Connection.Connect(Settings.ServerAddress, Settings.ServerPort);
            Connection.Start();


        }

        public void Stop()
        {
            // Disconnect
            Application.OnDisconnecting();
            Connection.Stop();
        }


        public void Send(Instruction.OpCode type) => Send(type, new Dictionary<string, InstructionParameter>());
        public void Send(Instruction.OpCode type, Dictionary<string, string> parameters) => Send(type, Instruction.CreateParametersDictionary(parameters));
        public void Send(Instruction.OpCode type, Dictionary<string, InstructionParameter> parameters)
        {
            Packet p = new Packet(type, parameters);
            Shared.Log.Message("socket", "Sending " + p.GetOpCodes() + " to " + Settings.ServerAddress + ":" + Settings.ServerPort.ToString());
            Connection.Sender.Send(Protocol.GetBytes(p));
        }
        public void Send(Packet packet)
        {
            Shared.Log.Message("socket", "Sending " + packet.GetOpCodes() + " to " + Settings.ServerAddress + ":" + Settings.ServerPort.ToString());
            Connection.Sender.Send(Protocol.GetBytes(packet));
        }
        public void Send(Packet[] packets)
        {
            foreach (Packet p in packets)
            {
                Shared.Log.Message("socket", "Sending " + p.GetOpCodes() + " to " + Settings.ServerAddress + ":" + Settings.ServerPort.ToString());
            }
            Connection.Sender.Send(Protocol.GetBytes(packets));
        }

    }
}
