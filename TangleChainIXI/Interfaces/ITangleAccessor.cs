using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface ITangleAccessor
    {
        T GetSpecificFromAddress<T>(string hash, string address, IXISettings settings) where T : IDownloadable;
        List<T> GetAllFromAddress<T>(string address, IXISettings settings) where T : IDownloadable;
    }
}
