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
            
            SecurePrimitive[] inputPrimitives = InputPrimitives(inputDeclaration);
            SecurePrimitive[] outputPrimitives = Run(inputPrimitives);

            Wire[] inputWires = inputPrimitives.SelectMany(primitive => primitive.Wires).ToArray();
            Wire[] outputWires = outputPrimitives.SelectMany(primitive => primitive.Wires).ToArray();

            BitArray outputBuffer = await secureComputation.EvaluateAsync(
                new ForwardCircuit(inputWires, outputWires),
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

        private static SecurePrimitive[] InputPrimitives(InputPrimitiveDeclaration[] inputDeclaration)
        {
            SecurePrimitive[] primitives = new SecurePrimitive[inputDeclaration.Length];
            for (int i = 0; i < primitives.Length; ++i)
            {
                PrimitiveConverter converter = inputDeclaration[i].Converter;
                primitives[i] = converter.FromWires(InputWires(converter.NumberOfWires));
            }

            return primitives;
        }

        private static Wire[] InputWires(int numberOfWires)
        {
            Wire[] wires = new Wire[numberOfWires];
            for (int i = 0; i < numberOfWires; ++i)
                wires[i] = Wire.CreateAssignable();

            return wires;
        }

        protected abstract SecurePrimitive[] Run(SecurePrimitive[] inputs);
        protected abstract InputPrimitiveDeclaration[] InputDeclaration { get; }
        protected abstract OutputPrimitiveDeclaration[] OutputDeclaration { get; }
    }
}
