using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using NUnit.Framework;

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
            var privateKey = "68040878110175628235481263019639686";
            var publicKey = Cryptography.GetPublicKey(privateKey);
            var signature = Cryptography.CryptoSign(message, privateKey);

            Console.WriteLine(signature);

            bool result = Cryptography.VerifyMessage(message,signature, publicKey);

            Assert.IsTrue(result);
        }

    }
}
