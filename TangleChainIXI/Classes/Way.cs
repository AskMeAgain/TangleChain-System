using System;
using System.Collections.Generic;

namespace TangleChainIXI.Classes
{

    public class Way
    {

        public int Length { get; set; }

        public Block CurrentBlock { get; set; }

        public Way Before { get; set; }

        public Way(Block block)
        {
            Before = null;
            Length = 1;
            CurrentBlock = block;
        }

        public void AddOldWay(Way way)
        {
            Before = way;
            Length = way.Length + 1;

        }

        public void Print()
        {

            CurrentBlock.Print();
            Console.WriteLine("==============================================================");

            Before?.Print();

        }

        public Way GetWayViaHeight(long height)
        {
            Way way = this;

            while (way.CurrentBlock.Height != height && way.Before != null)
            {
                way = way.Before;
            }

            return (way.CurrentBlock.Height == height) ? way : null;

        }

        public List<Block> ToBlockList()
        {

            var returnList = new List<Block>();

            returnList.Add(CurrentBlock);
            Way curWay = Before;

            if (curWay != null)
                do
                {
                    returnList.Insert(0, curWay.CurrentBlock);
                    curWay = curWay.Before;
                } while (curWay.Before != null);

            return returnList;
        }
    }
}
