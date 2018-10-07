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
using TangleChainIXI.Interfaces;

namespace TangleChainIXI
{
    public static class Cryptography
    {

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

        /// <summary>
        /// Signs the given data with a private key
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="privKey">Private key</param>
        /// <returns></returns>
        public static string Sign(string data, string privKey)
        {

            NetherumSigner::EthereumMessageSigner gen = new NetherumSigner::EthereumMessageSigner();

            HexUTF8StringConvertor conv = new HexUTF8StringConvertor();
            string hexPrivKey = conv.ConvertToHex(privKey);

            return gen.EncodeUTF8AndSign(data, new NetherumSigner::EthECKey(hexPrivKey));
        }

        /// <summary>
        /// Returns the public key from a given private key
        /// </summary>
        /// <param name="privateKey">The key</param>
        /// <returns></returns>
        public static string GetPublicKey(this string privateKey)
        {

            HexUTF8StringConvertor conv = new HexUTF8StringConvertor();
            string hexPrivKey = conv.ConvertToHex(privateKey);

            return NetherumSigner::EthECKey.GetPublicAddress(hexPrivKey);
        }

        /// <summary>
        /// Finds a fitting nonce with the given difficulty
        /// </summary>
        /// <param name="hash">The hash where we want to find the correct nonce</param>
        /// <param name="difficulty">The given difficulty</param>
        /// <returns></returns>
        public static long ProofOfWork(string hash, Difficulty difficulty)
        {
            return ProofOfWork(hash, difficulty, new CancellationTokenSource().Token);
        }

        /// <summary>
        /// Finds a fitting nonce with the given difficulty
        /// </summary>
        /// <param name="hash">The hash where we want to find the correct nonce</param>
        /// <param name="difficulty">The given difficulty</param>
        /// <param name="token">Takes a CancellationToken</param>
        /// <returns></returns>
        public static long ProofOfWork(string hash, Difficulty difficulty, CancellationToken token)
        {

            long nonce = 0;

            while (!token.IsCancellationRequested)
            {

                if (VerifyNonce(hash, nonce, difficulty))
                    return nonce;

                nonce++;
            }

            return -1;

        }

        /// <summary>
        /// Generates NextAddress from a hash and sendto
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        public static string GenerateNextAddress(string blockHash, string sendTo)
        {

            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(blockHash).ToTrits());
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(sendTo).ToTrits());

            var result = new int[243];
            sponge.Squeeze(result);

