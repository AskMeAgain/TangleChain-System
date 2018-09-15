using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using System;
using Tangle.Net.Cryptography;
using System.Threading;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestUtil {       

        [Test]
        public void TestRandomGenerator() {
            int length = 10;
            string result = Utils.GenerateRandomString(length);

            Assert.AreEqual(result.Length, length);
        }

        [Test]
        public void TestConnection() {

            Assert.IsTrue(Utils.TestConnection(@"https://potato.iotasalad.org:14265"));
            Assert.IsFalse(Utils.TestConnection(@"https://google.org/:3000"));

        }


       
    }
}
