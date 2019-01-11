using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Interfaces
{
    public interface IConsensus
    {
        List<Block> FindNewBlocks(string address, long height, int startDifficulty);
    }
}
