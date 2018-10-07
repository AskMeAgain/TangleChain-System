using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tangle.Net.Cryptography;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Entity;
using Tangle.Net.Utils;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Classes
{
    public static class BlockExtensions
    {
        /// <summary>
        /// Finalizes the block: Adds a time, the owner as specified in ixisettings.publickey, generates the hash & adds the next address
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static Block Final(this Block block)
        {

            block.Time = Timestamp.UnixSecondsTimestamp;
            block.Owner = IXISettings.PublicKey;

            block.GenerateHash();

            block.NextAddress = Cryptography.GenerateNextAddress(block.Hash, block.SendTo);

            block.IsFinalized = true;

            return block;
        }

        /// <summary>
        /// Adds the smartcontract to the block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="smart"></param>
        /// <returns></returns>
        public static Block AddSmartcontract(this Block block, Smartcontract smart)
        {
            if (block.SmartcontractHashes == null)
                block.SmartcontractHashes = new List<string>();

            block.SmartcontractHashes.Add(smart.Hash);

            return block;
        }

        /// <summary>
        /// Adds a transaction to the block. Internally only the hash of the block will be stored.
        /// The Transaction needs to get uploaded to the correct transactionPoolAddress too
        /// </summary>
        /// <param name="block"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static Block AddTransaction(this Block block, Transaction trans)
        {
            block.AddTransactionHash(trans.Hash);

            return block;
        }

        /// <summary>
        /// Adds a transaction list to the block. Internally only the hashes will be stored inside the block
        /// The Transaction needs to get uploaded to the correct transactionPoolAddress too
        /// </summary>
        /// <param name="block"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Block AddTransactions(this Block block, List<Transaction> list)
        {
            list.ForEach(x => block.AddTransaction(x));

            return block;
        }

        /// <summary>
        /// Adds transaction hashes to the block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Block AddTransactionHashes(this Block block, List<string> list)
        {
            list.ForEach(x => block.AddTransactionHash(x));

            return block;
        }

        /// <summary>
        /// adds a transaction hash to the block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Block AddTransactionHash(this Block block, string hash)
        {
            if (block.TransactionHashes == null)
                block.TransactionHashes = new List<string>();

            block.TransactionHashes.Add(hash);

            return block;
        }

        /// <summary>
        /// Adds smartcontract hash to the block.
        /// The Smartcontract needs to get uploaded to the correct transactionPoolAddress too
        /// </summary>
        /// <param name="block"></param>
        /// <param name="smartList"></param>
        /// <returns></returns>
        public static Block AddSmartcontractHashes(this Block block, List<string> smartList)
        {
            if (block.SmartcontractHashes == null)
                block.SmartcontractHashes = new List<string>();

            if (block.TransactionHashes == null)
                block.TransactionHashes = new List<string>();

            block.TransactionHashes.AddRange(smartList);

            return block;
        }

        /// <summary>
        /// Generates the Proof of work
        /// </summary>
        /// <param name="block"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static Block GenerateProofOfWork(this Block block, int difficulty)
        {
            return block.GenerateProofOfWork(difficulty, new CancellationTokenSource().Token);
        }

        /// <summary>
        /// Generates the proof of work with a cancellation token
        /// </summary>
        /// <param name="block"></param>
        /// <param name="difficulty"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Block GenerateProofOfWork(this Block block, int difficulty, CancellationToken token)
        {
            block.Nonce = Cryptography.ProofOfWork(block.Hash, difficulty, token);
            block.Difficulty = difficulty;

            return block;
        }

        /// <summary>
        /// automaticly handles every settings if you downloaded the whole chain.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static Block GenerateProofOfWork(this Block block) {

            int difficulty = DBManager.GetDifficulty(block.CoinName, block.Height);
            return block.GenerateProofOfWork(difficulty);

        }
    }
}
