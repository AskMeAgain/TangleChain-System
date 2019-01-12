using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SimpleTangleAccessorTest
    {
        private static IEnumerable<TestCaseData> IDownloadableObjects()
        {
            IXISettings.Default(true);

            yield return new TestCaseData(new Transaction("me",1,Utils.GenerateRandomString(81)).Final());
            yield return new TestCaseData(new Smartcontract("cool",Utils.GenerateRandomString(81)).Final());
            yield return new TestCaseData(new Block(0,Utils.GenerateRandomString(81),"TangleChain").Final());
        }

        [Test, TestCaseSource("IDownloadableObjects")]
        public void UploadDownload<T>(T obj) where T : IDownloadable
        {
            obj.Upload();

            var result = new SimpleTangleAccessor().GetSpecificFromAddress<T>(obj.Hash, obj.SendTo);

            result.Should().NotBeNull();
            result.Should().Be(obj);

        }
    }
}
