using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Data.SQLite;
using Nethereum.Hex.HexConvertors;
using Newtonsoft.Json;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.Classes
{

    [Serializable]
    public class Transaction : IDownloadable, ISignable
    {

        public string Hash { get; set; }

        public bool IsFinalized { get; set; }

        [JsonIgnore]
        public string NodeAddress { get; set; }

        public string SendTo { get; set; }

        public long Time { get; set; }
        public string From { get; set; }
        public string Signature { get; set; }
        public int Mode { get; set; }
        public List<int> OutputValue { get; set; }
        public List<string> OutputReceiver { get; set; }
        public List<string> Data { get; set; }

        /// <summary>
        /// Standard constructor for a transaction
        /// </summary>
        /// <param name="from">The public key of the person who wants to send coins</param>
        /// <param name="mode">The mode of the transaction. See github for each mode</param>
        /// <param name="transPoolAddress">The transaction pool address where you need to send the transaction</param>
        public Transaction(string from, int mode, string transPoolAddress)
        {
            SendTo = transPoolAddress;
            From = from;
            Mode = mode;

            OutputReceiver = new List<string>();
            OutputValue = new List<int>();
            Data = new List<string>();
        }

        public Transaction() { }

        /// <summary>
        /// The constructor for an sqlitedata reader from a Database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="value"></param>
        /// <param name="receiver"></param>
        /// <param name="data"></param>
        public Transaction(SQLiteDataReader reader, List<int> value, List<string> receiver, List<string> data)
        {

            Hash = (string)reader[1];
            Time = (long)reader[2];
            From = (string)reader[3];
            Signature = (string)reader[4];
            Mode = (int)reader[5];

            OutputValue = value;
            OutputReceiver = receiver;
            Data = data;

        }

        /// <summary>
        /// Computes the outgoing values from this Transaction
        /// </summary>
        /// <returns></returns>
        public int ComputeOutgoingValues()
        {

            int sum = 0;

            //we get the trans fees
            sum += Int32.Parse(Data[0]);

            //and then each outgoing transaction value
            OutputValue.ForEach(m => sum += m);

            return sum;
        }

        /// <summary>
        /// Computes the miner reward for this transaction (just the transaction fee)
        /// </summary>
        /// <returns></returns>
        public int ComputeMinerReward()
        {
            return Int32.Parse(Data[0]);
        }

        public void Print()
        {
            Console.WriteLine("Hash " + Hash);
            Console.WriteLine("FROM " + From);
            Console.WriteLine("Signature " + Signature);
            Console.WriteLine("Sendto " + SendTo);
            Console.WriteLine("Mode" + Mode);
            Console.WriteLine("data count " + Data.Count);
            Console.WriteLine("===========================================");

        }

        public override bool Equals(object obj)
        {

            Transaction newTrans = obj as Transaction;

            if (newTrans == null)
                return false;

            return (newTrans.Hash.Equals(Hash) && newTrans.SendTo.Equals(SendTo));
        }

        /// <summary>
        /// Generates the hash of the transaction
        /// </summary>
        /// <returns></returns>
        public IDownloadable GenerateHash()
        {

            Curl curl = new Curl();

            curl.Absorb(TangleNet::TryteString.FromAsciiString(SendTo).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(From).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Mode + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Data.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Time + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(OutputValue.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(OutputReceiver.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Data.GetHashCode() + "").ToTrits());

            var hash = new int[60];
            curl.Squeeze(hash);

            Hash = Converter.TritsToTrytes(hash);

            return this;
        }

        /// <summary>
        /// Adds a transaction fee to the transaction
        /// </summary>
        /// <param name="fee"></param>
        /// <returns></returns>
        public Transaction AddFee(int fee)
        {
            if (Data == null)
                Data = new List<string>();

            Data.Add(fee + "");

            return this;

        }

        /// <summary>
        /// Finalizes the transaction
        /// </summary>
        /// <returns></returns>
        public Transaction Final(IXISettings settings)
        {

            Time = Timestamp.UnixSecondsTimestamp;
            GenerateHash();
            Sign(settings);
            NodeAddress = settings.NodeAddress;
            IsFinalized = true;

            return this;
        }

        /// <summary>
        /// Adds an output to a transaction
        /// </summary>
        /// <param name="value"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public Transaction AddOutput(int value, string receiver)
        {

            if (value < 0)
                return this;

            OutputValue.Add(value);
            OutputReceiver.Add(receiver);

            return this;

        }

        public Transaction SetGenesisInformation(int BlockReward, int RewardReduction, int ReductionFactor, int TransactionsPerBlock, int BlockTime, int TransInterval, int diffi)
        {

            Data = new List<string>();
            Data.Add("0");
            Data.Add(BlockReward + "");
            Data.Add(RewardReduction + "");
            Data.Add(ReductionFactor + "");
            Data.Add(TransactionsPerBlock + "");
            Data.Add(BlockTime + "");
            Data.Add(TransInterval + "");
            Data.Add(diffi + "");
            Mode = -1;

            return this;
        }

        public Transaction SetGenesisInformation(ChainSettings set)
        {

            Data = new List<string>();

            Data.Add("0");
            Data.Add(set.BlockReward + "");
            Data.Add(set.RewardReduction + "");
            Data.Add(set.ReductionFactor + "");
            Data.Add(set.TransactionsPerBlock + "");
            Data.Add(set.BlockTime + "");
            Data.Add(set.TransactionPoolInterval + "");
            Data.Add(set.DifficultyAdjustment + "");
            Mode = -1;

            return this;

        }

        /// <summary>
        /// Signs the transaction
        /// </summary>
        /// <returns></returns>
        public void Sign(IXISettings settings)
        {
            if (Mode == -1)
                Signature = "GENESIS";
            else if (Mode == 100)
                Signature = "SMARTCONTRACTRESULT";
            else
                Signature = Cryptography.Sign(Hash, settings.PrivateKey);
        }

        public int GetFee()
        {
            return (int.TryParse(Data[0], out int result)) ? result : 0;
        }

        public Transaction AddData(string data)
        {

            if (Data == null)
                Data = new List<string>();

            Data.Add(data);

            return this;

        }
    }
}
