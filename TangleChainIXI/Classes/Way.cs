using System;

namespace TangleChainIXI.Classes {

    public class Way {

        public int Length { get; set; }

        public long BlockHeight { get; set; }
        public string BlockHash { get; set; }
        public string Address { get; set; }
        public long Time { get; set; }

        public Way Before { get; set; }

        public Way(string hash, string addr,long height, long time) {
            BlockHash = hash;
            Address = addr;
            Before = null;
            Length = 1;
            BlockHeight = height;
            Time = time;
        }

        public void AddOldWay(Way way) {
            Before = way;
            Length = way.Length + 1;

        }

        public void Print() {

            Console.WriteLine("Blockheight: " + BlockHeight);
            Console.WriteLine("Length: " + Length);
            Console.WriteLine("Hash " + BlockHash);
            Console.WriteLine("Address " + Address);
            Console.WriteLine("==============================================================");

            Before?.Print();

        }

    }
}
