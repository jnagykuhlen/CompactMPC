using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class GateEqualityComparer : IEqualityComparer<Gate>
    {
        public static readonly GateEqualityComparer Instance = new GateEqualityComparer();

        public bool Equals(Gate first, Gate second)
        {
            return first.Context.CircuitUniqueId == second.Context.CircuitUniqueId;
        }

        public int GetHashCode(Gate gate)
        {
            return gate.Context.CircuitUniqueId;
        }
    }
}
