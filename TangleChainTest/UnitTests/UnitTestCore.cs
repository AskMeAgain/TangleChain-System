using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Tangle.Net.Entity;
using TangleChain;

namespace TangleChainTest {

    [TestClass]
    public class UnitTestCore {

        [TestMethod]
        public void UploadBlock() {

            Block testBlock = new Block();

            var transList = Core.SendBlock(testBlock);

            Transaction trans = Transaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Utils.GetBlockFromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [TestMethod]
        public void DownloadBlocksFromAddress() {

            string address = "ZBVYKBQWSUMUDPPTLQFPSDHGSJYVPUOKREWSDHRAMYRGI9YALHGRZXJAKZIYZHGFPMYPMWIGUWBNVPVCB";

            List<Block> blocks = Core.GetAllBlocksFromAddress(address);

            Assert.IsTrue(blocks.Count > 0);

        }

        [TestMethod]
        public void DownloadSpecificBlockFromAddressAndBundleHash() {

            string address = "CBVYKBQWSUMUDPPTLQFPSDHGSJYVPUOKREWSDHRAMYRGI9YALHGRZXJAKZIYZHGFPMYPMWIGUWBNVPVCB";
            string blockHash = "BYIKMJDR9ZSWSATBRZWCMSPUYRILWHANTBJOMCFHXXPTFEBINULZPSN9FDZOK9Q9HNCJPBCXEJWNV99IK";

            Block newBlock = Core.GetSpecificBlock(address, blockHash);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }
    }
}
