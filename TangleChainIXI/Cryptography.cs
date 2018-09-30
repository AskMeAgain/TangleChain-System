using System;
using NetherumSigner = Nethereum.Signer;
using Nethereum.Hex.HexConvertors;
using TangleChainIXI.Classes;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using System.Threading;
using Tangle.Net.Cryptography;
using System.Collections.Generic;
using System.Linq;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI {
    public static class Cryptography {

        //lookup table for difficulty stuff... could have used log, but it was to late lol
        static Dictionary<double, int> lookup = new Dictionary<double, int>(){
            {0.0123 ,-4},
            {0.037 ,-3},
            {0.111 ,-2},
            {0.333 ,-1},
            {1,0},
            {3,1},
            {27,2},
            {81,3},
            {243,4},
            {729,5},
            {2187,6}
        };

        public static string Sign(string data, string privKey) {

            NetherumSigner::EthereumMessageSigner gen = new NetherumSigner::EthereumMessageSigner();

            HexUTF8StringConvertor conv = new HexUTF8StringConvertor();
            string hexPrivKey = conv.ConvertToHex(privKey);

            return gen.EncodeUTF8AndSign(data, new NetherumSigner::EthECKey(hexPrivKey));
        }

        public static string GetPublicKey(string s) {

            HexUTF8StringConvertor conv = new HexUTF8StringConvertor();
            string hexPrivKey = conv.ConvertToHex(s);

            return NetherumSigner::EthECKey.GetPublicAddress(hexPrivKey);
        }

        public static bool VerifyMessage(string message, string signature, string pubKey) {

            NetherumSigner::EthereumMessageSigner gen = new NetherumSigner::EthereumMessageSigner();
            string addr;
            try {
                addr = gen.EncodeUTF8AndEcRecover(message, signature);
            } catch {
                return false;
            }

            return (addr.ToLower().Equals(pubKey.ToLower())) ? true : false;
        }

        public static bool VerifyHashAndNonceAgainstDifficulty(Block block, Difficulty difficulty) {

            block.GenerateHash();

            return VerifyHashAndNonceAgainstDifficulty(block.Hash, block.Nonce, difficulty);

        }

        public static bool VerifyHashAndNonceAgainstDifficulty(string hash, long nonce, Difficulty difficulty) {

            Curl curl = new Curl();
            curl.Absorb(TangleNet::TryteString.FromAsciiString(hash).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(nonce + "").ToTrits());

            var trits = new int[120];
            curl.Squeeze(trits);

            return VerifyHashAgainstDifficulty(trits, difficulty);

        }

        public static bool VerifyHashAgainstDifficulty(int[] trits, Difficulty difficulty) {

            //check Preceding Zeros
            for (int i = 0; i < difficulty.PrecedingZeros; i++) {
                if (trits[i] != 0)
                    return false;
            }

            return true;
        }

        public static long ProofOfWork(string origHash, Difficulty difficulty) {
            return ProofOfWork(origHash, difficulty, new CancellationTokenSource().Token);
        }

        public static long ProofOfWork(string origHash, Difficulty difficulty, CancellationToken token) {

            long nonce = 0;

            while (!token.IsCancellationRequested) {

                if (VerifyHashAndNonceAgainstDifficulty(origHash, nonce, difficulty))
                    return nonce;

                nonce++;
            }

            return -1;

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

        public static bool SmartcontractsAreCorrect(Block block)
        {
            //TODO           
            return true;
        }

        public static string GenerateNextAddress(string blockHash, string sendTo) {

            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(blockHash).ToTrits());
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(sendTo).ToTrits());

            var result = new int[243];
            sponge.Squeeze(result);

            return Converter.TritsToTrytes(result);

        }

        public static int CalculateDifficultyChange(double multi) {

            var ConvertedArray = lookup.Keys.OrderBy(m => m).ToArray();

            for (int i = 0; i < ConvertedArray.Length - 1; i++) {

                if (multi >= ConvertedArray[i] && multi <= ConvertedArray[i + 1]) {

                    var testLeft = ConvertedArray[i] - multi;
                    var testRight = ConvertedArray[i + 1] - multi;

                    if (Math.Abs(testLeft) < Math.Abs(testRight))
                        return lookup[ConvertedArray[i]];


                    return lookup[ConvertedArray[i + 1]];
                }
            }

            return 0;
        }

        public static bool VerifyBlock(Block block, Difficulty difficulty) {

            //check if hash got correctly computed
            if (difficulty != null && !VerifyBlockHash(block))
                return false;

            //check if POW got correctly computed
            if (difficulty != null && !VerifyHashAndNonceAgainstDifficulty(block.Hash, block.Nonce, difficulty))
                return false;

            //checks if every transaction in this block is correct (spending, signatures etc)
            if (!TransactionsAreCorrect(block))
                return false;

            //checks if every smartcontract in this block is correct (spending, signatures etc)
            if (!SmartcontractsAreCorrect(block))
                return false;

            //check if next address is correctly computed
            if (!GenerateNextAddress(block.Hash, block.SendTo).Equals(block.NextAddress))
                return false;

            return true;

        }

        public static bool VerifyBlockHash(Block block) {

            string oldHash = block.Hash;
            block.GenerateHash();

            return oldHash.Equals(block.Hash);
        }

        public static string HashList<T>(this List<T> list, int Length) {

            string s = "";

            foreach (T t in list) {
                s += t.ToString();
            }

            return HashCurl(s,Length);
        }

    }



}
