using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using NUnit.Framework;
using TangleChainIXI.Classes;
using System.Threading;
using Tangle.Net.Cryptography;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestCryptography {

        [Test]
        public void WrongHash() {

            Difficulty difficulty = new Difficulty(60);
            IXISettings.Default(false);

            Block block = new Block(3, "lol", "test");
            block.Final();

            block.Hash = "LOLOLOLOL";

            Assert.IsFalse(Cryptography.VerifyBlockHash(block));

            block.Nonce = 0;

            Assert.IsFalse(Cryptography.VerifyHashAndNonceAgainstDifficulty(block,difficulty));

            block.Final();

            Assert.IsTrue(Cryptography.VerifyBlockHash(block));


        }

        [Test]
        public void ProofOfWorkStop() {

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Thread a = new Thread(() => {
                long nonce = Cryptography.ProofOfWork("ASDASDASDASD",new Difficulty(100),token);
                Assert.AreEqual(-1, nonce);
            });
            a.Start();

            source.Cancel();

        }

        [Test]
        public void DifficultyChange() {

            Assert.AreEqual(Cryptography.CalculateDifficultyChange(26),2);
            Assert.AreEqual(Cryptography.CalculateDifficultyChange(10),1);
            Assert.AreEqual(Cryptography.CalculateDifficultyChange(0.1),-2);
            Assert.AreEqual(Cryptography.CalculateDifficultyChange(27),2);
            Assert.AreEqual(Cryptography.CalculateDifficultyChange(2187),6);

            Assert.AreNotEqual(Cryptography.CalculateDifficultyChange(27),0);

            Assert.AreEqual(0, Cryptography.CalculateDifficultyChange(1000000000), 0);

        }

        [Test]
        public void VerifyNonce() {

            Difficulty difficulty = new Difficulty(7);

            //smaller
            var check01 = "99C";
            Assert.IsTrue(Cryptography.VerifyHashAgainstDifficulty(Converter.TrytesToTrits(check01), difficulty));

            //higher
            var check02 = "99A";
            Assert.IsFalse(Cryptography.VerifyHashAgainstDifficulty(Converter.TrytesToTrits(check02), difficulty));

        }

        [Test]
        public void ProofOfWork() {

            Difficulty difficulty = new Difficulty(7);
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            long nonce = Cryptography.ProofOfWork(hash, difficulty);

            Assert.IsTrue(Cryptography.VerifyHashAndNonceAgainstDifficulty(hash, nonce, difficulty));

            difficulty.PrecedingZeros += 30;

            Assert.IsFalse(Cryptography.VerifyHashAndNonceAgainstDifficulty(hash, nonce, difficulty));

            Console.WriteLine("Hash: " + hash);
            Console.WriteLine("Nonce" + nonce);

        }

        [Test]
        public void VerifyHash() {

            //precomputed
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";
            int nonce = 479;
            Difficulty difficulty = new Difficulty();

            Assert.IsTrue(Cryptography.VerifyHashAndNonceAgainstDifficulty(hash, nonce, difficulty));

        }

        [Test]
        public void GetPublicKey() {

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
        public void TransactionSignature() {

            IXISettings.Default(true);

            Transaction trans = new Transaction(IXISettings.GetPublicKey(), 1, "ADDR");
            trans.AddFee(1);
            trans.AddOutput(100, "YOU");
            trans.Final();

            Assert.IsTrue(trans.Verify());

        }
    }
}
