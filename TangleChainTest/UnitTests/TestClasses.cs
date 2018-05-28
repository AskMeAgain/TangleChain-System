using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {
    [TestFixture]
    public class TestClasses {

        [Test]
        public void Way() {

            Way way01 = new Way("hash","addr", 12);
            Way way02 = new Way("hash2","addr2", 3);

            way01.AddOldWay(way02);

            Assert.AreEqual(way02, way01.Before);

            Assert.AreEqual(way01.BlockHeight,12);
        }

        [Test]
        public void Settings(){
            //TODO
        }
    }
}
