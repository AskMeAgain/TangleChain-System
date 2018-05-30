using System;
using System.Collections.Generic;
using NUnit.Framework;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.CompleteExamples {

    [TestFixture]
    public class Example01 {

        //[Test]
        public void StartExample01() {

            //some vars
            int difficulty = 5;
            Settings.Default(true);

            //we first create a random coin name:
            string name = Utils.GenerateRandomString(10);

            //we then generate the genesis block and upload it, also we give us some money
            Block genesis = Core.CreateAndUploadGenesisBlock(name, "ME", 10000);

            Console.WriteLine("Genesis Address: " + genesis.SendTo);
            
            //test
            Assert.AreEqual(1, genesis.Height + 1);


            //we now mine another block ontop of it
            //first we create the block
            Block nextBlock = new Block(genesis.Height + 1, genesis.NextAddress, name);

            //we need to create some transactions first or we cant fill our block
            Utils.FillTransactionPool(5, name, genesis.Height);

            //get trans list
            var transList = Core.GetAllTransactionsFromAddress(Utils.GetTransactionPoolAddress(genesis.Height,name));

            //short test
            Assert.AreEqual(6, transList.Count);

            //assign transactions to block
            int numOfTransAdded = nextBlock.AddTransactions(transList, Settings.NumberOfTransactionsPerBlock);

            //generate hash and do proof of work
            nextBlock.GenerateHash();
            nextBlock.Nonce = Utils.ProofOfWork(nextBlock.Hash, difficulty);

            //checking if added trans are correct
            Assert.AreEqual(Settings.NumberOfTransactionsPerBlock, numOfTransAdded);

            //we then upload the block
            Core.UploadBlock(nextBlock);

            //we now download the chain
            Block latest = Core.DownloadChain(genesis.SendTo, genesis.Hash, difficulty, true);

            Assert.AreEqual(nextBlock, latest);

            //db stuff
            DataBase db = new DataBase(name);
            int balance = db.GetBalance("ME");

            Console.WriteLine("BALANCE OF ME " + balance);

        }
    }

}
