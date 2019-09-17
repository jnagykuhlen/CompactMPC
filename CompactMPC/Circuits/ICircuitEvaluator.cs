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
    /// <typeparam name="T">The data type that represents actual wire values during evaluation.</typeparam>
    public interface ICircuitEvaluator<T>
    {
        /// <summary>
        /// Evaluates an AND gate given the values assigned to its left and right input wire.
        /// </summary>
        /// <param name="leftValue">Value assigned to the left input wire.</param>
        /// <param name="rightValue">Value assigned to the right input value.</param>
        /// <returns>Logical AND applied to the two input values.</returns>
        T EvaluateAndGate(T leftValue, T rightValue);

        /// <summary>
        /// Evaluates an XOR gate given the values assigned to its left and right input wire.
        /// </summary>
        /// <param name="leftValue">Value assigned to the left input wire.</param>
        /// <param name="rightValue">Value assigned to the right input value.</param>
        /// <returns>Logical XOR applied to the two input values.</returns>
        T EvaluateXorGate(T leftValue, T rightValue);

        /// <summary>
        /// Evaluates a NOT gate given the value assigned to its input wire.
        /// </summary>
        /// <param name="value">Value assigned to the input wire.</param>
        /// <returns>Logical NOT applied to the input value.</returns>
        T EvaluateNotGate(T value);
    }
}
