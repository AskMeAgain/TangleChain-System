using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using IXIComponents.Simple;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace ComponentTest
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SimpleTangleAccessorTest
    {
        private static IXISettings _settings;

        private static IEnumerable<TestCaseData> IDownloadableObjects()
        {
            _settings = new IXISettings().Default(true);

            yield return new TestCaseData(new Transaction("me", 1, Utils.GenerateRandomString(81)).Final(_settings));
            yield return new TestCaseData(new Smartcontract("cool", Utils.GenerateRandomString(81)).Final(_settings));
            yield return new TestCaseData(new Block(0, Utils.GenerateRandomString(81), "TangleChain").Final(_settings));
        }

        [Test, TestCaseSource("IDownloadableObjects")]
        public void UploadDownload<T>(T obj) where T : IDownloadable
        {
            obj.Upload();

            var result = new SimpleTangleAccessor().GetSpecificFromAddress<T>(obj.Hash, obj.SendTo, _settings);

            result.HasValue.Should().BeTrue();
            result.Value.Should().Be(obj);
        }
    }
}
