using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tangle.Net.Entity;
using TangleChain;
using TangleChain.Classes;
using System.Linq;

namespace TangleChainTest {

    [TestClass]
    public class TestCore {

        //[TestMethod]
        public void SetupChain() {

            //JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH
            //is a 0 1 22 33 4 tree

            int difficulty = 5;

            //we first create genesis block
            Block genesis = Core.CreateAndUploadGenesisBlock(false);

            //we print the genesis address so we can use this somewhere else
            Console.WriteLine("Genesis Address" + genesis.SendTo);
            Console.WriteLine("Genesis Hash" + genesis.Hash);

            //we then attach a single block to it
            Block nextBlock = Core.MineBlock(genesis.Height + 1, genesis.NextAddress, difficulty, false);

            //we then split the network with two blocks
            Block nextBlock2 = Core.MineBlock(nextBlock.Height + 1, nextBlock.NextAddress, difficulty, false);
            Core.MineBlock(nextBlock.Height + 1, nextBlock.NextAddress, difficulty, false);

            //we then split the chain again
            Block nextBlock3 = Core.MineBlock(nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty, false);
            Core.MineBlock(nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty, false);

            //we mine a last block ontop
            Block last = Core.MineBlock(nextBlock3.Height + 1, nextBlock3.NextAddress, difficulty, false);

            Console.WriteLine("Last Hash: " + last.Hash);
            Console.WriteLine("Last Address: " + last.SendTo);
        }

        [TestMethod]
        public void UploadBlock() {

            Block testBlock = new Block();

            var transList = Core.UploadBlock(testBlock);

            Transaction trans = Transaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Block.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [TestMethod]
        public void DownloadSpecificBlock() {

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string blockHash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";

            Block newBlock = Core.GetSpecificBlock(address, blockHash, 5);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }

        [TestMethod]
        public void CreateBlock() {

            int height = 2;
            string sendTo = "lol";

            Block block = Core.CreateBlock(height, sendTo);

            Assert.AreEqual(height, block.Height);
            Assert.IsNotNull(block.Hash);
        }

        [TestMethod]
        public void CreateGenesisBlock() {

            int difficulty = 5;

            Block testBlock = new Block();

            Block genesis = Core.CreateAndUploadGenesisBlock(false);

            Block newBlock = Core.GetSpecificBlock(genesis.SendTo, genesis.Hash, difficulty);

            Assert.AreEqual(genesis, newBlock);
            Assert.AreNotEqual(genesis, testBlock);

            newBlock.Print();

        }

        [TestMethod]
        public void MineBlock() {

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            int height = 3;
            int difficulty = 5;

            //mine block and upload it
            Block block = Core.MineBlock(height, address, difficulty, true);
            block.GenerateHash();

            //download this block again
            Block newBlock = Core.GetSpecificBlock(address, block.Hash, difficulty);
            newBlock.GenerateHash();

            Assert.AreEqual(block, newBlock);
        }

        [TestMethod]
        public void TestDownloadChain() {

            //testing download function in a more sophisticated split 1 22 33 4  

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string hash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";
            int difficulty = 5;

            string expectedHash = "YNQ9PRSBFKKCMO9DZUPAQPWMYVDQFDYXNJBWISBOHZZHLPMCRS9KSJOIZAQPYLIOCJPLNORCASITPUJUV";

            Block latest = Core.DownloadChain(address, hash, difficulty, false);
            Assert.AreEqual(latest.Hash, expectedHash);

        }

        [TestMethod]
        public void OneClickMining() {

            int difficulty = 5;

            Block block = Core.CreateAndUploadGenesisBlock(false);
            Console.WriteLine("Genesis: " + block.SendTo);

            for (int i = 0; i < 2; i++) {
                block = Core.OneClickMining(block.SendTo, block.Hash, difficulty);
            }

            Console.WriteLine("Latest: " + block.SendTo);

        }

        [TestMethod]
        public void UploadDownloadOrder() {

            string sendTo = "OIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";

            Order order = Core.CreateOrder("FROM ME!!", sendTo, 0, 100);

            var transList = Core.UploadOrder(order);

            Transaction trans = Transaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Order newOrder = Order.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(order, newOrder);

            List<Order> listOrders = Core.GetAllOrdersFromAddress(sendTo);

            var findOrder = listOrders.Where(m => m.Identity.Hash.Equals(order.Identity.Hash));

            Assert.IsTrue(findOrder.Count<Order>() > 0);
        }

    }
}
