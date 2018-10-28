using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.IO;

namespace ConsoleMiner
{
    public static class Utils
    {

        static string pathFlag = Environment.CurrentDirectory + @"/flag.txt";

        public static void WriteFlag()
        {
            //we create file as flag
            if (!File.Exists(pathFlag))
            {
                using (FileStream fs = File.Create(pathFlag))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes("Started Process. Deleting this will stop the process.");
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        public static bool FlagIsSet()
        {
            return File.Exists(pathFlag);
        }

        public static void DeleteFlag()
        {
            if (File.Exists(pathFlag))
            {
                File.Delete(pathFlag);
            }
        }


        public static T Print<T>(this T obj, string text)
        {
            Console.WriteLine(text);
            return obj;
        }

        private static void Print(string text, bool readKey)
        {
            Console.WriteLine(text);

            if (readKey)
                Console.ReadKey();
        }

        public static void Print(string text, bool readKey, string para1 = "", string para2 = "", string para3 = "")
        {
            string s = string.Format(text, para1, para2, para3);
            Print(s, readKey);
        }

        public static string IsConnectionEstablished(List<string> nodeAddress)
        {
            Print("Testing Connection...", false);

            foreach (string s in nodeAddress)
            {
                if (TangleChainIXI.Utils.TestConnection(s))
                {
                    TangleChainIXI.Classes.IXISettings.SetNodeAddress(s);
                    Print("Connection established\n", false);
                    return s;
                }
            }

            return null;
        }


    }
}
