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

        //[Test]
        public void SetupChain() {

            //JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH
            //is a 0 1 22 33 4 tree

            int difficulty = 5;
            string coinName = "asd";
            //we first create genesis block
            Block genesis = Core.CreateAndUploadGenesisBlock(coinName, "ME", 100000);

            //we print the genesis address so we can use this somewhere else
            Console.WriteLine("Genesis Address" + genesis.SendTo);
            Console.WriteLine("Genesis Hash" + genesis.Hash);

            //we then attach a single block to it
            Block nextBlock = Core.MineBlock(coinName, genesis.Height + 1, genesis.NextAddress, difficulty, false);

            //we then split the network with two blocks
            Block nextBlock2 = Core.MineBlock(coinName, nextBlock.Height + 1, nextBlock.NextAddress, difficulty, false);
            Core.MineBlock(coinName, nextBlock.Height + 1, nextBlock.NextAddress, difficulty, false);

            //we then split the chain again
            Block nextBlock3 = Core.MineBlock(coinName, nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty, false);
            Core.MineBlock(coinName, nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty, false);

            //we mine a last block ontop
            Block last = Core.MineBlock(coinName, nextBlock3.Height + 1, nextBlock3.NextAddress, difficulty, false);

            Console.WriteLine("Last Hash: " + last.Hash);
            Console.WriteLine("Last Address: " + last.SendTo);
        }

        [Test]
        public void UploadBlock() {

            Block testBlock = new Block();

            var transList = Core.UploadBlock(testBlock);

            TangleNetTransaction trans = TangleNetTransaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Block.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void DownloadSpecificBlock() {

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string blockHash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";

            Block newBlock = Core.GetSpecificBlock(address, blockHash, 5);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }

        [Test]
        public void CreateBlock() {

            int height = 2;
            string sendTo = "lol";

            Block block = Block.CreateBlock(height, sendTo, "TESTLOL");

            Assert.AreEqual(height, block.Height);
            Assert.IsNotNull(block.Hash);
        }

        [Test]
        public void CreateGenesisBlock() {

            int difficulty = 5;

            string name = Utils.GenerateRandomString(10);
            Block testBlock = new Block();

            Block genesis = Core.CreateAndUploadGenesisBlock(name, "ME", 100000);

            Block newBlock = Core.GetSpecificBlock(genesis.SendTo, genesis.Hash, difficulty);

            Assert.AreEqual(genesis, newBlock);
            Assert.AreNotEqual(genesis, testBlock);

            newBlock.Print();

        }

        [Test]
        public void MineBlock() {

            string address = Utils.GenerateRandomAddress();
            int height = 3;
            int difficulty = 5;

            //mine block and upload it
            Block block = Core.MineBlock("TESTASDASDASD", height, address, difficulty, true);
            block.GenerateHash();

            //download this block again
            Block newBlock = Core.GetSpecificBlock(address, block.Hash, difficulty);
            newBlock.GenerateHash();

            Assert.AreEqual(block, newBlock);
        }

        [Test]
        public void TestDownloadChain() {

            //testing download function in a split 1 22 33 4  
            string address = "DZFESBAHRNXVHJJVJTXA9BIQOFQOTSZMFTFPIYQPLTRQPHVVXZPEFMILQLRZEBPHDOMMLFBTGXNDSDAKJ";
            string hash = "IHEDGOTHDZISLLFZJ9ZDU9QWGYERFWXOUQUY9JKHQYMWMPIEQF9ZMAJWAV9EUUFCJFUMOXXVSGZKKUIUM";
            int difficulty = 5;

            string expectedHash = "CHYWPLCG9WI9NULCHJT9QRVEWJRGFBZHA9PKJVQVZE9AP9OSWDYMHZWBDJ9BUDXHRJTPAWLUTMRLMVPXR";

            Block latest = Core.DownloadChain(address, hash, difficulty, false);

            if (latest.Hash.Equals(hash))
                throw new ArgumentException("DownloadChain LATEST IS EQUAL TO THE FIRST");

            Assert.AreEqual(expectedHash,latest.Hash);

        }

        [Test]
        public void OneClickMining() {

            int difficulty = 5;
            string name = Utils.GenerateRandomString(10);

            Block genesis = Core.CreateAndUploadGenesisBlock(name, "ME", 100000);
            Block block = genesis;
            Console.WriteLine("Genesis: " + block.SendTo);

            for (int i = 0; i < 2; i++) {
                block = Core.OneClickMining(block.SendTo, block.Hash, difficulty);
            }

            Console.WriteLine("Latest: " + block.SendTo);

            Block latest = Core.DownloadChain(genesis.SendTo, genesis.Hash, difficulty, false);

            Assert.AreEqual(latest, block);

        }

        [Test]
        public void UploadDownloadTransaction() {

            string sendTo = Utils.GenerateRandomAddress();

            Transaction trans = Transaction.CreateTransaction("ME", sendTo, 0, 100);

            var resultTrytes = Core.UploadTransaction(trans);

            TangleNetTransaction tnTrans = TangleNetTransaction.FromTrytes(resultTrytes[0]);

            Assert.IsTrue(tnTrans.IsTail);

            Transaction newTrans = Transaction.FromJSON(tnTrans.Fragment.ToUtf8String());

            Assert.AreEqual(trans, newTrans);

            List<Transaction> transList = Core.GetAllTransactionsFromAddress(sendTo);

            var findTrans = transList.Where(m => m.Identity.Equals(trans.Identity));

            Assert.AreEqual(findTrans.Count(), 1);
        }

        [Test]
        public void FillTransactionPool() {

            int n = 4;
            string coinName = Utils.GenerateRandomString(10);
            string addr = Utils.GetTransactionPoolAddress(3, coinName);

            for (int i = 0; i < n; i++) {

                //we create now the transactions
                Transaction trans = Transaction.CreateTransaction("ME", addr, 1, 100);

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            List<Transaction> list = Core.GetAllTransactionsFromAddress(addr);

            Assert.AreEqual(list.Count, n);

            Console.WriteLine(addr);
        }

        [Test]
        public void DownloadBlocksFromAddress() {

            string addr = "ATHEGAXYAKPVRJHDSNAVETVUWWMBYCQHV9UDQOP99FDPSZZIRASRZAXPAMBSEMNLCTDROEWVSAHMAHSXH";
            int difficulty = 5;

            List<Block> blocks = Core.GetAllBlocksFromAddress(addr, difficulty, 2);

            Assert.AreEqual(2,blocks.Count);
        }

    }
}
