using NUnit.Framework;
using System;
using System.Collections.Generic;
using TangleChain;
using TangleChain.Classes;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;

namespace TangleChainTest.UnitTests {

    [TestFixture]
    public class TestCore {

        [Test]
        public void UploadBlock() {

            Block testBlock = new Block();

            var transList = Core.UploadBlock(testBlock);

            var trans = TangleNetTransaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Block.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void DownloadSpecificBlock() {

            Settings.Default(false);

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string blockHash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";

            Block newBlock = Core.GetSpecificBlock(address, blockHash, 5);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }

        [Test]
        public void CreateBlock() {

            int height = 2;
            string sendTo = "lol";

            Block block = new Block(height, sendTo, "TESTLOL");

            Assert.AreEqual(height, block.Height);
            Assert.IsNotNull(block.Hash);
        }

        [Test]
        public void CreateGenesisBlock() {

            int difficulty = 5;
            Settings.Default(true);

            string name = Utils.GenerateRandomString(10);
            Block testBlock = new Block();

            Block genesis = Core.CreateAndUploadGenesisBlock(name, "ME", 100000);

            Block newBlock = Core.GetSpecificBlock(genesis.SendTo, genesis.Hash, difficulty);

            Assert.AreEqual(genesis, newBlock);
            Assert.AreNotEqual(genesis, testBlock);

            newBlock.Print();

        }

        [Test]
        public void FailAtSpecificBlock() {

            Block block = Core.GetSpecificBlock(Utils.GenerateRandomAddress(), "lol", 5);
            Assert.IsNull(block);
        }


        [Test]
        public void MineBlock() {

            Settings.Default(true);
            string address = Utils.GenerateRandomAddress();
            int height = 3;
            int difficulty = 5;
            string name = Utils.GenerateRandomString(10);

            //mine block and upload it
            Block block = Core.MineBlock(name, height, address, difficulty, true);
            block.GenerateHash();

            //download this block again
            Block newBlock = Core.GetSpecificBlock(address, block.Hash, difficulty);

            Assert.AreEqual(block, newBlock);
        }

        [Test]
        public void OneClickMining() {

            int difficulty = 5;
            string name = Utils.GenerateRandomString(10);
            Settings.Default(true);


            Block genesis = Core.CreateAndUploadGenesisBlock(name, "ME", 100000);
            Block block = genesis;
            Console.WriteLine("Genesis: " + block.SendTo);

            for (int i = 0; i < 2; i++) 
                block = Core.OneClickMining(block.SendTo, block.Hash, difficulty);
            

            Console.WriteLine("Latest: " + block.SendTo);

            Block latest = Core.DownloadChain(genesis.SendTo, genesis.Hash, difficulty, false);

            Assert.AreEqual(latest, block);

        }

        [Test]
        public void UploadDownloadTransaction() {

            string sendTo = Utils.GenerateRandomAddress();

            Transaction trans = Transaction.CreateTransaction("ME", sendTo, 0, 100);

            var resultTrytes = Core.UploadTransaction(trans);
            var tnTrans = TangleNetTransaction.FromTrytes(resultTrytes[0]);

            Assert.IsTrue(tnTrans.IsTail);

            Transaction newTrans = Transaction.FromJSON(tnTrans.Fragment.ToUtf8String());

            Assert.AreEqual(trans, newTrans);

            var transList = Core.GetAllTransactionsFromAddress(sendTo);
            var findTrans = transList.Where(m => m.Identity.Equals(trans.Identity));

            Assert.AreEqual(findTrans.Count(), 1);
        }

        [Test]
        public void FillTransactionPool() {

            int n = 3;
            string coinName = Utils.GenerateRandomString(10);
            string addr = Utils.GetTransactionPoolAddress(3, coinName);
            Settings.Default(true);

            for (int i = 0; i < n; i++) {
                //we create now the transactions
                Transaction trans = Transaction.CreateTransaction("ME", addr, 1, 100);

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            var transList = Core.GetAllTransactionsFromAddress(addr);

            Assert.AreEqual(transList.Count, n);

            Console.WriteLine(addr);
        }

        [Test]
        public void DownloadBlocksFromAddress() {

            Settings.Default(false);

            string addr = "ATHEGAXYAKPVRJHDSNAVETVUWWMBYCQHV9UDQOP99FDPSZZIRASRZAXPAMBSEMNLCTDROEWVSAHMAHSXH";
            int difficulty = 5;

            var blockList = Core.GetAllBlocksFromAddress(addr, difficulty, 2);

            Assert.AreEqual(2,blockList.Count);
        }

    }
}