            return Converter.TritsToTrytes(result);

        }

        /// <summary>
        /// Calculates the difficulty change from a multiplier
        /// </summary>
        /// <param name="multi">Multiplier</param>
        /// <returns></returns>
        public static int CalculateDifficultyChange(double multi)
        {

            var ConvertedArray = lookup.Keys.OrderBy(m => m).ToArray();

            for (int i = 0; i < ConvertedArray.Length - 1; i++)
            {

                if (multi >= ConvertedArray[i] && multi <= ConvertedArray[i + 1])
                {

                    var testLeft = ConvertedArray[i] - multi;
                    var testRight = ConvertedArray[i + 1] - multi;

                    if (Math.Abs(testLeft) < Math.Abs(testRight))
                        return lookup[ConvertedArray[i]];


                    return lookup[ConvertedArray[i + 1]];
                }
            }

            return 0;
        }

        /// <summary>
        /// Hashes a list to a hash with the specified length
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length">The Length of the resulting hash</param>
        /// <returns></returns>
        public static string HashList<T>(this List<T> list, int length)
        {

            if (list == null)
                return "".HashCurl(length);

            string s = "";

            foreach (T t in list)
            {
                s += t.ToString();
            }

            return s.HashCurl(length);
        }

        /// <summary>
        /// Hashes a given string to a hash with the specified length
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length">The length of the resulting hash</param>
        /// <returns></returns>
        public static string HashCurl(this string text, int length)
        {

            Curl sponge = new Curl();
            sponge.Absorb(TangleNet::TryteString.FromAsciiString(text).ToTrits());

            var hash = new int[length * 3];
            sponge.Squeeze(hash);

            var trytes = Converter.TritsToTrytes(hash);

            return trytes;
        }

        /// <summary>
        /// Verifies all transactions from a given block. Takes a while because it downloads stuff
        /// </summary>
        /// <param name="block">The block</param>
        /// <returns></returns>
        public static bool VerifyTransactions(this Block block)
        {

            var hashSet = new HashSet<string>();
            DataBase Db = new DataBase(block.CoinName);

            if (block.Height == 0)
                return true;

            var transList = Core.GetAllFromBlock<Transaction>(block);

            if (transList == null)
                return false;

            Dictionary<string, long> balances = new Dictionary<string, long>();

            //check if address can spend and are legit
            foreach (Transaction trans in transList)
            {

                //check if signature is correct
                if (!trans.Verify())
                    return false;

                if (!balances.ContainsKey(trans.From))
                    balances.Add(trans.From, Db.GetBalance(trans.From));
            }

            foreach (Transaction trans in transList)
            {

                balances[trans.From] -= trans.ComputeOutgoingValues();

                if (balances[trans.From] < 0)
                    return false;

            }

            return true;
        }

        /// <summary>
        /// Verifies all smartcontracts from a given block. Takes a while because it downloads stuff
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static bool VerifySmartcontracts(this Block block)
        {

            //get objects first
            var list = Core.GetAllFromBlock<Smartcontract>(block);

            foreach (Smartcontract smart in list)
            {
                //we need to check if smartcontract has correct hash
                if (!smart.VerifyHash())
                    return false;

                //we need to check if the receiving address is correct
                if (!smart.VerifyReceivingAddress())
                    return false;

                //we need to check if the signature is correct
                if (!smart.VerifySignature())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a boolean which indicates wheather the smartcontract was correctly signed or not
        /// </summary>
        /// <returns>If true the smartcontract is correctly signed</returns>
        public static bool VerifySignature(this ISignable obj)
        {
            return obj.Hash.VerifyMessage(obj.Signature, obj.From);
        }

        /// <summary>
        /// Verifies the receiving address of a smartcontract
        /// </summary>
        /// <param name="smart"></param>
        /// <returns></returns>
        public static bool VerifyReceivingAddress(this Smartcontract smart)
        {
            return smart.Hash.GetPublicKey().Equals(smart.ReceivingAddress);
        }

        /// <summary>
        /// Verifies a message with a given signature and a public key
        /// </summary>
        /// <param name="message">The message which got signed</param>
        /// <param name="signature">The Signature</param>
        /// <param name="pubKey">The publickey to check against</param>
        /// <returns></returns>
        public static bool VerifyMessage(this string message, string signature, string pubKey)
        {

            NetherumSigner::EthereumMessageSigner gen = new NetherumSigner::EthereumMessageSigner();
            string addr;
            try
            {
                addr = gen.EncodeUTF8AndEcRecover(message, signature);
            }
            catch
            {
                return false;
            }

            return (addr.ToLower().Equals(pubKey.ToLower())) ? true : false;
        }

        /// <summary>
        /// Verifes If the given block has the correct nonce with the specified difficulty
        /// </summary>
        /// <param name="block">The Block</param>
        /// <param name="difficulty">The Difficulty</param>
        /// <returns></returns>
        public static bool VerifyNonce(this Block block, Difficulty difficulty)
        {

            block.GenerateHash();

            return VerifyNonce(block.Hash, block.Nonce, difficulty);

        }

        /// <summary>
        /// Checks if the hash and nonce result in the dificulty
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="nonce"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static bool VerifyNonce(this string hash, long nonce, Difficulty difficulty)
        {

            Curl curl = new Curl();
            curl.Absorb(TangleNet::TryteString.FromAsciiString(hash).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(nonce + "").ToTrits());

            var trits = new int[120];
            curl.Squeeze(trits);

            return trits.VerifyDifficulty(difficulty);

        }

        /// <summary>
        /// Checks if the int array with trits has X leading 0
        /// </summary>
        /// <param name="trits"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static bool VerifyDifficulty(this int[] trits, Difficulty difficulty)
        {
            //check Preceding Zeros
            for (int i = 0; i < difficulty.PrecedingZeros; i++)
            {
                if (trits[i] != 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies that a block is correct. Does POW Check, Transaction & Smartcontract check and checks if the block got correctly computed
        /// </summary>
        /// <param name="block"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static bool VerifyBlock(this Block block, Difficulty difficulty)
        {

            //check if hash got correctly computed
            if (difficulty != null && !block.VerifyHash())
                return false;

            //check if POW got correctly computed
            if (difficulty != null && !block.VerifyNonce(difficulty))
                return false;

            //checks if every transaction in this block is correct (spending, signatures etc)
            if (!block.VerifyTransactions())
                return false;

            //checks if every smartcontract in this block is correct (spending, signatures etc)
            if (!block.VerifySmartcontracts())
                return false;

            //check if next address is correctly computed
            if (!block.VerifyNextAddress())
                return false;

            return true;

        }

        /// <summary>
        /// Verifies the next address of a block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private static bool VerifyNextAddress(this Block block)
        {
            return GenerateNextAddress(block.Hash, block.SendTo).Equals(block.NextAddress);
        }

        /// <summary>
        /// Checks if the blockhash is correctly computed
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool VerifyHash(this IDownloadable obj)
        {

            string oldHash = obj.Hash;
            obj.GenerateHash();

            return oldHash.Equals(obj.Hash);
        }

    }



}
