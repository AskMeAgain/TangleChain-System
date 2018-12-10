using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestCore
    {
        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(false);
        }

        [Test]
        public async Task TestAsyncUpload()
        {

            Stopwatch watch = new Stopwatch();
            watch.Start();

            var block = new Block(1, Utils.GenerateRandomString(81), "lol").Final();

            var task = block.UploadAsync();
            var time1 = watch.Elapsed;

            var result = await task;
            result.Should().BeOfType<Block>();

            var time2 = watch.Elapsed;

            var resultTime = time2 - time1;

            resultTime.TotalSeconds.Should().BeGreaterThan(1);

        }

        [Test]
        public async Task TestAsyncPOW()
        {

            var difficulty = 7;

            var block = new Block(0, Utils.GenerateRandomString(81), "lol");

            Stopwatch watch = new Stopwatch();
            watch.Start();


            var task = block.Final().GenerateProofOfWorkAsync(difficulty);

            watch.Elapsed.TotalSeconds.Should().BeLessOrEqualTo(0.2);

            var result = await task;

            result.Should().BeOfType<Block>();

            result.VerifyNonce(difficulty).Should().BeTrue();

        }

        [Test]
        public async Task TransactionUploadDownload()
        {

            string sendTo = Utils.GenerateRandomString(81);

            Transaction trans = new Transaction("ME", 0, sendTo)
                .AddFee(30)
                .AddOutput(100, "YOU")
                .Final()
                .Upload();

            Core.GetSpecificFromAddress<Transaction>(trans.SendTo, trans.Hash).Should().Be(trans);

            var transList = Core.GetAllFromAddress<Transaction>(sendTo);
            var findTrans = transList.Where(m => m.Equals(trans));

            findTrans.Count().Should().Be(1);

            //we now test also Async version
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var task = Core.GetSpecificFromAddressAsync<Transaction>(trans.SendTo, trans.Hash);
            watch.Elapsed.TotalSeconds.Should().BeLessOrEqualTo(0.2);
            var result = await task;
            watch.Elapsed.TotalSeconds.Should().BeGreaterThan(0.3);

        }

        [Test]
        public void BlockFailAtSpecific()
        {
            Core.GetSpecificFromAddress<Block>(Utils.GenerateRandomString(81), "lol")
                .Should().BeNull();
        }


        [Test]
        public void BlockFailUpload()
        {

            string name = Utils.GenerateRandomString(81);

            Block testBlock = new Block(3, name, "coolname");

            testBlock.Invoking(b => b.Upload()).Should().Throw<ArgumentException>()
                .WithMessage("Object not finalized");

        }
    }
}
