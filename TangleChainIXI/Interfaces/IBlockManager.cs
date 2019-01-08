﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Interfaces
{
    public interface IBlockManager
    {
        string CoinName { get; set; }

        Block GetSpecificBlock(string address, string hash);

        Way FindCorrectWay(string address, long startHeight);

        int GetDifficulty(long height);

        void AddBlock(List<Block> obj);

        Block GetBlock(int height);
    }
}
