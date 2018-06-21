using System;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using System.Linq;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using System.Security.Cryptography;

namespace TangleChainIXI {
    public static class Utils {

        public static int ProofOfWork(string origHash, int difficulty) {

            int nonce = 0;

            while (true) {

                Curl curl = new Curl();
                curl.Absorb(TangleNet::TryteString.FromAsciiString(origHash).ToTrits());
                curl.Absorb(TangleNet::TryteString.FromAsciiString(nonce + "").ToTrits());

                var hash = new int[120];
                curl.Squeeze(hash);

                if (CheckPOWResult(hash, difficulty))
                    return nonce;

                nonce++;
            }
        }

        public static bool VerifyHash(string hash, int nonce, int difficulty) {

            Curl curl = new Curl();
            curl.Absorb(TangleNet::TryteString.FromAsciiString(hash).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(nonce + "").ToTrits());

            var result = new int[120];
            curl.Squeeze(result);

            return CheckPOWResult(result, difficulty);

        }

        public static bool CheckPOWResult(int[] hash, int difficulty) {

            for (int i = 0; i < difficulty; i++) {
                if (hash[i] != 0)
                    return false;
            }
            return true;
        }

        public static string GenerateNextAddr(long height, string sendTo, long time) {

            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(height + "").ToTrits());
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(sendTo).ToTrits());
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(time + "").ToTrits());

            var hash = new int[243];
            sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            return trytes;

        }

        public static string HashCurl(string text, int length) {

            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(text).ToTrits());

            var hash = new int[length * 3];
            sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

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

            //if translist contains dupes, block is invalid
            //if (transList.Any(e => !hashSet.Add(e.From)))
            //    return false;

            //check if address can spend
            foreach (Transaction trans in transList) {
                if (Db.GetBalance(trans.From) - trans.ComputeOutgoingValues() < 0)
                    return false;
            }

            return true;
        }

        public static bool VerifyBlock(Block block, int difficulty) {

            if (!VerifyHash(block.Hash, block.Nonce, difficulty))
                return false;

            if (!TransactionsAreCorrect(block))
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

        public static List<Way> ConvertBlocklistToWays(List<Block> blocks) {

            var wayList = new List<Way>();

            foreach (Block block in blocks)
                wayList.Add(new Way(block.Hash, block.SendTo, block.Height));

            return wayList;
        }

        public static string GetTransactionPoolAddress(long height, string coinName) {

            int interval = Settings.GetChainSettings(coinName).TransactionPoolInterval;

            string num = height / interval * interval + "";

            return HashCurl(coinName + "_" + num, 81);

        }

        public static string FillTransactionPool(int num, string coinName, long height) {

            string addr = GetTransactionPoolAddress(height, coinName);

            Random rnd = new Random();

            for (int i = 0; i < num; i++) {

                //we create now the transactions
                Classes.Transaction trans = new Classes.Transaction("ME", 1, addr);
                trans.AddOutput(100, "YOU");
                trans.AddFee(10);
                trans.Final();

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            return addr;
        }

    }
}
