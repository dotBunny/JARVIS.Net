﻿using System;
using System.Collections.Generic;

namespace JARVIS.Shared.Protocol
{
    /// <summary>
    /// Custom 'Secure' JARVIS Protocol
    /// </summary>
    public class JCP
    {
        public const int SizeOfLength = sizeof(int);

        /// <summary>
        /// An indicator byte that data is not encrypted.
        /// </summary>
        public const byte DecryptedMarker = 0x0e;

        /// <summary>
        /// An indicator byte that data is encrypted.
        /// </summary>
        public const byte EncryptedMarker = 0x0f;

        /// <summary>
        /// The indicator byte of the operation code.
        /// </summary>
        public const byte OpCodeTerminator = 0x01;

        /// <summary>
        /// The indicator byte of a parameter name.
        /// </summary>
        public const byte ParameterNameTerminator = 0x02;

        /// <summary>
        /// The indicator byte of a parameter value.
        /// </summary>
        public const byte ParameterValueTerminator = 0x03;

        /// <summary>
        /// Protocol Version
        /// </summary>
        public const int Version = 13;

        /// <summary>
        /// The Encryption Key
        /// </summary>
        public string EncryptionKey
        {
            get
            {
                if (IsAuthenticated)
                {
                    return currentEncryptionKey;
                }
                return initialEncryptionKey;
            }
        }

        /// <summary>
        /// Has the JCP protocol session been authenticated? 
        /// </summary>
        public bool IsAuthenticated = false;

        /// <summary>
        /// Should encryption be used?
        /// </summary>
        public bool UseEncryption = true;

        /// <summary>
        /// The current encryption key
        /// </summary>
        string currentEncryptionKey;

        /// <summary>
        /// The encryption key used during the login process.
        /// </summary>
        string initialEncryptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:JARVIS.Shared.Protocol.JCP"/> class.
        /// </summary>
        /// <param name="useEncryption">If set to <c>true</c> packets should be encrypted.</param>
        /// <param name="encryptionKey">The encryption key.</param>
        public JCP(bool useEncryption = true, string encryptionKey = "max")
        {
            UseEncryption = useEncryption;
            initialEncryptionKey = encryptionKey;
            currentEncryptionKey = encryptionKey;

            Log.Message("Socket", "Using JCP Version " + Version.ToString());
        }

        /// <summary>
        /// Gets the bytes of a given packet.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="packet">The target packet.</param>
        public byte[] GetBytes(Packet packet)
        {
            Packet[] quickArray = { packet };
            return GetBytes(quickArray);
        }
        /// <summary>
        /// Gets the bytes of an array of packets.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="packets">The packets.</param>
        public byte[] GetBytes(Packet[] packets)
        {
            List<byte> returnBytes = new List<byte>();

            foreach (Packet p in packets)
            {
                byte[] data = p.ToBytes(UseEncryption, EncryptionKey);

                if (data.Length > 0)
                {
                    // Add length (as bytes)
                    returnBytes.AddRange(BitConverter.GetBytes(data.Length));

                    // Add data
                    returnBytes.AddRange(data);
                }
            }

            return returnBytes.ToArray();
        }
    }

}
