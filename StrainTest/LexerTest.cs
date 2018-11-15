using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Strain;

namespace StrainTest
{
    [TestFixture]
    public class LexerTest
    {
        [Test]
        public void SimpleLexing() {

            var code = "funct Main { int i = 0; if(x == 0){ i = 1;}}";

            var list = new Lexer(code).Lexing();

            ;

        }

    }
}
