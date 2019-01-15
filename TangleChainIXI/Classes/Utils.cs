using System;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using System.Linq;
using Tangle.Net.Cryptography;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using System.Threading;
using Newtonsoft.Json;
using Tangle.Net.Repository;
using RestSharp;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Classes
{
    public static class Utils
    {
        public static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            TypeNameHandling = TypeNameHandling.All
        };

        public static string GenerateRandomString(int n)
        {

            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ9";
            return new string(Enumerable.Repeat(chars, n).Select(s => s[random.Next(s.Length)]).ToArray());

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

        public static T FromJSON<T>(string json) where T : IDownloadable
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, new JsonISCTypeConverter());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default(T);
            }
        }

        public static string ToJSON(this IDownloadable obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonISCTypeConverter());
        }

        public static string GetTransactionPoolAddress(long height,string coinName, int interval = -1)
        {
            if (height == 0)
                return (coinName.ToLower() + "_GENESIS_POOL").HashCurl(81);

            string num = height / interval * interval + "";
            return (num + "_" + coinName.ToLower()).HashCurl(81);
        }
    }
}
