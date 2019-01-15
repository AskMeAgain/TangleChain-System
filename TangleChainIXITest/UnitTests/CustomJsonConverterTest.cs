using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class CustomJsonConverterTest
    {
        private IXISettings _settings;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = new IXISettings().Default(true);
        }


        [Test]
        public void ReadTest()
        {

            var smart = new Smartcontract("me", "sendto");

            smart.AddExpression(new Expression(00, "", "", ""))
                .AddVariable("lol",new SC_Int())
                .AddVariable("lol2",new SC_String())
                .Final(_settings);

            var json = smart.ToJSON();

            var result = Utils.FromJSON<Smartcontract>(json);

            result.Should().NotBeNull();
            result.Should().Be(smart);

        }

    }
}
