using System;
using System.Collections.Generic;
using System.Text;

namespace  StrainLanguage.Classes
{
    public class ParserContext
    {
        private string _context;
        private int _counter;

        public ParserContext(string context)
        {
            _context = context;
        }

        public ParserContext NewContext(string con = null)
        {

            if (con == null)
            {
                return new ParserContext(_context + "-" + _counter++);
            }

            return new ParserContext(_context + "-" + con + "-" + _counter++);
        }

        public override string ToString()
        {
            return _context;
        }

        public ParserContext OneContextUp() {
            var index = _context.LastIndexOf("-");

            return new ParserContext(_context.Substring(0, index));
        }

    }
}
