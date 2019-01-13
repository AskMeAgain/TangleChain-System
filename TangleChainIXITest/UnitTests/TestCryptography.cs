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
    [Parallelizable(ParallelScope.All)]
    public class TestCryptography
    {

        private IXISettings _settings;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = new IXISettings().Default(true);
        }

        [Test]
        public void WrongHash()
        {

            int difficulty = 60;

            Block block = new Block(3, "lol", "test").Final(_settings);

            block.Hash = "LOLOLOLOL";

            block.VerifyHash().Should().BeFalse();

            block.Nonce = 0;

            block.VerifyNonce(difficulty).Should().BeFalse();

            block.Final(_settings).VerifyHash().Should().BeTrue();

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
        [TestCase(26, 2)]
        [TestCase(10, 1)]
        [TestCase(0.1, -2)]
        [TestCase(27, 2)]
        [TestCase(2187, 6)]
        [TestCase(1000000000, 0)]
        public void DifficultyChange(double change, int result)
        {
            Cryptography.CalculateDifficultyChange(change).Should().Be(result);
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

            new Transaction(_settings.PublicKey, 1, "ADDR")
                .AddFee(1)
                .AddOutput(100, "YOU")
                .Final(_settings)
                .VerifySignature()
                .Should().BeTrue();


        }
    }
}
