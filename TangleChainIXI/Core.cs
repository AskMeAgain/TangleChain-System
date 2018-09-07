using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI {

    public static class Core {

        #region basic functionality

        public static List<TangleNet::TransactionTrytes> UploadBlock(Block block) {

            //get sending address
            String sendTo = block.SendTo;

            //prepare data
            string json = block.ToJSON();
            var blockJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
              new TangleNet::Transfer {
                  Address = new TangleNet::Address(sendTo),
                  Tag = new TangleNet::Tag("TANGLECHAIN"),
                  Message = blockJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            return repository.SendTrytes(bundle.Transactions, 10, 14);

        }

        public static List<TangleNet::TransactionTrytes> UploadTransaction(Transaction trans) {

            if (trans.Hash == null)
                throw new ArgumentException("Transaction is not finalized. Did you forget to Final() the Transaction?");

            //get sending address
            String sendTo = trans.TransactionPoolAddress;

            //prepare data
            string json = trans.ToJSON();
            var transJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
                new TangleNet::Transfer {
                    Address = new TangleNet::Address(sendTo),
                    Tag = new TangleNet::Tag("TANGLECHAIN"),
                    Message = transJson,
                    Timestamp = Timestamp.UnixSecondsTimestamp
                });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 10, 14);

            return result;

        }

        public static Block GetSpecificBlock(string address, string blockHash, Difficulty difficulty, bool verifyBlock) {

            //we dont precheck the blocks here because we do it later...
            var blockList = GetAllBlocksFromAddress(address, difficulty, null, false);

            foreach (Block block in blockList) {
                if (block.Hash.Equals(blockHash)) {

                    //if difficulty is null, then we dont care about the verification
                    if (verifyBlock && difficulty != null) {
                        if (Cryptography.VerifyBlock(block, difficulty))
                            return block;
                    } else
                        return block;
                }
            }

            return null;
        }

        public static Smartcontract GetSpecificSmartContract(string address, string smartHash) {

            //we dont precheck the blocks here because we do it later...
            var blockList = GetAllSmartcontractsFromAddresss(address);

            foreach (Smartcontract smart in blockList) {
                if (smart.Hash.Equals(smartHash)) {
                    return smart;
                }
            }

            return null;
        }

        public static List<Smartcontract> GetAllSmartcontractsFromBlock(Block block) {

            //all hashes of the transactions included in the block
            var hashList = block.SmartcontractHashes;

            //transactions are on this address
            string searchAddr = Utils.GetTransactionPoolAddress(block.Height, block.CoinName);

            //we now need to get the smartcontract from the hashes:
            var transList = GetAllSmartcontractsFromAddresss(searchAddr);

            var returnList = new List<Smartcontract>();

            if (hashList != null)
                for (int i = 0; i < transList.Count; i++) {
                    if (hashList.Contains(transList[i].Hash))
                        returnList.Add(transList[i]);
                }

            return returnList;
        }

        public static List<Smartcontract> GetAllSmartcontractsFromAddresss(string sendTo) {

            var smartList = new List<Smartcontract>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(sendTo)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {

                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Smartcontract newSmart = Smartcontract.FromJSON(json);

                if (newSmart != null)
                    smartList.Add(newSmart);
            }

            return smartList;

        }

        public static List<Block> GetAllBlocksFromAddress(string address, Difficulty difficulty, long? height, bool verifyTransactions) {

            //create objects
            var blockList = new List<Block>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Block newBlock = Block.FromJSON(json);

                //filter correct blocks
                if (height == null || height == newBlock.Height) {

                    //verify Block
                    if (verifyTransactions) {
                        if (Cryptography.VerifyBlock(newBlock, difficulty))
                            blockList.Add(newBlock);
                    } else {
                        if (difficulty != null)
                            Cryptography.VerifyHashAndNonceAgainstDifficulty(newBlock, difficulty);
                        else
                            blockList.Add(newBlock);
                    }
                }

            }

            return blockList;
        }

        public static List<Transaction> GetAllTransactionsFromAddress(string address) {

            //create objects
            var transList = new List<Transaction>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Transaction newTrans = Transaction.FromJSON(json);

                //verify block too
                if (newTrans != null)
                    transList.Add(newTrans);
            }

            return transList;
        }

        public static List<Transaction> GetAllTransactionsFromBlock(Block block) {

            //all hashes of the transactions included in the block
            var hashList = block.TransactionHashes;

            //transactions are on this address
            string searchAddr = Utils.GetTransactionPoolAddress(block.Height, block.CoinName);

            //we now need to get the transactions from the hashes:
            var transList = GetAllTransactionsFromAddress(searchAddr);

            var returnList = new List<Transaction>();

            if (hashList != null)
                for (int i = 0; i < transList.Count; i++) {
                    if (hashList.Contains(transList[i].Hash))
                        returnList.Add(transList[i]);
                }

            return returnList;
        }

        private static List<Block> GetBlocksFromWay(Way way) {

            List<Block> blockList = new List<Block>();

            Block block = GetSpecificBlock(way.Address, way.BlockHash, null, false);

            blockList.Add(block);

            while (true) {
                if (way.Before == null)
                    break;

                way = way.Before;
                block = GetSpecificBlock(way.Address, way.BlockHash, null, false);
                blockList.Add(block);
            }

            return blockList;
        }

        #endregion

        #region advanced functionality

        public static Block DownloadChain(string CoinName, string address, string hash, bool storeDB, bool includeSmartcontracts, Action<Block> Hook) {

            //difficulty doesnt matter. we need to assume address and hash are correct from calculations before
            Block block = GetSpecificBlock(address, hash, null, true);

            Hook?.Invoke(block);

            //we store first block! stupid hack
            if (storeDB) {
                DBManager.AddBlock(CoinName, block, true, includeSmartcontracts);
            }

            while (true) {

                //first we need to get the correct way
                Way way = FindCorrectWay(block.NextAddress, block.CoinName, block.Height + 1, CoinName);

                //we repeat the whole until we dont have a newer way
                if (way == null)
                    break;

                //we then download this whole chain
                if (storeDB) {
                    List<Block> list = GetBlocksFromWay(way);
                    DBManager.AddBlocks(CoinName, list, true, includeSmartcontracts);
                }

                //we just jump to the latest block
                block = GetSpecificBlock(way.Address, way.BlockHash, null, false);

                Hook?.Invoke(block);

            }

            return block;

        }

        private static List<Way> GrowWays(List<Way> ways, string coinName) {

            var wayList = new List<Way>();

            foreach (Way way in ways) {

                //first we get this specific block
                //we dont need to check for correct difficulty because we did it before
                Block specificBlock = GetSpecificBlock(way.Address, way.BlockHash, null, false);

                //compute now the next difficulty in case we go over the difficulty gap
                Difficulty nextDifficulty = DBManager.GetDifficulty(coinName, way);

                //we then download everything in the next address
                List<Block> allBlocks = GetAllBlocksFromAddress(specificBlock.NextAddress, nextDifficulty, specificBlock.Height + 1, true);

                foreach (Block block in allBlocks) {

                    Way temp = new Way(block.Hash, block.SendTo, block.Height, block.Time);
                    temp.AddOldWay(way);

                    wayList.Add(temp);
                }
            }

            return wayList;

        }

        private static Way FindCorrectWay(string address, string name, long startHeight, string coinName) {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //preparing
            Difficulty difficulty = DBManager.GetDifficulty(coinName, startHeight);

            //first we get all blocks
            var allBlocks = GetAllBlocksFromAddress(address, difficulty, startHeight, true);

            //we then generate a list of all ways from this block list
            var wayList = Utils.ConvertBlocklistToWays(allBlocks);

            //we then grow the list until we find the longest way
            while (wayList.Count > 1) {

                //we get the size before and if we add not a single more way, it means we only need to compare the sum of all lengths.
                //If the difference is 1 or less we only added a single way => longest chain for now
                int size = wayList.Count;

                int sumBefore = 0;
                wayList.ForEach(obj => { sumBefore += obj.Length; });

                //here happens the magic
                wayList = GrowWays(wayList, coinName);

                int sumAfter = 0;
                wayList.ForEach(obj => { sumAfter += obj.Length; });

                if (size == wayList.Count && sumAfter <= (sumBefore + 1))
                    break;
            }

            //growth stopped now because we only added a single block
            //we choosed now the longest way

            if (wayList.Count == 0)
                return null;

            return wayList.OrderByDescending(item => item.Length).First();

        }

        #endregion

        public static List<TangleNet::TransactionTrytes> UploadSmartContract(Smartcontract smart) {

            if (string.IsNullOrEmpty(smart.SendTo)) {
                throw new ArgumentException("Did you forget to final the smartcontract?");
            }

            //get sending address
            String sendTo = smart.SendTo;

            //prepare data
            string json = smart.ToJSON();
            var transJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
                new TangleNet::Transfer {
                    Address = new TangleNet::Address(sendTo),
                    Tag = new TangleNet::Tag("TANGLECHAIN"),
                    Message = transJson,
                    Timestamp = Timestamp.UnixSecondsTimestamp
                });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 10, 14);

            return result;

        }

    }
}
