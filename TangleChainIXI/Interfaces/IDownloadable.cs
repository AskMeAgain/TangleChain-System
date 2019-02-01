using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Interfaces
{
    public interface IDownloadable
    {
        string NodeAddress { get; set; }
        string SendTo { get; set; }
        string Hash { get; set; }

        bool IsFinalized { get; }

        IDownloadable GenerateHash();

    }
}
