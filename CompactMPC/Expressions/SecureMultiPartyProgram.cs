using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Protocol;

namespace CompactMPC.Expressions
{
    public abstract class SecureMultiPartyProgram
    {
        public async Task<object[]> EvaluateAsync(ISecureComputation secureComputation, object[] localInputPrimitives)
        {
            InputPrimitiveDeclaration[] inputDeclaration = InputDeclaration;
            OutputPrimitiveDeclaration[] outputDeclaration = OutputDeclaration;
            
            CircuitBuilder builder = new CircuitBuilder();
            SecurePrimitive[] inputPrimitives = InputPrimitives(builder, inputDeclaration);
            SecurePrimitive[] outputPrimitives = Run(builder, inputPrimitives);
            OutputPrimitives(builder, outputPrimitives);
            
            BitArray outputBuffer = await secureComputation.EvaluateAsync(
                new ForwardCircuit(builder.CreateCircuit()),
                CreateInputMapping(inputDeclaration),
                CreateOutputMapping(outputDeclaration),
                CreateLocalInputBuffer(inputDeclaration, localInputPrimitives, secureComputation.MultiPartySession.LocalParty.Id)
            );

            return CreateLocalOutputPrimitives(outputDeclaration, outputBuffer, secureComputation.MultiPartySession.LocalParty.Id);
        }

        private static BitArray CreateLocalInputBuffer(InputPrimitiveDeclaration[] inputDeclaration, object[] localInputPrimitives, int localPartyId)
        {
            int numberOfLocalInputWires = inputDeclaration
                .Where(declaration => declaration.PartyId == localPartyId)
                .Sum(declaration => declaration.Converter.NumberOfWires);

            BitArray inputBuffer = new BitArray(numberOfLocalInputWires);

            int nextWireId = 0;
            int nextInputId = 0;
            foreach (InputPrimitiveDeclaration declaration in inputDeclaration)
            {
                if (declaration.PartyId == localPartyId)
                {
                    if (nextInputId >= localInputPrimitives.Length)
                        throw new ArgumentException("Not enough local input primitives provided.", nameof(localInputPrimitives));

                    declaration.Converter.WriteInput(localInputPrimitives[nextInputId], inputBuffer, nextWireId);
                    nextWireId += declaration.Converter.NumberOfWires;
                    nextInputId++;
                }
            }

            return inputBuffer;
        }

        private static object[] CreateLocalOutputPrimitives(OutputPrimitiveDeclaration[] outputDeclaration, BitArray outputBuffer, int localPartyId)
        {
            int numberOfLocalOutputPrimitives = outputDeclaration.Count(declaration => declaration.PartyIds.Contains(localPartyId));

            object[] outputPrimitives = new object[numberOfLocalOutputPrimitives];

            int nextWireId = 0;
            int nextOutputId = 0;
            foreach (OutputPrimitiveDeclaration declaration in outputDeclaration)
            {
                if (declaration.PartyIds.Contains(localPartyId))
                {
                    outputPrimitives[nextOutputId] = declaration.Converter.ReadOutput(outputBuffer, nextWireId);
                    nextWireId += declaration.Converter.NumberOfWires;
                    nextOutputId++;
                }
            }

            return outputPrimitives;
        }

        private static InputPartyMapping CreateInputMapping(InputPrimitiveDeclaration[] inputDeclaration)
        {
            InputPartyMapping inputMapping = new InputPartyMapping(inputDeclaration.Sum(declaration => declaration.Converter.NumberOfWires));

            int nextWireId = 0;
            foreach (InputPrimitiveDeclaration declaration in inputDeclaration)
            {
                inputMapping.AssignRange(nextWireId, declaration.Converter.NumberOfWires, declaration.PartyId);
                nextWireId += declaration.Converter.NumberOfWires;
            }

            return inputMapping;
        }

        public static OutputPartyMapping CreateOutputMapping(OutputPrimitiveDeclaration[] outputDeclaration)
        {
            OutputPartyMapping outputMapping = new OutputPartyMapping(outputDeclaration.Sum(declaration => declaration.Converter.NumberOfWires));

            int nextWireId = 0;
            foreach (OutputPrimitiveDeclaration declaration in outputDeclaration)
            {
                outputMapping.AssignRange(nextWireId, declaration.Converter.NumberOfWires, declaration.PartyIds);
                nextWireId += declaration.Converter.NumberOfWires;
            }

            return outputMapping;
        }

        private static SecurePrimitive[] InputPrimitives(CircuitBuilder builder, InputPrimitiveDeclaration[] inputDeclaration)
        {
            SecurePrimitive[] primitives = new SecurePrimitive[inputDeclaration.Length];
            for (int i = 0; i < primitives.Length; ++i)
            {
                PrimitiveConverter converter = inputDeclaration[i].Converter;
                primitives[i] = converter.FromWires(builder, InputWires(builder, converter.NumberOfWires));
            }

            return primitives;
        }

        private static void OutputPrimitives(CircuitBuilder builder, SecurePrimitive[] primitives)
        {
            for (int i = 0; i < primitives.Length; ++i)
                OutputWires(builder, primitives[i].Wires);
        }

        private static IEnumerable<Wire> InputWires(CircuitBuilder builder, int numberOfWires)
        {
            for (int i = 0; i < numberOfWires; ++i)
                yield return builder.Input();
        }

        private static void OutputWires(CircuitBuilder builder, IEnumerable<Wire> wires)
        {
            foreach (Wire wire in wires)
                builder.Output(wire);
        }

        protected abstract SecurePrimitive[] Run(CircuitBuilder builder, SecurePrimitive[] inputs);
        protected abstract InputPrimitiveDeclaration[] InputDeclaration { get; }
        protected abstract OutputPrimitiveDeclaration[] OutputDeclaration { get; }
    }
}
