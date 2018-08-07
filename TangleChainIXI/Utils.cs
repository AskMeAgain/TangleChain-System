﻿using System;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using System.Linq;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using System.Security.Cryptography;
using System.Threading;
using Tangle.Net.Repository;
using RestSharp;

namespace TangleChainIXI {
    public static class Utils {

        public static long ProofOfWork(string origHash, Difficulty difficulty) {
            return ProofOfWork(origHash, difficulty, new CancellationTokenSource().Token);
        }

        public static long ProofOfWork(string origHash, Difficulty difficulty, CancellationToken token) {

            long nonce = 0;

            while (!token.IsCancellationRequested) {

                Curl curl = new Curl();
                curl.Absorb(TangleNet::TryteString.FromAsciiString(origHash).ToTrits());
                curl.Absorb(TangleNet::TryteString.FromAsciiString(nonce + "").ToTrits());

                var hash = new int[120];
                curl.Squeeze(hash);

                string result = Converter.TritsToTrytes(hash);

                if (CheckPOWResult(result, difficulty))
                    return nonce;

                nonce++;
            }

            return -1;

        }

        public static bool VerifyHash(Block block, Difficulty difficulty) {

            block.GenerateHash();

            return VerifyHash(block.Hash, block.Nonce, difficulty);

        }

        public static bool VerifyHash(string hash, long nonce, Difficulty difficulty) {

            Curl curl = new Curl();
            curl.Absorb(TangleNet::TryteString.FromAsciiString(hash).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(nonce + "").ToTrits());

            var temp = new int[120];
            curl.Squeeze(temp);

            string result = Converter.TritsToTrytes(temp);

            return CheckPOWResult(result, difficulty);

        }

        public static bool CheckPOWResult(string result, Difficulty difficulty) {

            //check Preceding Zeros
            for (int i = 0; i < difficulty.PrecedingZeros; i++) {
                if (!result[i].Equals('A'))
                    return false;
            }
  
            //check last letter
            if (ConvertLettertoNumber(result[difficulty.PrecedingZeros].ToString()) <= difficulty.Number)
                return true;
            
            return false;
        }

        public static string GenerateNextAddr(string blockHash, string sendTo) {

            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(blockHash).ToTrits());
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(sendTo).ToTrits());

            var result = new int[243];
            sponge.Squeeze(result);

            return Converter.TritsToTrytes(result);

        }

        public static string HashCurl(string text, int length) {


            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(text).ToTrits());

            var hash = new int[length * 3];
            sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            //Console.WriteLine("Hashing: {0} results in {1}",text,trytes);

            return trytes;
        }

        public static bool TransactionsAreCorrect(Block block) {

            var hashSet = new HashSet<string>();
            DataBase Db = new DataBase(block.CoinName);

            if (block.Height == 0)
                return true;

            var transList = Core.GetAllTransactionsFromBlock(block);

            if (transList == null)
                return false;

            Dictionary<string, long> balances = new Dictionary<string, long>();

            //check if address can spend and are legit
            foreach (Transaction trans in transList) {

                //check if signature is correct
                if (!trans.VerifySignature())
                    return false;

                if (!balances.ContainsKey(trans.From))
                    balances.Add(trans.From, Db.GetBalance(trans.From));
            }

            foreach (Transaction trans in transList) {

                balances[trans.From] -= trans.ComputeOutgoingValues();

                if (balances[trans.From] < 0)
                    return false;

            }

            return true;
        }

        public static bool VerifyBlock(Block block, Difficulty difficulty) {

            Block oldBlock = block;
            block.GenerateHash();

            if (oldBlock != block)
                return false;

            if (!VerifyHash(block.Hash, block.Nonce, difficulty))
                return false;

            if (!TransactionsAreCorrect(block))
                return false;

            //check if next address is correctly computed:
            if (!GenerateNextAddr(block.Hash, block.SendTo).Equals(block.NextAddress))
                return false;

            return true;

        }

        public static string GenerateRandomString(int n) {

            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ9";
            return new string(Enumerable.Repeat(chars, n).Select(s => s[random.Next(s.Length)]).ToArray());

        }

        public static int GenerateRandomInt(int n) {

            Random random = new Random();

            const string chars = "0123456789";

            string num = new string(Enumerable.Repeat(chars, n).Select(s => s[random.Next(s.Length)]).ToArray());

            return int.Parse(num);

        }

        public static List<Way> ConvertBlocklistToWays(List<Block> blocks, Difficulty difficulty) {

            var wayList = new List<Way>();

            foreach (Block block in blocks)
                wayList.Add(new Way(block.Hash, block.SendTo, block.Height, difficulty));

            return wayList;
        }

        public static string GetTransactionPoolAddress(long height, string coinName) {

            int interval = IXISettings.GetChainSettings(coinName).TransactionPoolInterval;

            string num = height / interval * interval + "";

            if (height == 0)
                return HashCurl(coinName.ToLower() + "_GENESIS_POOL", 81);


            return HashCurl(num + "_" + coinName.ToLower(), 81);

        }

        public static string FillTransactionPool(string owner, string receiver, int numOfTransactions, string coinName, long height) {

            string addr = GetTransactionPoolAddress(height, coinName);

            for (int i = 0; i < numOfTransactions; i++) {

                //we create now the transactions
                Transaction trans = new Transaction(owner, 1, addr);
                trans.AddOutput(100, receiver);
                trans.AddFee(0);
                trans.Final();

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            return addr;
        }

        public static bool TestConnection(string url) {
            try {
                var repository = new RestIotaRepository(new RestClient(url));
                var info = repository.GetNodeInfo();
            } catch (Exception e) {
                return false;
            }

            return true;

        }

        public static int ConvertLettertoNumber(string Num){			
            return (Num.Equals('9'))? 27 : Convert.ToChar(Num) - 65;
        }
    }
}
