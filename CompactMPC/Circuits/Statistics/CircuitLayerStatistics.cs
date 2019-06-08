using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Statistics
{
    public class CircuitLayerStatistics
    {
        private int _numberOfNonlinearGates;
        private int _numberOfLinearGates;
        
        public CircuitLayerStatistics(int numberOfNonlinearGates, int numberOfLinearGates)
        {
            _numberOfNonlinearGates = numberOfNonlinearGates;
            _numberOfLinearGates = numberOfLinearGates;
        }

        public int NumberOfNonlinearGates
        {
            get
            {
                return _numberOfNonlinearGates;
            }
        }

        public int NumberOfLinearGates
        {
            get
            {
                return _numberOfLinearGates;
            }
        }
    }
}
