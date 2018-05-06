using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {

    [TestClass]
    public class TestCase_02_ConsensusCheck {

        //[TestMethod]
        public void Init_01() {

            //WWJMRIYSVNIIRNXMKZYRPBG9AIRCDWJQGISQIQDLSWXYNXVQEZWHHSVZYGFFATDHTFXXTXVWJEQUKUV9T 
            //is a 0 1 22 3 tree

            int difficulty = 5;

            //we first create genesis block
            Block genesis = Core.CreateAndUploadGenesisBlock();

            //we print the genesis address so we can use this somewhere else
            Console.WriteLine(genesis.SendTo);

            //we then attach a single block to it
            Block nextBlock = Core.MineBlock(genesis.Height + 1, genesis.NextAddress, difficulty);

            //we then split the network with two blocks
            Block nextBlock2 = Core.MineBlock(nextBlock.Height + 1, nextBlock.NextAddress, difficulty);
            Core.MineBlock(nextBlock.Height + 1, nextBlock.NextAddress, difficulty);

            //we then add another block on "theResult"
            Block final = Core.MineBlock(nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty);


        }

        //[TestMethod]
        public void Init_02() {

            //is a 0 1 2 tree
            //BCKXMNLXRYYILNXZASEERPBY99MBKQLPOJNXEHRCWSBJRDBYBWKAFXWYTAUQENMGHWHVVOGMJNUFTTRGK

            int difficulty = 5;

            //we first create genesis block
            Block genesis = Core.CreateAndUploadGenesisBlock();

            //we print the genesis address so we can use this somewhere else
            Console.WriteLine(genesis.SendTo);

            //we then attach a single block to it
            Block nextBlock = Core.MineBlock(genesis.Height + 1, genesis.NextAddress, difficulty);

            //we then split the network with two blocks
            Block nextBlock2 = Core.MineBlock(nextBlock.Height + 1, nextBlock.NextAddress, difficulty);
            


        }

        [TestMethod]
        public void FindWay_01() {

            int difficulty = 5;

            string address = "BCKXMNLXRYYILNXZASEERPBY99MBKQLPOJNXEHRCWSBJRDBYBWKAFXWYTAUQENMGHWHVVOGMJNUFTTRGK";
            //just a comparison
            List<Block> block = Core.GetAllBlocksFromAddress(address, difficulty);

            Assert.IsTrue(block.Count == 1);

            Way way = Core.FindCorrectWay(address);

            Assert.AreEqual(block[0].Hash, way.BlockHash);
            Assert.IsTrue(way.Length == 1);
        }

        [TestMethod]
        public void FindWay_02() {

            int difficulty = 5;

            string address = "WWJMRIYSVNIIRNXMKZYRPBG9AIRCDWJQGISQIQDLSWXYNXVQEZWHHSVZYGFFATDHTFXXTXVWJEQUKUV9T";

            List<Block> block_00 = Core.GetAllBlocksFromAddress(address, difficulty);

            Assert.IsTrue(block_00.Count == 1);

            List<Block> block_01 = Core.GetAllBlocksFromAddress(block_00[0].NextAddress, difficulty);

            Assert.IsTrue(block_01.Count == 1);

            List<Block> block_02 = Core.GetAllBlocksFromAddress(block_01[0].NextAddress, difficulty);

            Assert.IsTrue(block_02.Count == 2);

            List<Block> block_03 = Core.GetAllBlocksFromAddress(block_02[0].NextAddress, difficulty);

            Assert.IsTrue(block_03.Count == 1);


            Way way = Core.FindCorrectWay(block_01[0].NextAddress);
            Assert.IsTrue(way.Length == 2);
            way.Print();
        }

        [TestMethod]
        public void TestGrowth_01(){

            //this test tests the growth function with a simple line

            int difficulty = 5;
            string address = "WWJMRIYSVNIIRNXMKZYRPBG9AIRCDWJQGISQIQDLSWXYNXVQEZWHHSVZYGFFATDHTFXXTXVWJEQUKUV9T";

            List<Block> block = Core.GetAllBlocksFromAddress(address, difficulty);
            List<Block> nextBlock = Core.GetAllBlocksFromAddress(block[0].NextAddress, difficulty);
            Assert.IsTrue(block.Count == 1);

            List<Way> ways = Utils.ConvertBlocklistToWays(block);

            ways = Core.GrowWays(ways);

            ways[0].Print();

            Assert.IsTrue(ways.Count == 1);
            Assert.AreEqual(block[0].Hash,(ways[0].Before.BlockHash));
            Assert.AreEqual(nextBlock[0].Hash, (ways[0].BlockHash));

        }

        [TestMethod]
        public void TestGrowth_02() {

            //this test tests the growth function where we split on an address

            int difficulty = 5;
            string address = "WWJMRIYSVNIIRNXMKZYRPBG9AIRCDWJQGISQIQDLSWXYNXVQEZWHHSVZYGFFATDHTFXXTXVWJEQUKUV9T";

            List<Block> block = Core.GetAllBlocksFromAddress(address, difficulty);
            List<Block> nextBlock = Core.GetAllBlocksFromAddress(block[0].NextAddress, difficulty);

            Assert.IsTrue(block.Count == 1);

            List<Way> ways = Utils.ConvertBlocklistToWays(block);

            ways = Core.GrowWays(ways);

            Assert.IsTrue(ways.Count == 1);
            Assert.AreEqual(block[0].Hash, (ways[0].Before.BlockHash));
            Assert.AreEqual(nextBlock[0].Hash, (ways[0].BlockHash));


            ways = Core.GrowWays(ways);

            Assert.IsTrue(ways.Count == 2);

            ways = Core.GrowWays(ways);

            Assert.IsTrue(ways.Count == 2);

        }


    }
}
