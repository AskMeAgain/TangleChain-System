using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Utils;
using Tangle.Net.Cryptography;
using System.Linq;
using System.Data.SQLite;
using System.Threading;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Classes
{

    [Serializable]
    public class Block
    {

        public long Height { get; set; }
        public long Nonce { get; set; }
        public long Time { get; set; }
        public string Hash { get; set; }
        public string NextAddress { get; set; }
        public string Owner { get; set; }
        public string SendTo { get; set; }
        public string CoinName { get; set; }
        public Difficulty Difficulty { get; set; }

        public List<string> TransactionHashes { get; set; }
        public List<string> SmartcontractHashes { get; set; }

        public Block(long height, string sendTo, string coinName)
        {
            Height = height;
            SendTo = sendTo;
            CoinName = coinName;
            TransactionHashes = new List<string>();
            SmartcontractHashes = new List<string>();
        }

        public Block()
        {
            TransactionHashes = new List<string>();
            SmartcontractHashes = new List<string>();
        }

        public Block(SQLiteDataReader reader, string name)
        {

            Height = (int)reader[0];
            Nonce = (int)reader[1];
            Time = (long)reader[2];
            Hash = (string)reader[3];
            NextAddress = (string)reader[4];
            Owner = (string)reader[5];
            SendTo = (string)reader[6];
            CoinName = name;
            Difficulty = new Difficulty((int)reader[7]);

        }

        public void AddSmartcontract(Smartcontract smart)
        {
            SmartcontractHashes.Add(smart.Hash);
        }

        public void AddTransactions(Transaction trans)
        {

            var transList = new List<Transaction>();
            transList.Add(trans);

            AddTransactions(transList);
        }

        public void AddTransactions(List<Transaction> list)
        {
            if (list != null)
                TransactionHashes.AddRange(list.Select(m => m.Hash));
        }

        public void AddTransactionHashes(List<string> list) {
            if (TransactionHashes == null)
                TransactionHashes = new List<string>();

            TransactionHashes.AddRange(list);

        }

        public void GenerateProofOfWork(Difficulty difficulty)
        {
            GenerateProofOfWork(difficulty, new CancellationTokenSource().Token);
        }

        public void GenerateProofOfWork(Difficulty difficulty, CancellationToken token)
        {
            Nonce = Cryptography.ProofOfWork(Hash, difficulty, token);
            Difficulty = difficulty;
        }

        public void GenerateHash()
        {

            Curl curl = new Curl();
            curl.Absorb(TangleNet.TryteString.FromAsciiString(Height + "").ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(Time + "").ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(Owner).ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(SendTo).ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(CoinName).ToTrits());

            curl.Absorb(TangleNet.TryteString.FromAsciiString(TransactionHashes.HashList(20) + "").ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(SmartcontractHashes.HashList(20) + "").ToTrits());


            var hash = new int[243];
            curl.Squeeze(hash);

            Hash = Converter.TritsToTrytes(hash);

        }

        public void Final()
        {

            Time = Timestamp.UnixSecondsTimestamp;
            Owner = IXISettings.PublicKey;

            GenerateHash();

            NextAddress = Cryptography.GenerateNextAddress(Hash, SendTo);

        }

        #region Utility    

        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Block FromJSON(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Block>(json);
        }

        public void Print()
        {
            Console.WriteLine("Height: " + Height);
            Console.WriteLine("Block Hash: " + Hash);
            Console.WriteLine("Time: " + Time);
            Console.WriteLine("Next Address: " + NextAddress);
            Console.WriteLine("PublicKey: " + Owner);
            Console.WriteLine("SendTo: " + SendTo);
            Console.WriteLine("CoinName: " + CoinName);

            //Console.WriteLine("TransactionPool Address: " + Utils.GetTransactionPoolAddress(Height,CoinName));

        }

        public override bool Equals(object obj)
        {

            Block newBlock = obj as Block;

            if (newBlock == null)
                return false;

            newBlock.GenerateHash();

            GenerateHash();

            return newBlock.Hash.Equals(Hash);

        }

        #endregion

        public void AddSmartcontractHashes(List<string> smartList) {
            if (SmartcontractHashes == null)
                SmartcontractHashes = new List<string>();

            TransactionHashes.AddRange(smartList);
        }
    }
}
