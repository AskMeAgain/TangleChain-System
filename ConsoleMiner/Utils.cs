using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.IO;

namespace ConsoleMiner {
    public static class Utils {

        static string pathFlag = Environment.CurrentDirectory + @"/flag.txt";
        private static string pathInit = Environment.CurrentDirectory + @"/init.json";

        public static void WriteFlag() {

            //we create file as flag
            if (!File.Exists(pathFlag)) {
                using (FileStream fs = File.Create(pathFlag)) {
                    Byte[] info = new UTF8Encoding(true).GetBytes("Started Process. Deleting this will stop the process.");
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        public static bool FlagIsSet() {
            return File.Exists(pathFlag);
        }

        public static void DeleteFlag() {
            if (File.Exists(pathFlag)) {
                File.Delete(pathFlag);
            }
        }

        public static void CreateInitFile() {

            if (File.Exists(pathInit)) {
                File.Delete(pathInit);
            }

            Settings set = new Settings {
                CoreNumber = 1,
                MinedChain = new Settings.Chain("JVGRPWYTD9HJDSBQFQIPMXYXWMM9LODEXIZWSD9VRCBRSNXJYOPWT9DMMLAYMLBABOXRSSKIYVCHTYANF",
                "JBTTEVXLUUSCBLRPAXIAOZLA9MYPVPPQXGI9FJATLO9QTJ9HFLPAAD9UCKENHIMANHKOXYOGSPJOGCCVK",
                "TestCoin"),
                NodeAddress = new List<string> {
                    @"https://pea.iotasalad.org:14265",
                    @"https://wallet2.iota.town:443"
                }
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(set, Newtonsoft.Json.Formatting.Indented);

            using (FileStream fs = File.Create(pathInit)) {
                Byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        public static void WriteInitFile(Settings set) {

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(set, Newtonsoft.Json.Formatting.Indented);

            using (FileStream fs = File.Create(pathInit)) {
                Byte[] info = new UTF8Encoding(true).GetBytes(json);
                fs.Write(info, 0, info.Length);
            }
        }

        public static Settings ReadInitFile() {

            if (File.Exists(Environment.CurrentDirectory + @"/init.json"))
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory + @"/init.json"));
            else
                return null;
        }

    }
}
