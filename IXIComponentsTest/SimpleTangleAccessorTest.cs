using System.Collections.Generic;
using FluentAssertions;
using IXIComponents.Simple;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace IXIComponentsTest
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SimpleTangleAccessorTest
    {
        private static SimpleTangleAccessor _tangleAccessor;

        private static IEnumerable<TestCaseData> DownloadableObjects()
        {
            var _settings = new IXISettings().Default(true);
            _tangleAccessor = new SimpleTangleAccessor(_settings);

            yield return new TestCaseData(new Transaction("me", 1, Utils.GenerateRandomString(81)).Final(_settings));
            yield return new TestCaseData(new Smartcontract("cool", Utils.GenerateRandomString(81)).Final(_settings));
            yield return new TestCaseData(new Block(0, Utils.GenerateRandomString(81), "TangleChain").Final(_settings));
        }

        [Test, TestCaseSource(nameof(DownloadableObjects))]
        public void UploadDownload<T>(T obj) where T : IDownloadable
        {
            obj.Upload();

            var result = _tangleAccessor.GetSpecificFromAddress<T>(obj.Hash, obj.SendTo);

            result.HasValue.Should().BeTrue();
            result.Value.Should().Be(obj);
        }

        [Test, TestCaseSource(nameof(DownloadableObjects))]
        public void CacheVerification<T>(T obj) where T : IDownloadable
        {
            obj.Upload();
            
            var result1 = _tangleAccessor.GetSpecificFromAddress<T>(obj.Hash, obj.SendTo);
            var result2 = _tangleAccessor.GetSpecificFromAddress<T>(obj.Hash, obj.SendTo);

            result1.Value.Should().Be(result2.Value);
        }
    }
}
