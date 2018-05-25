using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TangleChain;
using TangleChain.Classes;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;

namespace TangleChainTest.CompleteExample {

    [TestClass]
    class Example_01 {

        [TestMethod]
        public void Start() {

            //we first create a random coin name:
            string name = GenerateRandomString();



        }

        public string GenerateRandomString() {

            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());

        }


    }

}
