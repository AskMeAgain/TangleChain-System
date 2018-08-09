using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Classes {
    [Serializable]
    public class Difficulty {

        public int PrecedingZeros { get; set; }

        public Difficulty(int prec) {
            PrecedingZeros = prec;
        }

        public Difficulty() {
            PrecedingZeros = 5;
        }

        public void Print() {
            Console.WriteLine($"Preceding Zeros: {PrecedingZeros}");
        }

        public static Difficulty operator -(Difficulty a, int b) {
            return new Difficulty(a.PrecedingZeros - b);
        }

        public override string ToString() {
            return PrecedingZeros+"";
        }  
    }
}
