using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Strain;
using Strain.Classes;

namespace StrainTest
{
    [TestFixture]
    public class StrainTest
    {
        [Test]
        public void SimpleLexing()
        {
            var code1 =
                "funct Main {" +
                "   int i = 0; " +
                "   if(x == 0){ " +
                "     i = 1;" +
                "   }" +
                "}";

            var code2 =
                "funct Main {" +
                "   int i = 0; " +
                "   if(x == 0){ " +
                "     i = 1;" +
                "   }" +
                "}" +
                "funct Main2 {" +
                "   int i = 0; " +
                "   if(x == 0){ " +
                "     i = 1;" +
                "   }" +
                "}";

            var result = new Strain.Strain(code1).Lexing();
            ;
            Console.WriteLine("-----------------------");
            new Strain.Strain(code2).Lexing().Print();

        }

        [Test]
        public void SimpleParser()
        {
            var code1 =
                "function Main {" +
                "int test = 3 + 2;" +
                "if(var is true){" +
                "//lol;" +
                "int test = 3;" +
                "}else{" +
                "int test = 1;" +
                "}" +
                "if(lol is null){" +
                "int i = 0;" +
                "}" +
                "}";

            var strain = new Strain.Strain(code1);

            var lexed = strain.Lexing();

            ;

            var result = strain.Parse(lexed);

        }

    }
}
