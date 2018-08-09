using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace TangleChainIXITest {


    public static class Initalizing {

        public static (string addr, string hash,string coinName) SetupCoreTest() {

            IXISettings.Default(true);

            //vars
            string coinName = Utils.GenerateRandomString(10);

            ChainSettings cSett = new ChainSettings(100, -1, 0, 4, 100, 10, 10);
            IXISettings.AddChainSettings(coinName,cSett);

            Console.WriteLine("CoinName: " + coinName);

            Difficulty difficulty = Core.GetDifficultyViaHeight(coinName,0);



            //we now fill the transpool
            string transactionPool = Utils.FillTransactionPool("Me","YOU",3, coinName, 1);
            var transList = Core.GetAllTransactionsFromAddress(transactionPool);

            //we then generate a genesis block
            String sendto = Utils.HashCurl(coinName + "_GENESIS", 81);
            Block genesisBlock = new Block(0, sendto, coinName);

            //add some money
            Transaction genesisTrans = new Transaction("ME",-1, Utils.GetTransactionPoolAddress(0,coinName));
            genesisTrans.SetGenesisInformation(cSett);
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
            nextBlock.AddTransactions(transList.Take(IXISettings.GetChainSettings(coinName).TransactionsPerBlock).ToList());

            //compute difficulty!
            Difficulty nextBlockDifficulty = Core.GetDifficultyViaHeight(coinName, 1);

            //upload block
            nextBlock.Final();
            nextBlock.GenerateProofOfWork(nextBlockDifficulty);
            Core.UploadBlock(nextBlock);

            //we also need to add another block to genesis addr
            Block dupBlock = new Block(0, sendto, "DIFFERENT NAME");

            dupBlock.Final();
            dupBlock.GenerateProofOfWork(nextBlockDifficulty);

            Core.UploadBlock(dupBlock);

            return (genesisBlock.SendTo, genesisBlock.Hash, coinName);


        }

        public static string SetupDatabaseTest() {

            IXISettings.Default(true);
            string coinName = Utils.GenerateRandomString(10);       

            //settings
            ChainSettings cSett = new ChainSettings(100, -1, 0, 4, 100, 10,10);
            IXISettings.AddChainSettings(coinName,cSett);

            //compute difficulty!
            Difficulty nextBlockDifficulty = Core.GetDifficultyViaHeight(coinName, 0);

            //create block first
            string transPool = Utils.GetTransactionPoolAddress(0, coinName);
            Block genesisBlock = new Block(0, Utils.GenerateRandomString(81), coinName);

            Transaction trans = new Transaction("ME", -1, transPool);
            trans.AddFee(0);
            trans.SetGenesisInformation(cSett);
            trans.Final();

            Core.UploadTransaction(trans);

            genesisBlock.AddTransactions(trans);

            genesisBlock.Final();
            genesisBlock.GenerateProofOfWork(nextBlockDifficulty);

            DataBase Db = new DataBase(coinName);
            Db.AddBlock(genesisBlock, true);

            return coinName;
        }
    }

}
