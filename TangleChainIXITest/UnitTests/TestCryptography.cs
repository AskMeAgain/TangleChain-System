using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using NUnit.Framework;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestCryptography {

        [Test]
        public void TestGetPubKey() {

            string privateKey = "123456789";
            string publicKey = Cryptography.GetPublicKey(privateKey);
            Console.WriteLine(publicKey);
        }

        [Test]
        public void SignMessage() {

            var message = "Hello World!";
            string privateKey = "teedsdddddddddddeeeeeeeeeeeeeeee";
            var publicKey = Cryptography.GetPublicKey(privateKey);
            var signature = Cryptography.Sign(message, privateKey);

            bool result1 = Cryptography.VerifyMessage(message, signature, publicKey);

            Assert.IsTrue(result1); 
       

        }

        [Test]
        public void TestTransactionSignature() {

            IXISettings.Default(true);

            Transaction trans = new Transaction(IXISettings.GetPublicKey(), 1, "ADDR");
            trans.AddFee(1);
            trans.AddOutput(100, "YOU");
            trans.Final();

            Assert.IsTrue(trans.VerifySignature());

        }
    }
}
