using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace TangleChainIXITest {


    public static class Initalizing {

        public static (string addr, string hash, string coinName, string dupHash) SetupCoreTest() {

            IXISettings.Default(true);

            //vars
            string coinName = Utils.GenerateRandomString(10);

            DataBase Db = new DataBase(coinName);
            Db.ChainSettings = new ChainSettings(100, -1, 0, 4, 100, 10, 10);

            Console.WriteLine("CoinName: " + coinName);

            Difficulty difficulty = new Difficulty(7);

            //we now fill the transpool
            string transactionPool = FillTransactionPool("Me", "YOU", 3, coinName, 1, Db);
            var transList = Core.GetAllTransactionsFromAddress(transactionPool);

            //we then generate a genesis block
            String sendto = Cryptography.HashCurl(coinName + "_GENESIS", 81);
            Block genesisBlock = new Block(0, sendto, coinName);

            //add some money
            Transaction genesisTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genesisTrans.SetGenesisInformation(Db.ChainSettings);
            genesisTrans.AddOutput(10000, "ME");
            genesisTrans.Final();

            Core.UploadTransaction(genesisTrans);

            //we then upload the block
            genesisBlock.AddTransactions(genesisTrans);
            genesisBlock.Final();
            genesisBlock.GenerateProofOfWork(difficulty);
            Core.UploadBlock(genesisBlock);

            Console.WriteLine("\nAddress: " + genesisBlock.SendTo);

            //to mine a block on top we first create a block
            Block nextBlock = new Block(1, genesisBlock.NextAddress, coinName);

            //we then fill this block with transactions
            nextBlock.AddTransactions(transList.Take(Db.ChainSettings.TransactionsPerBlock).ToList());

            //upload block
            nextBlock.Final();
            nextBlock.GenerateProofOfWork(new Difficulty(7));
            Core.UploadBlock(nextBlock);

            //we also need to add another block to genesis addr
            Block dupBlock = new Block(0, sendto, "DIFFERENT NAME");

            dupBlock.Final();
            dupBlock.GenerateProofOfWork(new Difficulty(7));

            Core.UploadBlock(dupBlock);

            return (genesisBlock.SendTo, genesisBlock.Hash, coinName, dupBlock.Hash);


        }

        private static string FillTransactionPool(string owner, string receiver, int numOfTransactions, string coinName, long height, DataBase Db) {

            int interval = Db.ChainSettings.TransactionPoolInterval;
            string num = height / interval * interval + "";
            string addr = Cryptography.HashCurl(num + "_" + coinName.ToLower(), 81);

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

        public static string SetupDatabaseTest() {

            IXISettings.Default(true);
            string coinName = Utils.GenerateRandomString(10);

            DataBase Db = new DataBase(coinName);

            //settings
            Db.ChainSettings = new ChainSettings(100, -1, 0, 4, 100, 10, 10);

            //compute difficulty!
            Difficulty nextBlockDifficulty = new Difficulty(7);

            //create block first
            string transPool = Utils.GetTransactionPoolAddress(0, coinName);
            Block genesisBlock = new Block(0, Utils.GenerateRandomString(81), coinName);

            Transaction trans = new Transaction("ME", -1, transPool);
            trans.AddFee(0);
            trans.SetGenesisInformation(Db.ChainSettings);
            trans.Final();

            Core.UploadTransaction(trans);

            genesisBlock.AddTransactions(trans);

            genesisBlock.Final();
            genesisBlock.GenerateProofOfWork(nextBlockDifficulty);

            Db.AddBlock(genesisBlock, true);

            return coinName;
        }
    }

}
