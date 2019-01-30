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

        public static Maybe<T> FromJSON<T>(string json) where T : IDownloadable
        {
            try
            {
                return Maybe<T>.Some(JsonConvert.DeserializeObject<T>(json, new JsonISCTypeConverter(), new CustomJsonConverter()));
            }
            catch (Exception)
            {
                return Maybe<T>.None;
            }
        }

        public static string ToJSON(this IDownloadable obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonISCTypeConverter(), new CustomJsonConverter());
        }

        public static string GetTransactionPoolAddress(long height, string coinName, int interval = -1)
        {
            if (height == 0)
                return Hasher.Hash(81, (coinName.ToLower(), "_GENESIS_POOL"));

            string num = height / interval * interval + "";
            return Hasher.Hash(81, num, '_', coinName.ToLower());
        }

        public static string ToFlatList(this List<Expression> list)
        {

            var str = "";

            list.ForEach(x => str += x.ToString().Replace(" ", "."));

            return str.Replace("\n", "");
        }
    }
}
