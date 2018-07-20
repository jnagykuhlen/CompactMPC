using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Circuits
{
    public interface ICircuitRecorder
    {
        void Record(CircuitBuilder builder);
    }
}
