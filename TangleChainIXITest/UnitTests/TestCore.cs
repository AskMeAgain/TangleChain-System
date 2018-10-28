using NUnit.Framework;
using System;
using System.Collections.Generic;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;
using FluentAssertions;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXITest.UnitTests
{

    [TestFixture]
    public class TestCore
    {

        private string GenesisAddress;
        private string GenesisHash;
        private string CoinName;
        private string DuplicateBlockHash;

        //[OneTimeSetUp]
        public void InitSpecificChain()
        {

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

            Block testBlock = new Block(3, name, "coolname");

            IXISettings.Default(true);

            testBlock.Invoking(b => b.Upload()).Should().Throw<ArgumentException>()
                .WithMessage("Object not finalized");

        }


        [Test]
        public void BlockFailAtSpecific()
        {
            IXISettings.Default(true);
            Core.GetSpecificFromAddress<Block>(Utils.GenerateRandomString(81), "lol")
                .Should().BeNull();
        }

        [Test]
        public void TransactionUploadDownload()
        {

            IXISettings.Default(true);

            string sendTo = Utils.GenerateRandomString(81);

            Transaction trans = new Transaction("ME", 0, sendTo)
                .AddFee(30)
                .AddOutput(100, "YOU")
                .Final();

            var resultTrytes = trans.Upload();

            Core.GetSpecificFromAddress<Transaction>(trans.SendTo, trans.Hash).Should().Be(trans);

            var transList = Core.GetAllFromAddress<Transaction>(sendTo);
            var findTrans = transList.Where(m => m.Equals(trans));

            Assert.AreEqual(findTrans.Count(), 1);

        }

        [Test]
        public void TransactionAndSmartcontractSplit() {

            IXISettings.Default(true);

            var poolAddr = "YXRCEGEYJDKKPROUM9PFKWECOCJRYFIQGPRAPSCHNOORH9JBNBYXSVCUBVNBDYKWLMQCOWPANDLLVQJMI";

            var smartList = Core.GetAllFromAddress<Smartcontract>(poolAddr);
            var transList = Core.GetAllFromAddress<Transaction>(poolAddr);

            transList.Count.Should().Be(0);
        }

        [Test]
        public void AddingMixedListToBlock() {

            IXISettings.Default(true);

            var poolAddr = "YXRCEGEYJDKKPROUM9PFKWECOCJRYFIQGPRAPSCHNOORH9JBNBYXSVCUBVNBDYKWLMQCOWPANDLLVQJMI";

            var selectedSmart = Core.GetAllFromAddress<Smartcontract>(poolAddr);
            var selectedTrans = Core.GetAllFromAddress<Transaction>(poolAddr);

            //select now highest fees
            List<ISignable> list = selectedSmart.Cast<ISignable>().ToList();
            list.AddRange(selectedTrans.Cast<ISignable>());
            var sortedList = list.OrderBy(x => x.GetFee()).Take(3).ToList();

            Block block = new Block();
            block.Add(sortedList);

            block.SmartcontractHashes.Count.Should().Be(1);

        }
    }
}
