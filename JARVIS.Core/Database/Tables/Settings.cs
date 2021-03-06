﻿using System.Collections.Generic;
using JARVIS.Core.Database.Rows;

namespace JARVIS.Core.Database.Tables
{
    /// <summary>
    /// JARVIS Settings Table
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The settings key of the database version.
        /// </summary>
        public const string DatabaseVersionKey = "Database.Version";

        /// <summary>
        /// The settings key of the server host.
        /// </summary>
        public const string ServerHostKey = "Server.Host";

        /// <summary>
        /// The settings key of the salt used when hashing passwords.
        /// </summary>
        public const string ServerSaltKey = "Server.Salt";

        /// <summary>
        /// The settings key of the socket encryption setting.
        /// </summary>
        public const string ServerSocketEncryptionKey = "Server.SocketEncryption";

        /// <summary>
        /// The settings key of the socket encryption key.
        /// </summary>
        public const string ServerSocketEncryptionKeyKey = "Server.SocketEncryptionKey";

        /// <summary>
        /// The settings key for the socket listen port.
        /// </summary>
        public const string ServerSocketPortKey = "Server.SocketPort";

        /// <summary>
        /// The settings key of the web server listen port.
        /// </summary>
        public const string ServerWebPortKey = "Server.WebPort";

        /// <summary>
        /// Get the specified row from the Settings table.
        /// </summary>
        /// <returns>The specified row.</returns>
        /// <param name="key">The settings key.</param>
        public static KeyValueRow<string> Get(string key)
        {
            key = Shared.Strings.Truncate(key, 128);

            Provider.ProviderResult result = Server.Database.ExecuteQuery(
                "SELECT Value FROM Settings WHERE Name = @Name LIMIT 1",
                new Dictionary<string, object>() {
                    {"@Name",key}
            }, System.Data.CommandBehavior.SingleResult);

            if (result.Data != null && result.Data.HasRows)
            {
                result.Data.Read();
                return new KeyValueRow<string>(key, result.Data.GetString(0));
            }

            return new KeyValueRow<string>();
        }

        /// <summary>
        /// Get all of the rows from the Settings table.
        /// </summary>
        /// <returns>All rows from the table.</returns>
        public static List<KeyValueRow<string>> GetAll()
        {
            List<KeyValueRow<string>> Rows = new List<KeyValueRow<string>>();

            Provider.ProviderResult result = Server.Database.ExecuteQuery(
                "SELECT Name, Value FROM Settings",
                new Dictionary<string, object>()
            );

            if (result.Data != null && result.Data.HasRows)
            {
                while (result.Data.Read())
                {
                    // Handle NULL Value
                    if (result.Data.IsDBNull(1))
                    {
                        Rows.Add(new KeyValueRow<string>(result.Data.GetString(0), string.Empty));
                    } else {
                        Rows.Add(new KeyValueRow<string>(result.Data.GetString(0), result.Data.GetString(1)));    
                    }

                    result.Data.NextResult();
                }
            }

            return Rows;
        }

        /// <summary>
        /// Set the specified key of the settings.
        /// </summary>
        /// <param name="key">Setting Key</param>
        /// <param name="newValue">Setting Value</param>
        public static void Set(string key, string newValue)
        {
            key = Shared.Strings.Truncate(key, 128);
            newValue = Shared.Strings.Truncate(newValue, 128);

            Shared.Log.Message("DB", "Set " + key + " to " + newValue);

            Server.Database.ExecuteNonQuery(
                "REPLACE INTO Settings (Name, Value) VALUES (@Name, @Value)",
                new Dictionary<string, object>() {
                    {"@Name",key},
                    {"@Value",newValue},
                }
            );
        }
    }
}
