﻿using System;
using System.Collections.Generic;
using System.IO;
namespace Developer_Tools
{
    class DS_TextFile
    {
        public static void createNewFile(string path)
        {
            StreamWriter sw = File.AppendText(path);
            sw.Close();
            sw.Dispose();
        }
        public static string readTextFile(string path)
        {
            string fileContents = "";
            fileContents = System.IO.File.ReadAllText(@path);
            return fileContents;
        }
    }
}
