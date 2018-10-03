using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Interfaces
{
    public interface IDownloadable
    {
        string SendTo { get; set; }
        string Hash { get; set; }

    }
}
