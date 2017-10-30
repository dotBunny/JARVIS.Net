﻿using System.Collections.Generic;

namespace JARVIS.Core.Database.Tables
{
    public class Settings : ITable
    {

        public const string ServerHostID = "Server.Host";
        public const string ServerWebPortID = "Server.WebPort";
        public const string ServerSocketPortID = "Server.SocketPort";
        public const string ServerSocketEncryptionKeyID = "Server.SocketEncryptionKey";

        public string Name { get; private set; }
        public string Value { get; private set; }


        public Settings(string name, string newValue)
        {
            Name = name;
            Value = newValue;
        }

        public static string GetTableName()
        {
            return "Settings";
        }

        public static string GetCreation()
        {
            return "CREATE TABLE \"" + GetTableName() + "\" (\"Name\" varchar(128) NOT NULL, \"Value\" varchar(128) PRIMARY KEY (\"Name\"))";
        }

       
        public static void Set(string key, string newValue)
        {
   
            key = Shared.Strings.Truncate(key, 128);
            newValue = Shared.Strings.Truncate(newValue, 128);

            Shared.Log.Message("DB", "Set " + key + " to " + newValue);

            Server.Database.ExecuteNonQueryAsyc(
                "REPLACE INTO \"" + GetTableName() + "\" (\"Name\", \"Value\") VALUES (\"" + key + "\", " + newValue +")"
            );
        }

        public static Dictionary<string, string> GetAll()
        {
            Dictionary<string, string> returnDictionary = new Dictionary<string, string>();

            Provider.ProviderResult result = Server.Database.ExecuteQuery(
                "SELECT * FROM \"" + GetTableName() + "\"", 
                System.Data.CommandBehavior.Default);

            if (result.Data != null && result.Data.HasRows)
            {
                while (result.Data.HasRows)
                {
                    while (result.Data.Read())
                    {
                        returnDictionary.Add(result.Data.GetString(0), result.Data.GetString(1));
                    }
                    result.Data.NextResult();
                }
            }

            return returnDictionary;
        }

    }
}
