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
        public void ConvertBlocklistToWays() {

            IXISettings.Default(true);

            var blockList = Core.GetAllBlocksFromAddress(GenesisAddress, null, null);
            var wayList = Utils.ConvertBlocklistToWays(blockList);

            Assert.AreEqual(blockList.Count, wayList.Count);
            Assert.AreEqual(blockList[0].Hash, wayList[0].BlockHash);

        }


        [Test]
        public void BlockUpload() {

            string name = Utils.GenerateRandomString(81);
            Difficulty difficulty = new Difficulty();

            Block testBlock = new Block(3, name, "coolname");

            testBlock.Final();
            testBlock.GenerateProofOfWork(difficulty);

            IXISettings.Default(true);

            var transList = Core.UploadBlock(testBlock);

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

            Assert.AreEqual(DuplicateBlockHash,dupBlock.Hash);

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

            var resultTrytes = Core.UploadTransaction(trans);
            var tnTrans = TangleNetTransaction.FromTrytes(resultTrytes[0]);

            Assert.IsTrue(tnTrans.IsTail);

            Transaction newTrans = Transaction.FromJSON(tnTrans.Fragment.ToUtf8String());

            Assert.AreEqual(trans, newTrans);

            var transList = Core.GetAllTransactionsFromAddress(sendTo);
            var findTrans = transList.Where(m => m.Equals(trans));

            Assert.AreEqual(findTrans.Count(), 1);

            trans.Hash = null;
            Assert.AreEqual("Transaction is not finalized. Did you forget to Final() the Transaction?", Assert.Throws<ArgumentException>(() => Core.UploadTransaction(trans)).Message);

        }

        [Test]
        public void FillTransactionPool() {

            string coinName = Utils.GenerateRandomString(10);

            ChainSettings cSett = new ChainSettings(100, -1, 0, 4, 100, 10, 20);
            IXISettings.AddChainSettings(coinName, cSett);

            int n = 3;
            string addr = Utils.GetTransactionPoolAddress(3, coinName);
            IXISettings.Default(true);

            for (int i = 0; i < n; i++) {
                //we create now the transactions
                Transaction trans = new Transaction("ME", 1, addr);
                trans.AddFee(30);
                trans.AddOutput(100, "YOU");
                trans.Final();

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            var transList = Core.GetAllTransactionsFromAddress(addr);

            Assert.AreEqual(transList.Count, n);

            Console.WriteLine(addr);
        }

        [Test]
        public void BlockDownloadAllFromAddress() {

            IXISettings.Default(true);

            var blockList = Core.GetAllBlocksFromAddress(GenesisAddress, null, null);

            Assert.AreEqual(2, blockList.Count);
        }

        [Test]
        public void DownloadCompleteHistory() {

            IXISettings.Default(true);
            Block latest = Core.DownloadChain(GenesisAddress, GenesisHash, true, (Block b) => { Console.WriteLine(b.Height); }, CoinName);

        }
    }
}
