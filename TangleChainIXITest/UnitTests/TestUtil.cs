using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using System;
using Tangle.Net.Cryptography;
using System.Threading;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests
{

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestUtil
    {

        [Test]
        public void TestRandomGenerator()
        {
            int length = 10;
            string result = Utils.GenerateRandomString(length);

            result.Length.Should().Be(length);
        }

        [Test]
        [TestCase(@"https://potato.iotasalad.org:14265", true)]
        [TestCase(@"https://google.org/:3000", false)]
        public void TestConnection(string url, bool result)
        {
            Utils.TestConnection(url).Should().Be(result);
        }

        [Test]
        public void TestGetPoolAddress()
        {

            Utils.GetTransactionPoolAddress(0, "lol", 100).Should().NotBeNull();

        }
    }
}
