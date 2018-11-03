using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Utils;
using Tangle.Net.Cryptography;
using System.Linq;
using System.Data.SQLite;
using System.Threading;
using Newtonsoft.Json;
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

        [JsonIgnore]
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
            TransactionHashes = new List<string>();
            SmartcontractHashes = new List<string>();
        }

        /// <summary>
        /// Constructor for JSONConverter
        /// </summary>
        public Block()
        {
            TransactionHashes = new List<string>();
            SmartcontractHashes = new List<string>();
        }

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
            TransactionHashes = new List<string>();
            SmartcontractHashes = new List<string>();
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

        public Block Add<T>(List<T> list) where T : ISignable
        {
            list.ForEach(x => Add<T>(x.Hash));

            return this;

        }

        public Block Add<T>(List<string> list) where T : ISignable
        {
            list.ForEach(x => Add<T>(x));

            return this;

        }

        public Block Add<T>(string hash) where T : ISignable
        {

            if (typeof(T) == typeof(Smartcontract))
            {
                SmartcontractHashes.Add(hash);
            }
            else if (typeof(T) == typeof(Transaction))
            {
                TransactionHashes.Add(hash);
            }
            else
            {
                throw new ArgumentException("Wrong type this should never happen!");
            }

            return this;
        }

        public Block Add<T>(T obj) where T : ISignable
        {
            Add<T>(obj.Hash);

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
        public Block GenerateProofOfWork(CancellationToken token)
        {
            if (Difficulty == 0)
                return GenerateProofOfWork(DBManager.GetDifficulty(CoinName, Height));
            return GenerateProofOfWork(Difficulty, token);

        }

        public Block GenerateProofOfWork()
        {
            if (Difficulty == 0)
                return GenerateProofOfWork(DBManager.GetDifficulty(CoinName, Height));
            return GenerateProofOfWork(Difficulty);

        }

        /// <summary>
        /// Adds the needed difficulty to block
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public Block SetDifficulty(int difficulty)
        {
            Difficulty = difficulty;
            return this;
        }
    }
}
