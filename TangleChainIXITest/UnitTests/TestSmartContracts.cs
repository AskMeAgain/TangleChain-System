using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using TangleChainIXI.Smartcontracts;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class TestSmartContracts
    {

        [Test]
        public void TestDownloadMultipleSmartcontracts()
        {

            IXISettings.Default(true);

            string addr = Utils.GenerateRandomString(81);

            //first one:
            Smartcontract smart1 = new Smartcontract("test1", addr);
            Smartcontract smart2 = new Smartcontract("test1", addr);

            Expression exp = new Expression(00, Utils.GenerateRandomString(5), Utils.GenerateRandomString(5));

            smart1.AddExpression(exp).Final().Upload();
            smart2.AddExpression(exp).Final().Upload();

            smart1.Should().Be(smart2);

            List<Smartcontract> list = Core.GetAllFromAddress<Smartcontract>(smart1.SendTo);

            list.Count.Should().Be(2);

        }

        [Test]
        public void TestCompile()
        {
            Computer comp = new Computer(new Smartcontract());
            comp.Invoking(y => y.Compile()).Should().Throw<Exception>().WithMessage("Your code doesnt have any entry points!");
        }

    }
}
