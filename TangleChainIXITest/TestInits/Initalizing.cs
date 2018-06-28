using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace TangleChainIXITest {


    public static class Initalizing {

        public static (string addr, string hash) SetupCoreTest() {

            Settings.Default(true);

            //vars
            string name = Utils.GenerateRandomString(10);
            string transactionPool = Utils.FillTransactionPool(3, name, 1);
            int difficulty = 5;

            Console.WriteLine("CoinName: " + name);

            //we fill the transaction pool with some transactions        
            var transList = Core.GetAllTransactionsFromAddress(transactionPool);

            //we then generate a genesis block
            String sendto = Utils.HashCurl(name + "_GENESIS", 81);
            Block genesisBlock = new Block(0, sendto, name);

            //add some money
            Transaction genesisTrans = new Transaction("ME",-1, transactionPool);
            genesisTrans.SetGenesisInformation(100, -1, 0, 4,100,10);
            genesisTrans.AddOutput(10000, "ME");
            genesisTrans.Final();

            Core.UploadTransaction(genesisTrans);

            //we then upload the block
            genesisBlock.AddTransactions(genesisTrans);
            genesisBlock.Final();
            genesisBlock.GenerateProofOfWork(difficulty);
            Core.UploadBlock(genesisBlock);

            Console.WriteLine("\nAddress: " + genesisBlock.SendTo + "\nTransactionPool: " + transactionPool);

            //to mine a block on top we first create a block
            Block nextBlock = new Block(1, genesisBlock.NextAddress, name);

            //we then fill this block with transactions
            nextBlock.AddTransactions(transList.Take(Settings.GetChainSettings(name).TransactionsPerBlock).ToList());

            //upload block
            nextBlock.Final();
            nextBlock.GenerateProofOfWork(difficulty);
            Core.UploadBlock(nextBlock);

            //we also need to add another block to genesis addr
            Block dupBlock = new Block(0, sendto, "DIFFERENT NAME");

            dupBlock.Final();
            dupBlock.GenerateProofOfWork(difficulty);

            Core.UploadBlock(dupBlock);

            return (genesisBlock.SendTo, genesisBlock.Hash);


        }

        public static string SetupDatabaseTest() {

            Settings.Default(true);

            //create block first
            string name = Utils.GenerateRandomString(10);
            string transPool = Utils.GetTransactionPoolAddress(0, name);

            Block genesisBlock = new Block(0, Utils.GenerateRandomString(81), name);

            Transaction trans = new Transaction("ME", -1, transPool);
            trans.AddFee(0);
            trans.SetGenesisInformation(100, 0, 0, 10, 100, 10);
            trans.Final();

            Core.UploadTransaction(trans);

            genesisBlock.AddTransactions(trans);

            genesisBlock.Final();
            genesisBlock.GenerateProofOfWork(5);

            DataBase Db = new DataBase(name);
            Db.AddBlock(genesisBlock, true);

            return name;
        }
    }

}
