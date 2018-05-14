﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChain.Classes;

namespace TangleChain {

    public static class Core {

        public static List<TransactionTrytes> UploadBlock(Block block) {

            //get sending address
            String sendTo = block.SendTo;

            //prepare data
            string json = block.ToJSON();
            TryteString blockJson = TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient("http://node02.iotatoken.nl:14265"), new PoWService(new CpuPearlDiver()));

            Bundle bundle = new Bundle();
            bundle.AddTransfer(
              new Transfer {
                  Address = new Address(sendTo),
                  Tag = Tag.Empty,
                  Message = blockJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 27, 14);

            return result;
        }

        public static Block GetSpecificBlock(string address, string blockHash, int difficulty) {

            var blocks = GetAllBlocksFromAddress(address, difficulty);

            foreach (Block block in blocks) {
                if (block.Hash.Equals(blockHash))
                    return block;
            }

            return null;

        }

        public static List<Block> GetAllBlocksFromAddress(string address, int difficulty) {

            //create objects
            List<Block> blocks = new List<Block>();
            var repository = new RestIotaRepository(new RestClient("http://iotanode.party:14265"));
            List<Address> addressList = new List<Address>() {
                new Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Block newBlock = Block.FromJSON(json);
                newBlock.GenerateHash();

                //verify block too
                if (Utils.VerifyBlock(newBlock, difficulty))
                    blocks.Add(newBlock);
            }

            return blocks;
        }

        public static Block CreateAndUploadGenesisBlock(bool createDB) {

            int difficulty = 5;
            string sendTo = Utils.Hash_Curl(Timestamp.UnixSecondsTimestamp.ToString(), 243);

            //first we need to create the block
            Block genesis = Block.CreateBlock(0, sendTo);

            //generate hash from the block
            genesis.GenerateHash();

            //then we find the correct nonce
            genesis.Nonce = Utils.ProofOfWork(genesis.Hash, difficulty);

            //we check the nonce first in case of wrong computation
            if (!Utils.VerifyHash(genesis.Hash, genesis.Nonce, difficulty))
                throw new ArgumentException("Nonce didnt got correctly computed");


            //then we upload the block
            UploadBlock(genesis);

            if (createDB) {
                DataBase db = new DataBase(genesis.CoinName);
                db.AddBlock(genesis);
            }

            return genesis;
        }

        public static Block MineBlock(int height, string NextAddress, int difficulty, bool storeDB) {

            //this function straight mines a block to a specific address with a difficulty.

            //create block first
            Block block = Block.CreateBlock(height, NextAddress);

            //generate hash
            block.GenerateHash();

            //do proof of work
            block.Nonce = Utils.ProofOfWork(block.Hash, difficulty);

            //send block
            UploadBlock(block);

            if (storeDB) {
                DataBase db = new DataBase(block.CoinName);
                db.AddBlock(block);
            }

            return block;
        }

        public static Way FindCorrectWay(string address) {

            //this function finds the "longest" chain of blocks when given an address

            int difficulty = 5;

            //general container
            List<Way> ways = new List<Way>();

            //first we get all blocks
            List<Block> allBlocks = GetAllBlocksFromAddress(address, difficulty);

            //we then generate a list of all ways from this block list
            ways = Utils.ConvertBlocklistToWays(allBlocks);

            //we then grow the list until we found the longst way
            while (ways.Count > 1) {

                //we get the size before and if we add not a single more way, it means we only need to compare the sum of all lengths.
                //If the difference is 1 or less we only added a single way => longest chain for now
                int size = ways.Count;

                int sumBefore = 0;
                ways.ForEach(obj => { sumBefore += obj.Length; });

                ways = GrowWays(ways);

                int sumAfter = 0;
                ways.ForEach(obj => { sumAfter += obj.Length; });


                if (size == ways.Count && sumAfter <= (sumBefore + 1))
                    break;
            }

            //growth stopped now because we only added a single block
            //we choose now the longest way

            if (ways == null || ways.Count == 0)
                return null;

            Way rightWay = ways.OrderByDescending(item => item.Length).First();

            return rightWay;

        }

        public static List<Way> GrowWays(List<Way> ways) {

            int difficulty = 5;
            List<Way> list = new List<Way>();

            foreach (Way way in ways) {

                //first we get this specific block
                Block specificBlock = GetSpecificBlock(way.Address, way.BlockHash, difficulty);

                //we then download everything in the next address
                List<Block> allBlocks = GetAllBlocksFromAddress(specificBlock.NextAddress, difficulty);

                foreach (Block block in allBlocks) {

                    Way temp = new Way(block.Hash, block.SendTo, block.Height);
                    temp.AddOldWay(way);

                    list.Add(temp);
                }
            }

            return list;

        }

        public static Block DownloadChain(string address, string hash, int difficulty, bool storeDB) {

            Block block = Core.GetSpecificBlock(address, hash, difficulty);

            while (true) {

                //first we need to get the correct way
                Way way = FindCorrectWay(block.NextAddress);

                //we repeat the whole thing until the way is empty
                if (way == null)
                    break;


                //we then download this whole chain
                if (storeDB)
                    DownloadBlocksFromWay(way, difficulty);

                //we just jump to the latest block
                block = GetSpecificBlock(way.Address, way.BlockHash, difficulty);

                if (block == null)
                    break;

            }

            return block;

        }

        public static void DownloadBlocksFromWay(Way way, int difficulty) {

            Block block = GetSpecificBlock(way.Address, way.BlockHash, difficulty);
            DataBase db = new DataBase(block.CoinName);

            while (way.Before != null) {
                block = GetSpecificBlock(way.Address, way.BlockHash, difficulty);
                db.AddBlock(block);
                way = way.Before;
            }
        }

        public static Block OneClickMining(string genesis, string hash, int difficulty) {

            //first we need to get to the latest block
            Block latest = DownloadChain(genesis, hash, difficulty, true);

            //we then mine a block ontop of this block
            Block newBlock = MineBlock(latest.Height + 1, latest.NextAddress, difficulty, true);

            return newBlock;
        }

        public static List<TransactionTrytes> UploadOrder(Order order) {

            //get sending address
            String sendTo = order.Identity.SendTo;

            //prepare data
            string json = order.ToJSON();
            TryteString orderJson = TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient("http://node05.iotatoken.nl:16265"), new PoWService(new CpuPearlDiver()));

            Bundle bundle = new Bundle();
            bundle.AddTransfer(
              new Transfer {
                  Address = new Address(sendTo),
                  Tag = Tag.Empty,
                  Message = orderJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 27, 14);

            return result;

        }

        public static List<Order> GetAllOrdersFromAddress(string address) {

            //create objects
            List<Order> orders = new List<Order>();
            var repository = new RestIotaRepository(new RestClient("http://iotanode.party:14265"));
            List<Address> addressList = new List<Address>() {
                new Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Order newOrder = Order.FromJSON(json);

                //verify block too
                if (newOrder != null)
                    orders.Add(newOrder);
            }

            return orders;
        }
    }
}
