using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {

    [TestClass]
    public class TestCase_01_SimpleChain {

        [TestMethod]
        public void SimpleChain() {

            //this testcase creates a genesis block, then mines 2 blocks ontop, and then outputs the genesis address and last block

            Block block = Core.CreateAndUploadGenesisBlock();

            Console.WriteLine("Genesis Address: " + block.SendTo);

            for (int i = 1; i <= 2; i++) {
                block = Core.MineBlock(block.Height + 1, block.NextAddress, 5);
            }

            block.Print();

        }
    }
}
