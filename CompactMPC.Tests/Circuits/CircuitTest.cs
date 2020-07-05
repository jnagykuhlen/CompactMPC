using System.Linq;
using CompactMPC.Circuits.Batching;
using CompactMPC.SampleCircuits;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Circuits
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
            
            BitArray expectedOutput = BitArray.FromBinaryString("01010000101100");

            Bit[] sequentialInput = inputs.SelectMany(bits => bits).ToArray();

            CircuitBuilder builder = new CircuitBuilder();
            SetIntersectionCircuitRecorder setIntersectionCircuitRecorder =
                new SetIntersectionCircuitRecorder(inputs.Length, inputs[0].Length);
            
            setIntersectionCircuitRecorder.Record(builder);

            Circuit circuit = builder.CreateCircuit();
            ForwardCircuit forwardCircuit = new ForwardCircuit(circuit);

            ICircuitEvaluator<Bit> evaluator = new LocalCircuitEvaluator();
            ReportingBatchCircuitEvaluator<Bit> batchCircuitEvaluator =
                new ReportingBatchCircuitEvaluator<Bit>(new BatchCircuitEvaluator<Bit>(evaluator));

            circuit.Evaluate(evaluator, sequentialInput).Should().Equal(expectedOutput);
            forwardCircuit.Evaluate(batchCircuitEvaluator, sequentialInput).Should().Equal(expectedOutput);

            batchCircuitEvaluator.BatchSizes.Should().Equal(10, 10, 9, 9, 8);
        }
    }
}
