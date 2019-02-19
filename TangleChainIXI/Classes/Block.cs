using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Utils;
using Tangle.Net.Cryptography;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public string NodeAddress { get; set; }

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
        /// Verifies the block
        /// </summary>
        /// <param name="difficulty">The difficulty</param>
        /// <returns>Returns true if the block is legit</returns>
        public bool Verify(int difficulty)
        {
            return VerifyBlock(difficulty);
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

        public Block Final(IXISettings settings)
        {

            Time = Timestamp.UnixSecondsTimestamp;
            Owner = settings.PublicKey;

            GenerateHash();

            NodeAddress = settings.NodeAddress;
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
            IsFinalized = false;

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
            return Add<T>(obj.Hash);
        }

        public IDownloadable GenerateProofOfWork(IXICore ixiCore, CancellationToken token = default)
        {
            Difficulty = ixiCore.GetDifficulty(Height);
            Nonce = Cryptography.ProofOfWork(Hash, Difficulty, token);

            return this;
        }

        public bool VerifyBlock(int difficulty)
        {

            //check if hash got correctly computed
            if (!this.VerifyHash())
                return false;

            //check if POW got correctly computed
            if (!this.VerifyNonce(difficulty))
                return false;

            //check if next address is correctly computed
            if (!this.VerifyNextAddress())
                return false;

            return true;

        }
    }
}
