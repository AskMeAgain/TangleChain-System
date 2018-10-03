using NUnit.Framework;
using System;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestCore {

        private string GenesisAddress;
        private string GenesisHash;
        private string CoinName;
        private string DuplicateBlockHash;

        //[OneTimeSetUp]
        public void InitSpecificChain() {

            var (genesisAddr, genesisHash, name, dupHash) = Initalizing.SetupCoreTest();

            GenesisAddress = genesisAddr;
            GenesisHash = genesisHash;
            CoinName = name;
            DuplicateBlockHash = dupHash;
        }

        [Test]
        public void BlockFailUpload()
        {

            string name = Utils.GenerateRandomString(81);
            Difficulty difficulty = new Difficulty();

            Block testBlock = new Block(3, name, "coolname");

            IXISettings.Default(true);

            testBlock.Invoking(b => b.Upload()).Should().Throw<ArgumentException>()
                .WithMessage("Object not finalized");

        }


        [Test]
        public void BlockFailAtSpecific() {
            IXISettings.Default(true);
            Block block = Core.GetSpecificFromAddress<Block>(Utils.GenerateRandomString(81), "lol");
            Assert.IsNull(block);
        }

        [Test]
        public void TransactionUploadDownload() {

            IXISettings.Default(true);

            string sendTo = Utils.GenerateRandomString(81);

            Transaction trans = new Transaction("ME", 0, sendTo);
            trans.AddFee(30);
            trans.AddOutput(100, "YOU");
            trans.Final();

            var resultTrytes = trans.Upload();
            var tnTrans = TangleNetTransaction.FromTrytes(resultTrytes[0]);

            Assert.IsTrue(tnTrans.IsTail);

            Transaction newTrans = Utils.FromJSON<Transaction>(tnTrans.Fragment.ToUtf8String());

            Assert.AreEqual(trans, newTrans);

            var transList = Core.GetAllFromAddress<Transaction>(sendTo);
            var findTrans = transList.Where(m => m.Equals(trans));

            Assert.AreEqual(findTrans.Count(), 1);

        }

        //[Test]
        //public void BlockDownloadAllFromAddress() {

        //    IXISettings.Default(true);

        //    var blockList = Core.GetAllBlocksFromAddress(GenesisAddress, null, null, false);

        //    Assert.AreEqual(2, blockList.Count);
        //}

        //[Test]
        //public void DownloadCompleteHistory() {

        //    IXISettings.Default(true);
        //    Block latest = Core.DownloadChain(CoinName, GenesisAddress, GenesisHash, true, true, (Block b) => { Console.WriteLine(b.Height); });

        //}

    }
}
