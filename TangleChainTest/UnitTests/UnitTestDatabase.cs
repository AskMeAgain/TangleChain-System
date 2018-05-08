using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests
{
    [TestClass]
    public class UnitTestDatabase
    {

        [TestMethod]
        public void TestInit() {

            DataBase db = new DataBase("Test");

            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsWorking());

        }

        [TestMethod]
        public void TestAddAndGetBlock(){

            Block test = new Block();
            DataBase db = new DataBase("Test");

            db.AddBlock(test);

            Block compare = db.GetBlock(test.Height);

            Assert.AreEqual(test, compare);

        }

    }
}
