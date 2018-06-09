using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Utils;
using Tangle.Net.Cryptography;
using System.Linq;
using System.Data.SQLite;

namespace TangleChainIXI.Classes {

    [Serializable]
    public class Block {

        public long Height { get; set; }
        public int Nonce { get; set; }
        public long Time { get; set; }
        public string Hash { get; set; }
        public string NextAddress { get; set; }
        public string Owner { get; set; }
        public string SendTo { get; set; }
        public string CoinName { get; set; }

        public List<string> TransactionHashes { get; private set; }

        public Block(Block block) {
            Nonce = block.Nonce;
            Height = block.Height;
            Time = block.Time;
            Hash = block.Hash;
            NextAddress = block.NextAddress;
            Owner = block.Owner;
            SendTo = block.SendTo;
            CoinName = block.CoinName;
            TransactionHashes = block.TransactionHashes;
        }

        public Block(long height, string sendTo, string coinName) {
            Height = height;
            SendTo = sendTo;
            CoinName = coinName;
            TransactionHashes = new List<string>();
        }

        public void AddTransactions(Transaction trans) {

            var transList = new List<Transaction>();
            transList.Add(trans);

            AddTransactions(transList);
        }

        public void AddTransactions(List<Transaction> list) {
            
            string sendTo = Utils.GetTransactionPoolAddress(Height, CoinName);

            TransactionHashes.AddRange(list.Select(m => m.Hash));

        }

        public void GenerateProofOfWork(int difficulty) {
            Nonce = Utils.ProofOfWork(Hash,difficulty);
        }

        private void GenerateHash() {

            try {
                Curl curl = new Curl();
                curl.Absorb(TangleNet.TryteString.FromAsciiString(Height + "").ToTrits());
                curl.Absorb(TangleNet.TryteString.FromAsciiString(Time + "").ToTrits());
                curl.Absorb(TangleNet.TryteString.FromAsciiString(NextAddress).ToTrits());
                curl.Absorb(TangleNet.TryteString.FromAsciiString(Owner).ToTrits());
                curl.Absorb(TangleNet.TryteString.FromAsciiString(SendTo).ToTrits());
                curl.Absorb(TangleNet.TryteString.FromAsciiString(CoinName).ToTrits());

                var hash = new int[243];
                curl.Squeeze(hash);

                Hash = Converter.TritsToTrytes(hash);
            }
            catch (Exception e) {
                Console.WriteLine("Missing Var: " + e);
                throw;
            }       
        }

        public void Final() {

            Time = Timestamp.UnixSecondsTimestamp;
            NextAddress = Utils.GenerateNextAddr(Height, SendTo, Time);
            Owner = Settings.Owner;

            GenerateHash();
        }

#region Utility    

        public Block() {}

        public Block(SQLiteDataReader reader, string name) {

            Height = (int)reader[0];
            Nonce = (int) reader[1];
            Time = (long) reader[2];
            Hash = (string) reader[3];
            NextAddress = (string) reader[4];
            Owner = (string) reader[5];
            SendTo = (string) reader[6];
            CoinName = name;

        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Block FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Block>(json);
        }

        public void Print() {
            Console.WriteLine("Height: " + Height);
            Console.WriteLine("Block Hash: " + Hash);
            Console.WriteLine("Time: " + Time);
            Console.WriteLine("Next Address: " + NextAddress);
            Console.WriteLine("Owner: " + Owner);
            Console.WriteLine("SendTo: " + SendTo);
            Console.WriteLine("CoinName: " + CoinName);

        }

        public override bool Equals(object obj) {

            Block newBlock = obj as Block;

            if (newBlock == null)
                return false;

            newBlock.GenerateHash();

            GenerateHash();

            return newBlock.Hash.Equals(Hash);

        }

        #endregion

    }
}
