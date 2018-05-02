using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TangleChain;

namespace TangleChainTest.UnitTests {
    [TestClass]
    public class UnitTestUtil {

        [TestMethod]
        public void ConvertBlock() {

            //create dummy block first
            Block testBlock = new Block();           

            //convert to json
            string json = Utils.GetStringFromBlock(testBlock);

            //convert string to block
            Block newBlock = Utils.GetBlockFromJSON(json);

            Assert.AreEqual(testBlock, newBlock);
        }
    }
}
