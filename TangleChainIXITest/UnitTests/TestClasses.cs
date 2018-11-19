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
    [Parallelizable(ParallelScope.All)]
    public class TestClasses
    {

        [Test]
        public void TestWay() {

            Block b = new Block(3, "asd", "asd");
            Way way = new Way(b);

            for(int i = 0; i < 10; i++) {
                Way temp = new Way(new Block(i, "asd", "asd"));

                temp.AddOldWay(way);
                way = temp;
            }

            way.Length.Should().Be(11);

            var list = way.ToBlockList();
            list.Count.Should().Be(11);

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

           

        }

        [Test]
        public void GenesisTest() {

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
