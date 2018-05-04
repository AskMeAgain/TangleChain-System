﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChain.Classes {

    public class Way {

        public int Length { get; set; }

        public int BlockHeight { get; set; }
        public string BlockHash { get; set; }
        public string Address { get; set; }

        public Way Before { get; set; }

        public Way(string hash, string addr,int height) {
            BlockHash = hash;
            Address = addr;
            Before = null;
            Length = 1;
            BlockHeight = height;
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
            if (Before != null)
                Before.Print();


        }

    }
}
