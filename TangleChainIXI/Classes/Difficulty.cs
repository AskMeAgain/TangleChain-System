using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Classes {
    public class Difficulty {

        public int PrecedingZeros { get; set; }
        public int Number { get; set; }

        public Difficulty(int prec, int num) {
            PrecedingZeros = prec;
            Number = num;
        }

        public Difficulty() {
            PrecedingZeros = 2;
            Number = 5;
        }

        public Difficulty(int i) {
            PrecedingZeros = i;
            Number = 5;
        }

        public void Print() {
            Console.WriteLine($"Preceding Zeros: {PrecedingZeros} \nNumber: {Number}");
        }
    }
}
