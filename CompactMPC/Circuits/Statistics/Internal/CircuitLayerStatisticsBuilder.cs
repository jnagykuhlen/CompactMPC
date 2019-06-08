using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Statistics.Internal
{
    public class CircuitLayerStatisticsBuilder
    {
        private int _numberOfNonlinearGates;
        private int _numberOfLinearGates;

        public CircuitLayerStatisticsBuilder()
        {
            _numberOfNonlinearGates = 0;
            _numberOfLinearGates = 0;
        }

        public void AddNonlinearGate()
        {
            _numberOfNonlinearGates++;
        }

        public void AddLinearGate()
        {
            _numberOfLinearGates++;
        }

        public CircuitLayerStatistics Create()
        {
            return new CircuitLayerStatistics(_numberOfNonlinearGates, _numberOfLinearGates);
        }
    }
}
