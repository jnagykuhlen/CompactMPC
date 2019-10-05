using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.SampleCircuits;

namespace CompactMPC.UnitTests.Circuits
{
    [TestClass]
    public class CircuitTest
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

            /*
            CircuitBuilder builder = new CircuitBuilder();
            (new SetIntersectionCircuitRecorder(inputs.Length, inputs[0].Length)).Record(builder);
            */

            new SecureSetIntersection();

            Circuit circuit = builder.CreateCircuit();
            ForwardCircuit forwardCircuit = new ForwardCircuit(circuit);

            ICircuitEvaluator<Bit> evaluator = new LocalCircuitEvaluator();
            ReportingBatchCircuitEvaluator<Bit> batchCircuitEvaluator = new ReportingBatchCircuitEvaluator<Bit>(new BatchCircuitEvaluator<Bit>(evaluator));

            BitArray lazyEvaluationOutput = new BitArray(circuit.Evaluate(evaluator, sequentialInput));
            BitArray forwardEvaluationOutput = new BitArray(forwardCircuit.Evaluate(batchCircuitEvaluator, sequentialInput));
            BitArray expectedEvaluationOutput = BitArray.FromBinaryString("01010000101100");

            CollectionAssert.AreEqual(
                expectedEvaluationOutput,
                lazyEvaluationOutput,
                "Incorrect lazy evaluation output {0} (should be {1}).",
                lazyEvaluationOutput.ToBinaryString(),
                expectedEvaluationOutput.ToBinaryString()
            );

            CollectionAssert.AreEqual(
                expectedEvaluationOutput,
                forwardEvaluationOutput,
                "Incorrect forward evaluation output {0} (should be {1}).",
                forwardEvaluationOutput.ToBinaryString(),
                expectedEvaluationOutput.ToBinaryString()
            );

            int[] expectedBatchSizes = new int[] { 10, 10, 9, 9, 8 };

            CollectionAssert.AreEqual(
                expectedBatchSizes,
                batchCircuitEvaluator.BatchSizes,
                "Incorrect batch sizes {0} (should be {1}).",
                String.Join(", ", batchCircuitEvaluator.BatchSizes),
                String.Join(", ", expectedBatchSizes)
            );
        }
    }
}
