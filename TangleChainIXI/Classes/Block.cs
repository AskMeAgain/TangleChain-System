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
using Tangle.Net.Entity;

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
        public int Difficulty { get; set; }

        public List<string> TransactionHashes { get; set; }
        public List<string> SmartcontractHashes { get; set; }

        /// <summary>
        /// The standard constructor for Block
        /// </summary>
        /// <param name="height">The height of the block</param>
        /// <param name="sendTo">The address where the block will be send</param>
        /// <param name="coinName">The name of the coin</param>
        public Block(long height, string sendTo, string coinName)
        {
            Height = height;
            SendTo = sendTo;
            CoinName = coinName;
        }

        /// <summary>
        /// Constructor for JSONConverter
        /// </summary>
        public Block() { }

        /// <summary>
        /// Constructor for SQLite reader from a Database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
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
            Difficulty = (int)reader[7];

        }

        /// <summary>
        /// Verifies the block
        /// </summary>
        /// <param name="difficulty">The difficulty</param>
        /// <returns>Returns true if the block is legit</returns>
        public bool Verify(int difficulty)
        {
            return this.VerifyBlock(difficulty);
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

        /// <summary>
        /// Generates the hash of the block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public IDownloadable GenerateHash()
        {

            Curl curl = new Curl();
            curl.Absorb(TryteString.FromAsciiString(Height + "").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Time + "").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Owner).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(SendTo).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(CoinName).ToTrits());

            curl.Absorb(TryteString.FromAsciiString(TransactionHashes.HashList(20) + "").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(SmartcontractHashes.HashList(20) + "").ToTrits());

            var hash = new int[243];
            curl.Squeeze(hash);

            Hash = Converter.TritsToTrytes(hash);

            return this;

        }

        /// <summary>
        /// Finalizes the block: Adds a time, the owner as specified in ixisettings.publickey, generates the hash & adds the next address
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public Block Final()
        {

            Time = Timestamp.UnixSecondsTimestamp;
            Owner = IXISettings.PublicKey;

            GenerateHash();

            NextAddress = Cryptography.GenerateNextAddress(Hash, SendTo);

            IsFinalized = true;

            return this;
        }

        /// <summary>
        /// Adds the smartcontract to the block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="smart"></param>
        /// <returns></returns>
        public Block AddSmartcontract(Smartcontract smart)
        {
            if (SmartcontractHashes == null)
                SmartcontractHashes = new List<string>();

            SmartcontractHashes.Add(smart.Hash);

            return this;
        }

        /// <summary>
        /// Adds a transaction to the   Internally only the hash of the block will be stored.
        /// The Transaction needs to get uploaded to the correct transactionPoolAddress too
        /// </summary>
        /// <param name="block"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Block AddTransaction(Transaction trans)
        {
            AddTransactionHash(trans.Hash);

            return this;
        }

        /// <summary>
        /// Adds a transaction list to the   Internally only the hashes will be stored inside the block
        /// The Transaction needs to get uploaded to the correct transactionPoolAddress too
        /// </summary>
        /// <param name="block"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Block AddTransactions(List<Transaction> list)
        {
            list.ForEach(x => AddTransaction(x));

            return this;
        }

        /// <summary>
        /// Adds transaction hashes to the block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Block AddTransactionHashes(List<string> list)
        {
            list.ForEach(x => AddTransactionHash(x));

            return this;
        }

        /// <summary>
        /// adds a transaction hash to the block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public Block AddTransactionHash(string hash)
        {
            if (TransactionHashes == null)
                TransactionHashes = new List<string>();

            TransactionHashes.Add(hash);

            return this;
        }

        /// <summary>
        /// Adds smartcontract hash to the  
        /// The Smartcontract needs to get uploaded to the correct transactionPoolAddress too
        /// </summary>
        /// <param name="block"></param>
        /// <param name="smartList"></param>
        /// <returns></returns>
        public Block AddSmartcontractHashes(List<string> smartList)
        {
            if (SmartcontractHashes == null)
                SmartcontractHashes = new List<string>();

            if (TransactionHashes == null)
                TransactionHashes = new List<string>();

            TransactionHashes.AddRange(smartList);

            return this;
        }

        /// <summary>
        /// Generates the Proof of work
        /// </summary>
        /// <param name="block"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public Block GenerateProofOfWork(int difficulty)
        {
            return GenerateProofOfWork(difficulty, new CancellationTokenSource().Token);
        }

        /// <summary>
        /// Generates the proof of work with a cancellation token
        /// </summary>
        /// <param name="block"></param>
        /// <param name="difficulty"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Block GenerateProofOfWork(int difficulty, CancellationToken token)
        {
            Nonce = Cryptography.ProofOfWork(Hash, difficulty, token);
            Difficulty = difficulty;

            return this;
        }

        /// <summary>
        /// automaticly handles every settings if you downloaded the whole chain.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public Block GenerateProofOfWork()
        {

            int difficulty = DBManager.GetDifficulty(CoinName, Height);
            return GenerateProofOfWork(difficulty);

        }
    }
}
