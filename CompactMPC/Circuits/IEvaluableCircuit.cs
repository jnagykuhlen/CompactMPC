using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    /// <summary>
    /// Represents an abstract boolean circuit that can be traversed and evaluated.
    /// </summary>
    public interface IEvaluableCircuit
    {
        /// <summary>
        /// Evaluates the circuit using a specified evaluator.
        /// </summary>
        /// <typeparam name="T">The data type that represents actual wire values.</typeparam>
        /// <param name="evaluator">An abstract evaluator that is called for each gate in the circuit.</param>
        /// <param name="inputValues">Input values corresponding to each declared input wire.</param>
        /// <returns>Output bits corresponding to each declared output wire.</returns>
        T[] Evaluate<T>(ICircuitEvaluator<T> evaluator, T[] inputValues);
    }
}
