using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using NUnit.Framework;
using TangleChainIXI.Classes;
using System.Threading;
using Tangle.Net.Cryptography;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests
{

    [TestFixture]
    public class TestCryptography
    {

        [Test]
        public void WrongHash()
        {

            int difficulty = 60;
            IXISettings.Default(false);

            Block block = new Block(3, "lol", "test").Final();

            block.Hash = "LOLOLOLOL";

            block.VerifyHash().Should().BeFalse();

            block.Nonce = 0;

            block.VerifyNonce(difficulty).Should().BeFalse();

            block.Final().VerifyHash().Should().BeTrue();

        }

        [Test]
        public void ProofOfWorkStop()
        {

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Thread a = new Thread(() =>
            {
                long nonce = Cryptography.ProofOfWork("ASDASDASDASD", 100, token);
                nonce.Should().Be(-1);
            });
            a.Start();

            source.Cancel();

        }

        [Test]
        public void DifficultyChange()
        {
            Cryptography.CalculateDifficultyChange(26).Should().Be(2);
            Cryptography.CalculateDifficultyChange(10).Should().Be(1);
            Cryptography.CalculateDifficultyChange(0.1).Should().Be(-2);
            Cryptography.CalculateDifficultyChange(27).Should().Be(2);
            Cryptography.CalculateDifficultyChange(2187).Should().Be(6);
            Cryptography.CalculateDifficultyChange(27).Should().NotBe(0);
            Cryptography.CalculateDifficultyChange(1000000000).Should().Be(0);
        }

        [Test]
        public void VerifyNonce()
        {

            int difficulty = 7;

            //smaller
            var check01 = "99C";
            Converter.TrytesToTrits(check01).VerifyDifficulty(difficulty).Should().BeTrue();

            //higher
            var check02 = "99A";
            Converter.TrytesToTrits(check02).VerifyDifficulty(difficulty).Should().BeFalse();

        }

        [Test]
        public void ProofOfWork()
        {

            int difficulty = 7;
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            long nonce = Cryptography.ProofOfWork(hash, difficulty);

            hash.VerifyNonce(nonce, difficulty).Should().BeTrue();

            difficulty += 30;

            hash.VerifyNonce(nonce, difficulty).Should().BeFalse();

            Console.WriteLine("Hash: " + hash);
            Console.WriteLine("Nonce" + nonce);

        }

        [Test]
        public void VerifyHash()
        {

            //precomputed
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";
            int nonce = 479;
            int difficulty = 7;

            Cryptography.VerifyNonce(hash, nonce, difficulty).Should().BeTrue();

        }

        [Test]
        public void GetPublicKey()
        {

            string privateKey = "123456789";
            string publicKey = privateKey.GetPublicKey();

            publicKey.Should().NotBeNull();

        }

        [Test]
        public void SignMessage()
        {

            var message = "Hello World!";
            string privateKey = "teedsdddddddddddeeeeeeeeeeeeeeee";
            var publicKey = privateKey.GetPublicKey();
            var signature = Cryptography.Sign(message, privateKey);

            message.VerifyMessage(signature, publicKey).Should().BeTrue();

        }

        [Test]
        public void TransactionSignature()
        {

            IXISettings.Default(true);

            new Transaction(IXISettings.GetPublicKey(), 1, "ADDR")
                .AddFee(1)
                .AddOutput(100, "YOU")
                .Final()
                .VerifySignature()
                .Should().BeTrue();


        }
    }
}
