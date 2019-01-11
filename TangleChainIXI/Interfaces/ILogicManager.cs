using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Interfaces
{
    public interface ILogicManager
    {

        Block GetSpecificBlock(string address, string hash);

        List<Block> FindCorrectWay(string address, long startHeight);

        int GetDifficulty(long? height);

        void AddBlock(List<Block> obj);

    }
}
