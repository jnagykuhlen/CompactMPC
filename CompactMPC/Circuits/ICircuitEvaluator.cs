using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Represents an abstract evaluator for boolean circuits that implements handlers for each gate traversed
    /// during evaluation.
    /// </summary>
    /// <typeparam name="TInteral">The data type that represents actual wire values during evaluation.</typeparam>
    public interface ICircuitEvaluator<TIn, TProcess, TOut>
    {
        /// <summary>
        /// Evaluates an AND gate given the values assigned to its left and right input wire.
        /// </summary>
        /// <param name="leftValue">Value assigned to the left input wire.</param>
        /// <param name="rightValue">Value assigned to the right input value.</param>
        /// <param name="gateContext">Information on the identity of the currently evaluated gate.</param>
        /// <param name="circuitContext">Information on the number of gates in the evaluated circuit.</param>
        /// <returns>Logical AND applied to the two input values.</returns>
        TProcess EvaluateAndGate(TProcess leftValue, TProcess rightValue, GateContext gateContext, CircuitContext circuitContext);

        /// <summary>
        /// Evaluates an XOR gate given the values assigned to its left and right input wire.
        /// </summary>
        /// <param name="leftValue">Value assigned to the left input wire.</param>
        /// <param name="rightValue">Value assigned to the right input value.</param>
        /// <param name="gateContext">Information on the identity of the currently evaluated gate.</param>
        /// <param name="circuitContext">Information on the number of gates in the evaluated circuit.</param>
        /// <returns>Logical XOR applied to the two input values.</returns>
        TProcess EvaluateXorGate(TProcess leftValue, TProcess rightValue, GateContext gateContext, CircuitContext circuitContext);

        /// <summary>
        /// Evaluates a NOT gate given the value assigned to its input wire.
        /// </summary>
        /// <param name="value">Value assigned to the input wire.</param>
        /// <param name="gateContext">Information on the identity of the currently evaluated gate.</param>
        /// <param name="circuitContext">Information on the number of gates in the evaluated circuit.</param>
        /// <returns>Logical NOT applied to the input value.</returns>
        TProcess EvaluateNotGate(TProcess value, GateContext gateContext, CircuitContext circuitContext);

        TProcess EvaluateInputGate(TIn input, GateContext gateContext, CircuitContext circuitContext);
        TOut EvaluateOutputGate(TProcess value, GateContext gateContext, CircuitContext circuitContext);
    }
}
