using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tangle.Net.Repository.Client;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.UnitTests {
    [TestFixture]
    public class TestClasses {

        [Test]
        public void BlockJSON() {

            IXISettings.Default(true);

            //create dummy block first
            Block testBlock = new Block(3,"sendto", "name");

            //HACK, DONT DO THIS NORMALLY. BETTER COMPUTE POW WITH FUNCTION
            testBlock.Difficulty = new Difficulty(3);

            testBlock.Final();


            //convert to json
            string json = testBlock.ToJSON();

            //convert string to block
            Block newBlock = Block.FromJSON(json);

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void TestBlock() {

            IXISettings.Default(true);

            Block block = new Block();

            Transaction trans1 = new Transaction("you", 1, "lol");
            trans1.Final();

            Transaction trans2 = new Transaction("you2", 1, "lol3");
            trans2.Final();

            Transaction trans3 = new Transaction("you3", 1, "lol2");
            trans3.Final();

            block.AddTransactions(new List<Transaction>() { trans1, trans2, trans3 });

        }

        [Test]
        public void TestDifficulty() {

            Difficulty d = new Difficulty(1);

            d += 1;

            Assert.IsTrue(d.PrecedingZeros == 2);

        }

        [Test]
        public void TestWay() {

            Way way01 = new Way("hash", "addr", 12, 12312312312);
            Way way02 = new Way("hash2", "addr2", 3, 12312312312);

            way01.AddOldWay(way02);

            Way checkWay = way01.GetWayViaHeight(3);

            Assert.AreEqual(way02, way01.Before);
            Assert.AreEqual(way01.BlockHeight, 12);
            Assert.AreEqual(way02, checkWay);
        }

        [Test]
        public void TestTransactions() {

            //adding fees
            Transaction trans = new Transaction("from", 1, "to");
            trans.AddFee(100);
            trans.AddOutput(100, "you");
            trans.AddOutput(200, "youagain");

            Assert.AreEqual(400, trans.ComputeOutgoingValues());
            Assert.IsNotNull(trans.Data);

            //adding NO output
            Transaction trans2 = new Transaction("from2", 2, "to2");
            trans2.AddOutput(-100, "lol");

            Assert.AreEqual(0, trans2.OutputReceiver.Count);

            //genesis stuff

            Transaction genesis1 = new Transaction("from3", 1, "addr2");
            genesis1.Time = Timestamp.UnixSecondsTimestamp;
            genesis1.GenerateHash();

            ChainSettings cSett = new ChainSettings(10, 10, 10, 10, 10, 10, 6);

            genesis1.SetGenesisInformation(10, 10, 10, 10, 10, 10, 6);

            Transaction genesis2 = genesis1;

            genesis2.SetGenesisInformation(cSett);

            Assert.AreEqual(-1, genesis1.Mode);
            Assert.AreEqual(genesis1.Data[7], "6");
            Assert.AreEqual(genesis1.Data[0], "0");
            Assert.AreEqual(genesis1, genesis2);

        }

    }
}
