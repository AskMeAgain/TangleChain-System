using System;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using System.Linq;
using Tangle.Net.Cryptography;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using System.Threading;
using Tangle.Net.Repository;
using RestSharp;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI
{
    public static class Utils
    {

        public static string GenerateRandomString(int n)
        {

            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ9";
            return new string(Enumerable.Repeat(chars, n).Select(s => s[random.Next(s.Length)]).ToArray());

        }

        public static string Intendation(int n)
        {

            string s = "";

            for (int i = 0; i < n; i++)
            {
                s += "\t";
            }

            return s;
        }

        public static int GenerateRandomInt(int n)
        {

            Random random = new Random();

            const string chars = "0123456789";

            string num = new string(Enumerable.Repeat(chars, n).Select(s => s[random.Next(s.Length)]).ToArray());

            return int.Parse(num);

        }

        public static List<Way> ToWayList(this List<Block> blocks)
        {

            var wayList = new List<Way>();

            foreach (Block block in blocks)
                wayList.Add(new Way(block));

            return wayList;
        }

        public static string GetTransactionPoolAddress(long height, string coinName)
        {

            if (height == 0)
                return Cryptography.HashCurl(coinName.ToLower() + "_GENESIS_POOL", 81);

            int interval = DBManager.GetChainSettings(coinName).TransactionPoolInterval;

            return GetTransactionPoolAddress(height, coinName, interval);

        }

        public static string GetTransactionPoolAddress(long height, string coinName, int interval)
        {

            if (height == 0)
                return Cryptography.HashCurl(coinName.ToLower() + "_GENESIS_POOL", 81);

            string num = height / interval * interval + "";
            return Cryptography.HashCurl(num + "_" + coinName.ToLower(), 81);

        }

        public static bool TestConnection(string url)
        {
            try
            {
                var repository = new RestIotaRepository(new RestClient(url));
                var info = repository.GetNodeInfo();
            }
            catch
            {
                return false;
            }

            return true;

        }

        public static T FromJSON<T>(string json) where T : IDownloadable
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJSON(this IDownloadable obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

    }
}
