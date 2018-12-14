using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage
{
    public class ParserContext
    {
        private string _context;
        private int _counter;

        public ParserContext(string context)
        {
            _context = context;
        }

        public ParserContext NewContext(string con = null) {

            return null;
        }

    }
}
