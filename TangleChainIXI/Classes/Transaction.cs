using LiteDB;
using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Data.SQLite;
using Nethereum.Hex.HexConvertors;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.Classes
{

    [Serializable]
    public class Transaction: IDownloadable,ISignable
    {

        public string Hash { get; set; }
        public bool IsFinalized { get; set; }
        public string SendTo { get; set; }

        public long Time { get; set; }
        public string From { get; set; }
        public string Signature { get; set; }
        public int Mode { get; set; }
        public List<int> OutputValue { get; set; }
        public List<string> OutputReceiver { get; set; }
        public List<string> Data { get; set; }

        public Transaction(string from, int mode, string transPoolAddress)
        {
            SendTo = transPoolAddress;
            From = from;
            Mode = mode;

            OutputReceiver = new List<string>();
            OutputValue = new List<int>();
            Data = new List<string>();
        }

        public bool Verify()
        {
            return Hash.VerifyMessage(Signature, From);
        }

        public Transaction() { }

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

        public int ComputeOutgoingValues()
        {

            int sum = 0;

            //we get the trans fees
            sum += Int32.Parse(Data[0]);

            //and then each outgoing transaction value
            OutputValue.ForEach(m => sum += m);

            return sum;
        }

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
    }
}
