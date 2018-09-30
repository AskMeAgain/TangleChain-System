using NUnit.Framework;
using System;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestCore {

        private string GenesisAddress;
        private string GenesisHash;
        private string CoinName;
        private string DuplicateBlockHash;

        [OneTimeSetUp]
        public void InitSpecificChain() {

            var (genesisAddr, genesisHash, name, dupHash) = Initalizing.SetupCoreTest();

            GenesisAddress = genesisAddr;
            GenesisHash = genesisHash;
            CoinName = name;
            DuplicateBlockHash = dupHash;
        }

        [Test]
        public void BlockUpload() {

            string name = Utils.GenerateRandomString(81);
            Difficulty difficulty = new Difficulty();

            Block testBlock = new Block(3, name, "coolname");

            testBlock.Final();
            testBlock.GenerateProofOfWork(difficulty);

            IXISettings.Default(true);

            var transList = testBlock.Upload();

            var trans = TangleNetTransaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Block.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void BlockSpecificDownload() {

            IXISettings.Default(true);

            Block newBlock = Core.GetSpecificBlock(GenesisAddress, GenesisHash, null, true);

            Assert.AreEqual(GenesisHash, newBlock.Hash);

            Block dupBlock = Core.GetSpecificBlock(GenesisAddress, DuplicateBlockHash, null, false);

            Assert.AreEqual(DuplicateBlockHash, dupBlock.Hash);

        }

        [Test]
        public void BlockFailAtSpecific() {

            Block block = Core.GetSpecificBlock(Utils.GenerateRandomString(81), "lol", new Difficulty(), true);
            Assert.IsNull(block);
        }

        [Test]
        public void TransactionUploadDownload() {

            string sendTo = Utils.GenerateRandomString(81);

            Transaction trans = new Transaction("ME", 0, sendTo);
            trans.AddFee(30);
            trans.AddOutput(100, "YOU");
            trans.Final();

            var resultTrytes = Core.Upload(trans);
            var tnTrans = TangleNetTransaction.FromTrytes(resultTrytes[0]);

            Assert.IsTrue(tnTrans.IsTail);

            Transaction newTrans = Transaction.FromJSON(tnTrans.Fragment.ToUtf8String());

            Assert.AreEqual(trans, newTrans);

            var transList = Core.GetAllTransactionsFromAddress(sendTo);
            var findTrans = transList.Where(m => m.Equals(trans));

            Assert.AreEqual(findTrans.Count(), 1);

            trans.Hash = null;
            Assert.AreEqual("Transaction is not finalized. Did you forget to Final() the Transaction?", Assert.Throws<ArgumentException>(() => Core.Upload(trans)).Message);

        }

        [Test]
        public void BlockDownloadAllFromAddress() {

            IXISettings.Default(true);

            var blockList = Core.GetAllBlocksFromAddress(GenesisAddress, null, null, false);

            Assert.AreEqual(2, blockList.Count);
        }

        [Test]
        public void DownloadCompleteHistory() {

            IXISettings.Default(true);
            Block latest = Core.DownloadChain(CoinName, GenesisAddress, GenesisHash, true, true, (Block b) => { Console.WriteLine(b.Height); });

        }

    }
}
