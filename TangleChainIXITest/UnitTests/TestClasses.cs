using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tangle.Net.Repository.Client;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class TestClasses
    {

        [Test]
        public void TestDifficulty()
        {

            Difficulty d = new Difficulty(1);

            d += 1;

            Assert.IsTrue(d.PrecedingZeros == 2);

        }

        [Test]
        public void TestTransactions()
        {

            //adding fees
            Transaction trans = new Transaction("from", 1, "to")
                .AddFee(100)
                .AddOutput(100, "you")
                .AddOutput(200, "youagain");

            trans.ComputeOutgoingValues().Should().Be(400);
            trans.Data.Should().NotBeNull();

            //adding NO output
            Transaction trans2 = new Transaction("from2", 2, "to2")
                .AddOutput(-100, "lol");

            trans2.OutputReceiver.Count.Should().Be(0);

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
