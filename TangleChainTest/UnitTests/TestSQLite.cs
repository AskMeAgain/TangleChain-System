using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests
{
    [TestFixture]
    public class TestSQLite
    {

        [Test]
        public void AddGetBlock() {

            //get connection
            DataBase_Lite Db = new DataBase_Lite("test");

            //create block
            Block block = new Block(2,"you","coolcoin");         
            block.Final();

            Db.AddBlock(block, false);
            //Console.WriteLine("_________________");
            //Db.GetBlock(block.Height);

            //Assert.AreEqual(result, block);

        }

        //[Test]
        //public void AddGetTransaction() {

        //    //connection
        //    DataBase_Lite Db = new DataBase_Lite("test");

        //    //create Trans
        //    Transaction trans = new Transaction("ME", 0, "CoolAddr");
        //    trans.AddFee(10);
        //    trans.AddOutput(100, "you");
        //    trans.Final();

        //    Db.AddTransaction(trans);

        //    Transaction result = Db.GetTransaction(trans.Hash,trans.SendTo);

        //    Assert.AreEqual(result, trans);

        //}

        [Test]
        public void Init() {

            DataBase_Lite Db = new DataBase_Lite("Test");

        }

    }
}
