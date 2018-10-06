using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tangle.Net.Cryptography;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Utils;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Classes
{
    public static class BlockExtensions
    {
        public static Block Final(this Block block)
        {

            block.Time = Timestamp.UnixSecondsTimestamp;
            block.Owner = IXISettings.PublicKey;

            block.GenerateHash();

            block.NextAddress = Cryptography.GenerateNextAddress(block.Hash, block.SendTo);

            block.IsFinalized = true;

            return block;
        }

        public static Block AddSmartcontract(this Block block, Smartcontract smart)
        {
            if (block.SmartcontractHashes == null)
                block.SmartcontractHashes = new List<string>();

            block.SmartcontractHashes.Add(smart.Hash);

            return block;
        }

        public static Block AddTransaction(this Block block, Transaction trans)
        {
            if (block.TransactionHashes == null)
                block.TransactionHashes = new List<string>();

            block.TransactionHashes.Add(trans.Hash);

            return block;
        }

        public static Block AddTransactions(this Block block, List<Transaction> list)
        {
            if (list != null)
                block.TransactionHashes.AddRange(list.Select(m => m.Hash));
            return block;
        }

        public static Block AddTransactionHashes(this Block block, List<string> list)
        {
            if (block.TransactionHashes == null)
                block.TransactionHashes = new List<string>();

            block.TransactionHashes.AddRange(list);

            return block;

        }

        public static Block GenerateHash(this Block block)
        {

            Curl curl = new Curl();
            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.Height + "").ToTrits());
            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.Time + "").ToTrits());
            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.Owner).ToTrits());
            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.SendTo).ToTrits());
            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.CoinName).ToTrits());

            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.TransactionHashes.HashList(20) + "").ToTrits());
            curl.Absorb(Tangle.Net.Entity.TryteString.FromAsciiString(block.SmartcontractHashes.HashList(20) + "").ToTrits());


            var hash = new int[243];
            curl.Squeeze(hash);

            block.Hash = Converter.TritsToTrytes(hash);

            return block;

        }

        public static Block AddSmartcontractHashes(this Block block, List<string> smartList)
        {
            if (block.SmartcontractHashes == null)
                block.SmartcontractHashes = new List<string>();

            block.TransactionHashes.AddRange(smartList);

            return block;
        }

        public static Block GenerateProofOfWork(this Block block, Difficulty difficulty)
        {
            return block.GenerateProofOfWork(difficulty, new CancellationTokenSource().Token);
        }

        public static Block GenerateProofOfWork(this Block block, Difficulty difficulty, CancellationToken token)
        {
            block.Nonce = Cryptography.ProofOfWork(block.Hash, difficulty, token);
            block.Difficulty = difficulty;

            return block;
        }

    }
}
