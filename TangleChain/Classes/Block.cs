using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Entity;
using Tangle.Net.Utils;
using Tangle.Net.Cryptography;
using LiteDB;

namespace TangleChain.Classes {

    [Serializable]
    public class Block {

        public int Nonce { get; set; }

        [BsonId]
        public int Height { get; set; }

        public long Time { get; set; }

        public string Hash { get; set; }
        public string NextAddress { get; set; }
        public string Owner { get; set; }
        public string SendTo { get; set; }
        public string CoinName { get; set; }


        public override bool Equals(object obj) {

            Block newBlock = obj as Block;
            newBlock.GenerateHash();

            GenerateHash();

            return newBlock.Hash.Equals(Hash);

        }

        public Block() {
            Nonce = 123456;
            Hash = "HASH";
            Height = 123456;
            NextAddress = "NEXTADDRESS";
            Owner = "OWNER";
            SendTo = "CBVYKBQWSUMUDPPTLQFPSDHGSJYVPUOKREWSDHRAMYRGI9YALHGRZXJAKZIYZHGFPMYPMWIGUWBNVPVCB";
            Time = Timestamp.UnixSecondsTimestamp;
            CoinName = "TestCoin";

            GenerateHash();
        }

        public void GenerateHash() {

            Curl curl = new Curl();
            curl.Absorb(TryteString.FromAsciiString(Height + "").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Time + "").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(NextAddress).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Owner).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(SendTo).ToTrits());

            var hash = new int[243];
            curl.Squeeze(hash);

            Hash = Converter.TritsToTrytes(hash).ToString();

        }

        public void Print() {
            Console.WriteLine("Height: " + Height);
            Console.WriteLine("SendTo: " + SendTo);
            Console.WriteLine("Block Hash: " + Hash);
        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Block FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Block>(json);
        }

        public static Block CreateBlock(int height, string SendTo) {

            long t = Timestamp.UnixSecondsTimestamp;

            Block block = new Block() {
                Height = height,
                Time = t,
                SendTo = SendTo,
                Owner = "ME",
                NextAddress = Utils.GenerateNextAddr(height, SendTo, t)
            };

            //generate hash from the insides
            block.GenerateHash();

            return block;

        }

    }
}
