﻿using System;
using System.Collections.Generic;
using Grapevine.Interfaces.Server;
using Grapevine.Shared;
using JARVIS.Core.Services.Web;
using JARVIS.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace JARVIS.Core.Protocols.OAuth2
{
    public class OAuth2Provider : ICallbackListener
    {
        string _code = string.Empty;
        public string Token { get; private set; }
        string _refreshToken = string.Empty;
        int _expiresInSeconds;
        string _state;
        DateTime _expiresOn = DateTime.Now;

        string _provider = string.Empty;
        string _clientID = string.Empty;
        string _clientSecret = string.Empty;
        string _clientEncoded = string.Empty;
        string _scope = string.Empty;
        string _authorizeEndpoint = string.Empty;
        string _tokenEndpoint = string.Empty;
        bool _callbackRequiresTrailingSlash = true;

        string _jarvisScope = string.Empty;

        public Action OnComplete;


        public bool Authenticated
        {
            get; private set;
        }

        public bool CheckToken()
        {
            if (_refreshToken != string.Empty && DateTime.Now >= _expiresOn)
            {
                GetRefreshToken();
            }

            return Authenticated;
        }

        public OAuth2Provider()
        {
           Reset();
        }
        public OAuth2Provider(string providerName, string clientID, string clientSecret, 
                              string scope, string authorizeEndpoint, string tokenEndpoint, 
                              string jarvisScope = "default", bool callbackRequiresTrailingSlash = true)
        {

            // TODO ADD thing to toggle if trailing /
            Reset();

            _provider = providerName;
            _clientID = clientID;
            _clientSecret = clientSecret;
            _scope = scope;
            _authorizeEndpoint = authorizeEndpoint;
            _tokenEndpoint = tokenEndpoint;
            _jarvisScope = jarvisScope;
            _callbackRequiresTrailingSlash = callbackRequiresTrailingSlash;

            _clientEncoded = (_clientID + ":" + _clientSecret).Base64Encode();
        }

       

        public void Reset()
        {
            Token = string.Empty;
            Authenticated = false;
        }

        public void Login()
        {
            Authenticated = false;

            // Clear out other data
            Token = string.Empty;
            _code = string.Empty;
            _refreshToken = string.Empty;
            _expiresInSeconds = 0;

            Log.Message(_provider, "Initiating authorization process ...");

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create random key that is used later to make sure this is the one that processes it
            _state = GenerateState();

            // These will be split and used in the function itself
            parameters.Add("endpoint",_authorizeEndpoint);
            parameters.Add("title", _provider + " Authentication");
            parameters.Add("message", "JARVIS needs to you to authenticate with your " + _provider + " account for it to be able to poll data.");

            // Data passed to each call (with the execption of the token)
            parameters.Add("client_id", _clientID);
            parameters.Add("client_secret", _clientSecret);

            // Ask for a lot of perms
            parameters.Add("scope", _scope);
            parameters.Add("state", _state);
            parameters.Add("response_type", "code");

            parameters.Add("redirect_uri", GenerateCallbackURI());

            // Add to listeners
            Server.Services.GetService<WebService>().CallbackListeners.Add(_state, this);
            Server.Services.GetService<Services.Socket.SocketService>().SendToAllSessions(Shared.Protocol.Instruction.OpCode.OAUTH_REQUEST, parameters, true, _jarvisScope);
        }

        string GenerateState()
        {
           return  _provider.ToLower() + "-" + Guid.NewGuid().ToString();
        }

        string GenerateCallbackURI()
        {
            if ( !_callbackRequiresTrailingSlash )
            {
                return "http://" + Server.Config.Host + ":" + Server.Config.WebPort + "/callback";
            }
            return "http://" + Server.Config.Host + ":" + Server.Config.WebPort + "/callback/";
        }

        public void Callback(IHttpRequest request)
        {
            string state = request.QueryString.GetValue("state", string.Empty);

            // Stage 1 
            if (_code == string.Empty)
            {
                _code = request.QueryString.GetValue("code", string.Empty);

                if (_code != string.Empty)
                {
                    GetToken();
                }
            }
        }

        void GetToken()
        {
            // Create our token request
            var tokenRequest = new Requests.TokenRequest
            {
                Code = _code,
                RedirectURI = GenerateCallbackURI(),
                State = _state
            };

            // Add our authorization header
            tokenRequest.Headers.Add("Authorization", "Basic " + _clientEncoded);

            // Get Response
            var responseObject = tokenRequest.GetResponse(_tokenEndpoint);

            if (responseObject != null)
            {

                if (!string.IsNullOrEmpty(responseObject.ErrorCode) && responseObject.ErrorCode != "")
                {
                    Log.Error(_provider, "An error occured (" + responseObject.ErrorCode + ") while getting the token. " + responseObject.ErrorDescription);
                    Authenticated = false;
                }
                else
                {
                    Token = responseObject.AccessToken;
                    _refreshToken = responseObject.RefreshToken;
                    _expiresInSeconds = responseObject.ExpiresInSeconds;
                    _scope = responseObject.Scope;
                    _expiresOn = DateTime.Now.AddSeconds(_expiresInSeconds);

                    // Flag we are good!
                    Log.Message(_provider, "Authentication Successful. (" + Token + ")");
                    OnComplete?.Invoke();
                    Authenticated = true;
                }
            }
            else
            {
                Authenticated = false;
                Log.Error(_provider, _provider + " failed to get the token. NULL Response Object.");
            }

        }

        void GetRefreshToken()
        {
            if (!Authenticated) return;

            Log.Message(_provider, "Refreshing Token");

            var tokenRequest = new Requests.RefreshTokenRequest(_refreshToken, _state);


            // Add our authorization header
            tokenRequest.Headers.Add("Authorization", "Basic " + _clientEncoded);

            // Get Response
            var responseObject = tokenRequest.GetResponse(_tokenEndpoint);

            if (responseObject != null)
            {

                if (!string.IsNullOrEmpty(responseObject.ErrorCode) && responseObject.ErrorCode != "")
                {
                    Log.Error(_provider, "An error occured (" + responseObject.ErrorCode + ") while refreshing the token. " + responseObject.ErrorDescription);
                    Authenticated = false;
                }
                else
                {
                    Token = responseObject.AccessToken;
                    _expiresInSeconds = responseObject.ExpiresInSeconds;
                    _scope = responseObject.Scope;
                    _expiresOn = DateTime.Now.AddSeconds(_expiresInSeconds);

                    // Flag we are good!
                    Authenticated = true;
                }
            }
            else
            {
                Authenticated = false;
                Log.Error(_provider, _provider + " failed to refresh the token. NULL Response Object.");
            }

        }

    }
}
