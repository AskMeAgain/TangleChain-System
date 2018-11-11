using System;
using NUnit.Framework;
using FluentAssertions;
using Strain;

namespace StrainTest
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void Mvp()
        {
            string code = "Main { }";

            Parser parser = new Parser(code);

            var list = parser.Parse();

            list[0].Should().Be("Main");
            list[1].Should().Be("Exit");
        }
    }
}
