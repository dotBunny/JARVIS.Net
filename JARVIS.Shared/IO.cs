﻿using System.IO;
using System.Collections.Generic;

namespace JARVIS.Shared
{
    public static class IO
    {
        public static void WriteContents(string file, string contents)
        {
            File.WriteAllText(file, contents, System.Text.Encoding.UTF8);
        }
        public static void WriteContentsAsync(string file, string contents)
        {
            File.WriteAllTextAsync(file, contents, System.Text.Encoding.UTF8);
        }
        public static void AppendContents(string file, string contents)
        {
            File.AppendAllText(file, contents);
        }
        public static void AppendContents(string file, List<string> contents)
        {
            File.AppendAllLines(file, contents);
        }
        public static void AppendContentsAsync(string file, string contents)
        {
            File.AppendAllTextAsync(file, contents);
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
