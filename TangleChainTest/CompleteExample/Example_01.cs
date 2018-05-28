using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TangleChain;
using TangleChain.Classes;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;

namespace TangleChainTest.CompleteExample {

    [TestClass]
    public class Example_01 {

        [TestMethod]
        public void Start() {

            //some vars
            int difficulty = 5;

            //we first create a random coin name:
            string name = Utils.GenerateRandomString(10);

            //we then generate the genesis block and upload it
            Block genesis = Core.CreateAndUploadGenesisBlock(name, "ME", 10000);

            Console.WriteLine("Genesis Address: " + genesis.SendTo);

            //we now mine another block ontop of it
            //first we create the block

            Assert.AreEqual(1, genesis.Height + 1);

            Block nextBlock = Block.CreateBlock(genesis.Height + 1, genesis.NextAddress, name);

            //we need to create some transactions
            FillTransactionPool(5, name, genesis.Height);

            //get trans list
            List<Transaction> transList = Core.GetAllTransactionsFromAddress(Utils.GetTransactionPoolAddress(genesis.Height,name));

            //short test
            Assert.AreEqual(6, transList.Count);

            //assign transactions to block
            int numOfTransAdded = nextBlock.AddTransactions(transList, Settings.NumberOfTransactionsPerBlock);

            //generate hash and do proof of work
            nextBlock.Owner = "tesst";
            nextBlock.GenerateHash();
            nextBlock.Nonce = Utils.ProofOfWork(nextBlock.Hash, difficulty);

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

        public void FillTransactionPool(int num, string coinName, int height) {

            string addr = Utils.GetTransactionPoolAddress(height, coinName);

            Random rnd = new Random();

            for (int i = 0; i < num; i++) {

                //we create now the transactions
                Transaction trans = Transaction.CreateTransaction("ME", addr, 1, rnd.Next(100,200));

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            Console.WriteLine("Transactionpool Address: " + addr);
        }


    }

}
