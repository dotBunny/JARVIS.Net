﻿using System.Collections.Generic;
using JARVIS.Shared.Services.Socket;

namespace JARVIS.Shard.Services.Socket.Commands
{
    public class LoginFail : ISocketCommand
    {
        public bool CanExecute(Sender session)
        {
            return true;
        }
        public void Execute(Sender session, Dictionary<string, string> parameters)
        {
            Shared.Log.Message("AUTH","Login Failed");
            Program.Quit(1);
        }
    }
}