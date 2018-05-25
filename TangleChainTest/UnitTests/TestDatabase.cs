
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {

    [TestClass]
    public class TestDatabase {

        [TestMethod]
        public void TestInit() {

            DataBase db = new DataBase("Test");

            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsWorking());

        }

        [TestMethod]
        public void TestAddAndGetBlock() {

            Block test = new Block();
            DataBase db = new DataBase("Test");

            db.AddBlock(test,false);

            Block compare = db.GetBlock(test.Height);

            Assert.AreEqual(test, compare);

        }

        [TestMethod]
        public void DownloadAndStorage_Block() {

            //testing download function in a more sophisticated split 1 22 33 4  

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string hash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";
            int difficulty = 5;

            string expectedHash = "YNQ9PRSBFKKCMO9DZUPAQPWMYVDQFDYXNJBWISBOHZZHLPMCRS9KSJOIZAQPYLIOCJPLNORCASITPUJUV";

            Block latest = Core.DownloadChain(address, hash, difficulty, true);
            Assert.AreEqual(latest.Hash, expectedHash);

            DataBase Db = new DataBase(latest.CoinName);

            Block storedBlock = Db.GetBlock(latest.Height);

            Assert.AreEqual(latest, storedBlock);

        }

        [TestMethod]
        public void UploadDownloadAndStorage_Transaction() {

            string sendTo = Utils.GenerateRandomAddress();

            Transaction uploadTrans = Transaction.CreateTransaction("ME", sendTo, 0, 1000);

            Core.UploadTransaction(uploadTrans);

            List<Transaction> transList = Core.GetAllTransactionsFromAddress(sendTo);

            Transaction trans = transList[0];

            DataBase db = new DataBase("TestUploadDownload");

            trans.Print();

            db.AddTransactionToDatabase(transList);

            Transaction compare = db.GetTransaction(trans.Identity.SendTo, trans.Identity.Hash);

            Assert.AreEqual(trans, compare);

        }

        [TestMethod]
        public void GetBalance() {

            //string sendTo = Utils.GenerateRandomAddress();

            //Transaction uploadTrans = Transaction.CreateTransaction("ME", sendTo, 0, 1000);
            //uploadTrans.AddOutput(100, "LOL");
            //uploadTrans.AddOutput(200, "LOL");

            //uploadTrans.Sign("private key!");

            //Core.UploadTransaction(uploadTrans);

            //List<Transaction> transList = Core.GetAllTransactionsFromAddress(sendTo);

            DataBase db = new DataBase("TestGetBalance");

            //db.AddTransactionToDatabase(transList);

            int balance = db.GetBalance("LOL");

            Assert.AreEqual(300, balance);

        }

        [TestMethod]
        public void GetBalanceOfASDDChain() {

            string hash = "BSGNJNCIGFBOL99ZHUSYSRWJHRRCDFTNPQBHWJUCOLRBKTR9OLDYXZCKZGKABXNDRJJNMQPZDNHDRCJRB";
            string addr = "EJGLSCIILECBMSM9GAYTTVHKS9Y9SUATIAFTKUOIDAEWOTIOHINEWJQUIQCNW9MKUETULLDDOOMUUAFLN";
            int difficulty = 5;

            Block latest = Core.DownloadChain(addr, hash, difficulty, true);

            DataBase db = new DataBase(latest.CoinName);

            int balance = db.GetBalance("ME");

            //tests
            Assert.AreEqual(hash, latest.Hash);
            Assert.AreEqual(100000, balance);

        }

        [TestMethod]
        public void AddBlockOnTopOfASDDChain() {

            //first we download whole chain
            string addr = "EJGLSCIILECBMSM9GAYTTVHKS9Y9SUATIAFTKUOIDAEWOTIOHINEWJQUIQCNW9MKUETULLDDOOMUUAFLN";
            string hash = "BSGNJNCIGFBOL99ZHUSYSRWJHRRCDFTNPQBHWJUCOLRBKTR9OLDYXZCKZGKABXNDRJJNMQPZDNHDRCJRB";
            int difficulty = 5;

            Block latestBlock = Core.DownloadChain(addr, hash, difficulty, true);

            //we "get" some transactions from the transactionpool
            List<Transaction> transList = Core.GetAllTransactionsFromAddress(Utils.GetTransactionPoolAddress(latestBlock.Height + 1, latestBlock.CoinName));            

            //we generate new block
            Block newBlock = Block.CreateBlock(latestBlock.Height + 1, latestBlock.NextAddress, latestBlock.CoinName);

            //add transactions
            newBlock.TransactionHashes.Add(transList[0].Identity.Hash);


            //generate hash
            newBlock.GenerateHash();

            //we upload the block now
            Core.UploadBlock(newBlock);

            //we store the block
            DataBase db = new DataBase(newBlock.CoinName);
            db.AddBlock(newBlock, true);

            //we calculate Balance now:
            int balance = db.GetBalance("ME");
            Console.WriteLine("Balance of ME is " + balance);


        }

    }
}
