using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using System;
using Tangle.Net.Cryptography;
using System.Threading;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestUtil {       

        [Test]
        public void TestRandomGenerator() {
            int length = 10;
            string result = Utils.GenerateRandomString(length);

           result.Length.Should().Be(length);
        }

        [Test]
        public void TestConnection() {

            Utils.TestConnection(@"https://potato.iotasalad.org:14265").Should().BeTrue();
            Utils.TestConnection(@"https://google.org/:3000").Should().BeFalse();

        }       
    }
}
