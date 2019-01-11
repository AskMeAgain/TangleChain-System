using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Tangle.Net.Repository;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleNet = Tangle.Net.Entity;

namespace TangleChainIXI
{
    public class SimpleLogicManager : ILogicManager
    {

        private readonly IDataAccessor _dataAccessor;
        private readonly IConsensus _consensus;
        private readonly ITangleAccessor _tangleAccessor;

        public SimpleLogicManager(string coinName, IDataAccessor dataAccessor, IConsensus consensus, ITangleAccessor tangleAccessor)
        {
            _dataAccessor = dataAccessor;
            _consensus = consensus;
            _tangleAccessor = tangleAccessor;
        }

        public Block GetSpecificBlock(string address, string hash)
        {
            return _tangleAccessor.GetBlock(address, hash);
        }

        public List<Block> FindCorrectWay(string address, long startHeight)
        {
            return _consensus.FindNewBlocks(address, startHeight, GetDifficulty(startHeight));
        }

        public int GetDifficulty(long? height)
        {
            return _consensus.GetDifficulty(height);
        }

        public void AddBlock(List<Block> obj)
        {
            obj.ForEach(x => _dataAccessor.AddBlock(x));
        }
    }
}
