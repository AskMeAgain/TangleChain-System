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

            return CheckPOWResult(result, difficulty) ? true : false;

        }

        public static bool CheckPOWResult(int[] hash, int difficulty) {

            for (int i = 0; i < difficulty; i++) {
                if (hash[i] != 0)
                    return false;
            }
            return true;
        }

        public static string GenerateNextAddr(int Height, string SendTo, long Time) {

            Curl Sponge = new Curl();
            Sponge.Absorb(Height.ToTrits());
            Sponge.Absorb(TryteString.FromAsciiString(SendTo).ToTrits());
            Sponge.Absorb(TryteString.FromAsciiString(Time+"").ToTrits());

            var hash = new int[243];
            Sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            return trytes.ToString();

        }

        public static string Hash_Curl(string text, int length) {

            Curl Sponge = new Curl();
            Sponge.Absorb(TryteString.FromAsciiString(text).ToTrits());

            var hash = new int[length];
            Sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            return trytes.ToString();
        }

        public static bool VerifyBlock(Block block, int difficulty) {

            return VerifyHash(block.Hash, block.Nonce, difficulty);

        }

        public static List<Way> ConvertBlocklistToWays(List<Block> blocks) {

            List<Way> ways = new List<Way>();

            foreach (Block block in blocks)
                ways.Add(new Way(block.Hash, block.SendTo,block.Height));

            return ways;
        }

        public static string GenerateTransactionPoolAddress(int height) {

            int interval = Settings.TransactionPoolInterval;

            string num = ((int)(height / interval) * interval) + "";

            return Hash_Curl(num, 243);

        }

        

    }
}
