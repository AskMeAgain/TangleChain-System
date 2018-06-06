using System;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Entity;
using System.Linq;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Collections.Generic;
using TangleChain.Classes;

namespace TangleChain {
    public static class Utils {

        public static int ProofOfWork(string origHash, int difficulty) {

            int nonce = 0;

            while (true) {

                Curl curl = new Curl();
                curl.Absorb(TryteString.FromAsciiString(origHash).ToTrits());
                curl.Absorb(TryteString.FromAsciiString(nonce + "").ToTrits());

                var hash = new int[120];
                curl.Squeeze(hash);

                if (CheckPOWResult(hash, difficulty))
                    return nonce;

                nonce++;
            }
        }

        public static bool VerifyHash(string hash, int nonce, int difficulty) {

            Curl curl = new Curl();
            curl.Absorb(TryteString.FromAsciiString(hash).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(nonce + "").ToTrits());

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

        public static string GenerateNextAddr(int height, string sendTo, long time) {

            Curl sponge = new Curl();
            sponge.Absorb(height.ToTrits());
            sponge.Absorb(TryteString.FromAsciiString(sendTo).ToTrits());
            sponge.Absorb(TryteString.FromAsciiString(time + "").ToTrits());

            var hash = new int[243];
            sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            return trytes;

        }

        public static string HashCurl(string text, int length) {

            Curl sponge = new Curl();
            sponge.Absorb(TryteString.FromAsciiString(text).ToTrits());

            var hash = new int[length * 3];
            sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            return trytes;
        }

        public static bool VerifyBlock(Block block, int difficulty) {

            return VerifyHash(block.Hash, block.Nonce, difficulty);

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

        public static string GetTransactionPoolAddress(int height, string coinName) {

            int interval = Settings.TransactionPoolInterval;

            string num = height / interval * interval + "";

            return HashCurl(coinName + "_" + num, 81);

        }

        public static string FillTransactionPool(int num, string coinName, int height) {

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
