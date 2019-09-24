using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface ITangleAccessor
    {
        Maybe<T> GetSpecificFromAddress<T>(string hash, string address) where T : IDownloadable;
        List<T> GetAllFromAddress<T>(string address) where T : IDownloadable;
    }
}
