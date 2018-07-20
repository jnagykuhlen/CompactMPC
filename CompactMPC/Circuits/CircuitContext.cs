using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class CircuitContext
    {
        private int _numberOfAndGates;
        private int _numberOfXorGates;
        private int _numberOfNotGates;

        public CircuitContext(int numberOfAndGates, int numberOfXorGates, int numberOfNotGates)
        {
            _numberOfAndGates = numberOfAndGates;
            _numberOfXorGates = numberOfXorGates;
            _numberOfNotGates = numberOfNotGates;
        }

        public int NumberOfAndGates
        {
            get
            {
                return _numberOfAndGates;
            }
        }

        public int NumberOfXorGates
        {
            get
            {
                return _numberOfXorGates;
            }
        }

        public int NumberOfNotGates
        {
            get
            {
                return _numberOfNotGates;
            }
        }

        public int NumberOfGates
        {
            get
            {
                return _numberOfAndGates + _numberOfXorGates + _numberOfNotGates;
            }
        }
    }
}
