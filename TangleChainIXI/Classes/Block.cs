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
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.Classes
{

    [Serializable]
    public class Block : IDownloadable
    {

        public long Height { get; set; }
        public long Nonce { get; set; }
        public long Time { get; set; }
        public bool IsFinalized { get; set; }
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

        public bool Verify(Difficulty difficulty)
        {
            return Cryptography.VerifyBlock(this, difficulty);
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

            this.GenerateHash();

            return newBlock.Hash.Equals(Hash);

        }
        
    }
}
