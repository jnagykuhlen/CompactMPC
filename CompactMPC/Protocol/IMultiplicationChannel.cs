using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Protocol
{
    public interface IMultiplicationChannel
    {
        Task<BitArray> ComputeMultiplicationShares();
    }
}
