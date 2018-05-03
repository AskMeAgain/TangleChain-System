using System;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Entity;
using System.Linq;

namespace TangleChain {
    public static class Utils {

        public static Block GetBlockFromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Block>(json);
        }

        public static string GetStringFromBlock(Block block) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(block);
        }

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
            curl.Absorb(TryteString.FromAsciiString(nonce+"").ToTrits());

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

    }
}
