using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.SampleCircuits;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class ForwardCircuitTest
    {
        [TestMethod]
        public void TestCircuitEvaluation()
        {
            BitArray[] inputs = new BitArray[]
            {
                BitArray.FromBinaryString("0111010011"),
                BitArray.FromBinaryString("1101100010"),
                BitArray.FromBinaryString("0111110011")
            };

            Bit[] sequentialInput = inputs.SelectMany(bits => bits).ToArray();

            CircuitBuilder builder = new CircuitBuilder();
            (new SetIntersectionCircuitRecorder(inputs.Length, inputs[0].Length)).Record(builder);

            Circuit circuit = builder.CreateCircuit();

            BitArray lazyEvaluationOutput = new BitArray(circuit.Evaluate(new LocalCircuitEvaluator(), sequentialInput));
            BitArray forwardEvaluationOutput = new BitArray(new ForwardCircuit(circuit).Evaluate(new LocalCircuitEvaluator(), sequentialInput));
            BitArray expectedEvaluationOutput = BitArray.FromBinaryString("01010000101100");

            Assert.IsTrue(
                Enumerable.SequenceEqual(expectedEvaluationOutput, lazyEvaluationOutput),
                "Incorrect lazy evaluation output {0} (should be {1}).",
                lazyEvaluationOutput.ToBinaryString(),
                expectedEvaluationOutput.ToBinaryString()
            );

            Assert.IsTrue(
                Enumerable.SequenceEqual(expectedEvaluationOutput, forwardEvaluationOutput),
                "Incorrect forward evaluation output {0} (should be {1}).",
                forwardEvaluationOutput.ToBinaryString(),
                expectedEvaluationOutput.ToBinaryString()
            );
        }
    }
}
