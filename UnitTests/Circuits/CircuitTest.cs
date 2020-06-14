using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.SampleCircuits;
using CompactMPC.UnitTests.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests.Circuits
{
    [TestClass]
    public class CircuitTest
    {
        [TestMethod]
        public void TestCircuitEvaluation()
        {
            BitArray[] inputs = {
                BitArray.FromBinaryString("0111010011"),
                BitArray.FromBinaryString("1101100010"),
                BitArray.FromBinaryString("0111110011")
            };

            Bit[] sequentialInput = inputs.SelectMany(bits => bits).ToArray();

            CircuitBuilder builder = new CircuitBuilder();
            SetIntersectionCircuitRecorder setIntersectionCircuitRecorder =
                new SetIntersectionCircuitRecorder(inputs.Length, inputs[0].Length);
            
            setIntersectionCircuitRecorder.Record(builder);

            Circuit circuit = builder.CreateCircuit();
            ForwardCircuit forwardCircuit = ForwardCircuit.FromCircuit(circuit);

            ICircuitEvaluator<Bit> evaluator = new LocalCircuitEvaluator();
            ReportingBatchCircuitEvaluator<Bit> batchCircuitEvaluator =
                new ReportingBatchCircuitEvaluator<Bit>(new BatchCircuitEvaluator<Bit>(evaluator));

            BitArray lazyEvaluationOutput = new BitArray(circuit.Evaluate(evaluator, sequentialInput));
            BitArray forwardEvaluationOutput = new BitArray(forwardCircuit.Evaluate(batchCircuitEvaluator, sequentialInput));
            BitArray expectedEvaluationOutput = BitArray.FromBinaryString("01010000101100");

            EnumerableAssert.AreEqual(
                expectedEvaluationOutput,
                lazyEvaluationOutput
            );

            EnumerableAssert.AreEqual(
                expectedEvaluationOutput,
                forwardEvaluationOutput
            );

            int[] actualBatchSizes = batchCircuitEvaluator.BatchSizes;
            int[] expectedBatchSizes = { 10, 10, 9, 9, 8 };

            EnumerableAssert.AreEqual(
                expectedBatchSizes,
                actualBatchSizes
            );
        }
    }
}
