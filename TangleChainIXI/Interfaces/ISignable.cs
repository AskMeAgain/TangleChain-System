using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Interfaces
{
    public interface ISignable
    {
        string Signature { get; set; }
        string Hash { get; set; }
        string From { get; set; }
        bool IsFinalized { get; set; }

        void Sign();
    }
}
