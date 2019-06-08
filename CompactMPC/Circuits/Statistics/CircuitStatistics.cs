using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Statistics.Internal;

namespace CompactMPC.Circuits.Statistics
{
    public class CircuitStatistics
    {
        private IReadOnlyList<CircuitLayerStatistics> _layers;
        private int _numberOfInputs;
        private int _numberOfOutputs;

        public CircuitStatistics(IReadOnlyList<CircuitLayerStatistics> layers, int numberOfInputs, int numberOfOutputs)
        {
            _layers = layers;
            _numberOfInputs = numberOfInputs;
            _numberOfOutputs = numberOfOutputs;
        }

        public static CircuitStatistics FromCircuit(Circuit circuit)
        {
            return StatisticsCircuitEvaluator.CreateStatistics(circuit);
        }

        public IReadOnlyList<CircuitLayerStatistics> Layers
        {
            get
            {
                return _layers;
            }
        }

        public int NumberOfInputs
        {
            get
            {
                return _numberOfInputs;
            }
        }

        public int NumberOfOutputs
        {
            get
            {
                return _numberOfOutputs;
            }
        }
    }
}
