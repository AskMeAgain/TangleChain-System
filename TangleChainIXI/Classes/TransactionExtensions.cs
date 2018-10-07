using System;
using System.Collections.Generic;
using System.Text;
using Tangle.Net.Cryptography;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Utils;
using TangleNet = Tangle.Net.Entity;


namespace TangleChainIXI.Classes
{
    public static class TransactionExtensions
    {

        public static Transaction AddFee(this Transaction trans, int fee)
        {
            if (trans.Data == null)
                trans.Data = new List<string>();

            trans.Data.Add(fee + "");

            return trans;

        }

        public static Transaction Final(this Transaction trans)
        {
            InitLists(trans);

            trans.Time = Timestamp.UnixSecondsTimestamp;
            trans.GenerateHash();
            trans.Sign();
            trans.IsFinalized = true;

            return trans;
        }

        private static void InitLists(Transaction trans) {
            if (trans.OutputReceiver == null)
                trans.OutputReceiver = new List<string>();

            if (trans.OutputValue == null)
                trans.OutputValue = new List<int>();
        }

        public static Transaction AddOutput(this Transaction trans, int value, string receiver)
        {

            if (value < 0)
                return trans;

            InitLists(trans);

            trans.OutputValue.Add(value);
            trans.OutputReceiver.Add(receiver);

            return trans;

        }

        public static Transaction SetGenesisInformation(this Transaction trans, int BlockReward, int RewardReduction, int ReductionFactor, int TransactionsPerBlock, int BlockTime, int TransInterval, int diffi)
        {

            trans.Data = new List<string>();
            trans.Data.Add("0");
            trans.Data.Add(BlockReward + "");
            trans.Data.Add(RewardReduction + "");
            trans.Data.Add(ReductionFactor + "");
            trans.Data.Add(TransactionsPerBlock + "");
            trans.Data.Add(BlockTime + "");
            trans.Data.Add(TransInterval + "");
            trans.Data.Add(diffi + "");
            trans.Mode = -1;

            return trans;
        }

        public static Transaction SetGenesisInformation(this Transaction trans, ChainSettings set)
        {

            trans.Data = new List<string>();

            trans.Data.Add("0");
            trans.Data.Add(set.BlockReward + "");
            trans.Data.Add(set.RewardReduction + "");
            trans.Data.Add(set.ReductionFactor + "");
            trans.Data.Add(set.TransactionsPerBlock + "");
            trans.Data.Add(set.BlockTime + "");
            trans.Data.Add(set.TransactionPoolInterval + "");
            trans.Data.Add(set.DifficultyAdjustment + "");
            trans.Mode = -1;

            return trans;

        }

        public static Transaction GenerateHash(this Transaction trans)
        {

            Curl curl = new Curl();

            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.SendTo).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.From).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.Mode + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.Data.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.Time + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.OutputValue.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.OutputReceiver.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(trans.Data.GetHashCode() + "").ToTrits());

            var hash = new int[60];
            curl.Squeeze(hash);

            trans.Hash = Converter.TritsToTrytes(hash);

            return trans;
        }

        public static Transaction Sign(this Transaction trans)
        {
            if (trans.Mode == -1)
                trans.Signature = "GENESIS";
            else if (trans.Mode == 100)
                trans.Signature = "SMARTCONTRACTRESULT";
            else
                trans.Signature = Cryptography.Sign(trans.Hash, IXISettings.PrivateKey);

            return trans;
        }

        public static Transaction AddData(this Transaction trans, string data) {

            if (trans.Data == null)
                trans.Data = new List<string>();

            trans.Data.Add(data);

            return trans;

        }
    }
}
