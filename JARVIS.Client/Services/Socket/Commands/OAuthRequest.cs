﻿using System;
using System.Collections.Generic;
using JARVIS.Shared.Services.Socket;
using JARVIS.Shared;
using JARVIS.Shared.Protocol;

namespace JARVIS.Client.Services.Socket.Commands
{
    /// <summary>
    /// OAuth Request Command
    /// </summary>
    public class OAuthRequest : ISocketCommand
    {
        /// <summary>
        /// Can this command be executed?
        /// </summary>
        /// <returns><c>true</c>, if settings look good, <c>false</c> otherwise.</returns>
        public bool CanExecute(Sender session)
        {
            return Settings.FeatureFileOutputs;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="session">The user session.</param>
        /// <param name="parameters">The parameters (endpoint, title, message, state) to use while executing the command.</param>
        public void Execute(Sender session, Dictionary<string, InstructionParameter> parameters)
        {
            Uri endpoint = new Uri(parameters["endpoint"].ToString());
            parameters.Remove("endpoint");

            string title = "OAuth Request";
            if (parameters.ContainsKey("title"))
            {;
                title = parameters["title"].ToString();
                parameters.Remove("title");
            }

            string message = "A service requires you to login for proper use.";
            if (parameters.ContainsKey("message"))
            {
                message = parameters["message"].ToString();
                parameters.Remove("message");
            }

            // Add left over parameters
            foreach(KeyValuePair<string,InstructionParameter> s in parameters)
            {
                if (s.Value.IsString())
                {
                    endpoint = endpoint.AddQuery(s.Key, s.Value.ToString());
                }
            }

            OAuthNotification notification = new OAuthNotification(endpoint.ToString(), title, message);
            if (parameters.ContainsKey("state"))
            {
                notification.State = parameters["state"].ToString();
            }
            Log.Notifier.Notify(notification);
        }
    }
}