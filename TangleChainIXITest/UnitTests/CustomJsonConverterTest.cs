using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class CustomJsonConverterTest
    {

        private static IXISettings _settings;

        private static IEnumerable<TestCaseData> Types()
        {
            _settings = new IXISettings().Default(true);

            yield return new TestCaseData(new Transaction("me", 1, Utils.GenerateRandomString(81)).Final(_settings));
            yield return new TestCaseData(CreateSmartcontract());
            yield return new TestCaseData(new Block(0, Utils.GenerateRandomString(81), "TangleChain").Final(_settings));
        }

        private static Smartcontract CreateSmartcontract()
        {
            var smart = new Smartcontract("me", "sendto");

            smart.AddExpression(new Expression(00, "asd", "asd", "asd"))
                .AddExpression(new Expression(00, "bbb", "bbb", "bbb"))
                .AddExpression(new Expression(00, "ccc", "ccc"))
                .AddExpression(new Expression(00, "ccc"))
                .AddExpression(new Expression(00))
                .AddVariable("lol", new SC_Int(1110))
                .AddVariable("lol2", new SC_String("lol3333"))
                .AddVariable("lol3", new SC_String("lol3"))
                .AddVariable("lol4", new SC_Int("2323222"))
                .Final(_settings);

            return smart;
        }

        [Test, TestCaseSource("Types")]
        public void ConvertTest<T>(T obj) where T : IDownloadable
        {

            var json = obj.ToJSON();

            var result = Utils.FromJSON<T>(json);

            result.Should().NotBeNull();
            result.Should().Be(obj);
        }

    }
}
