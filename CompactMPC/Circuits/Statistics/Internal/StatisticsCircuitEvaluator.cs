using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Statistics.Internal
{
    public class StatisticsCircuitEvaluator : ICircuitEvaluator<int>
    {
        private List<CircuitLayerStatisticsBuilder> _layerStatistics;
        private CircuitContext _context;

        private StatisticsCircuitEvaluator()
        {
            _layerStatistics = new List<CircuitLayerStatisticsBuilder>();
            _context = null;
        }

        public static CircuitStatistics CreateStatistics(Circuit circuit)
        {
            StatisticsCircuitEvaluator evaluator = new StatisticsCircuitEvaluator();

            int[] initialLayerIndices = new int[circuit.Context.NumberOfInputGates];
            for (int i = 0; i < initialLayerIndices.Length; ++i)
                initialLayerIndices[i] = 0;

            circuit.Evaluate(evaluator, initialLayerIndices);

            return evaluator.CreateStatistics();
        }

        public int EvaluateAndGate(int leftValue, int rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            int layerIndex = Math.Max(leftValue, rightValue);
            GetLayerStatisticsBuilder(layerIndex).AddNonlinearGate();
            UpdateContext(circuitContext);
            return layerIndex + 1;
        }

        public int EvaluateXorGate(int leftValue, int rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            int layerIndex = Math.Max(leftValue, rightValue);
            GetLayerStatisticsBuilder(layerIndex).AddLinearGate();
            UpdateContext(circuitContext);
            return layerIndex;
        }

        public int EvaluateNotGate(int value, GateContext gateContext, CircuitContext circuitContext)
        {
            int layerIndex = value;
            GetLayerStatisticsBuilder(layerIndex).AddLinearGate();
            UpdateContext(circuitContext);
            return layerIndex;
        }

        private CircuitLayerStatisticsBuilder GetLayerStatisticsBuilder(int layerIndex)
        {
            while (_layerStatistics.Count <= layerIndex)
                _layerStatistics.Add(new CircuitLayerStatisticsBuilder());
            
            return _layerStatistics[layerIndex];
        }

        private void UpdateContext(CircuitContext context)
        {
            if (_context == null)
                _context = context;
        }

        private CircuitStatistics CreateStatistics()
        {
            int numberOfInputs = 0;
            int numberOfOutputs = 0;

            if (_context != null)
            {
                numberOfInputs = _context.NumberOfInputGates;
                numberOfOutputs = _context.NumberOfOutputGates;
            }
            
            return new CircuitStatistics(_layerStatistics.Select(builder => builder.Create()).ToList(), numberOfInputs, numberOfOutputs);
        }
    }
}
