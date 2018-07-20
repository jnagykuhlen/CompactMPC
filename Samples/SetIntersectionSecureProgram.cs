using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC;
using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPCDemo
{
    public class SetIntersectionSecureProgram : SecureMultiPartyProgram
    {
        private const int NumberOfElements = 10;
        private const int NumberOfCounterBits = 4;

        protected override SecurePrimitive[] Run(CircuitBuilder builder, SecurePrimitive[] inputs)
        {
            SecureWord inputA = (SecureWord)inputs[0];
            SecureWord inputB = (SecureWord)inputs[1];
            SecureWord inputC = (SecureWord)inputs[2];

            SecureWord intersection = SecureWord.And(inputA, inputB, inputC);
            SecureInteger counter = SecureInteger.Zero(builder);

            for (int i = 0; i < NumberOfElements; ++i)
                counter = counter + SecureInteger.FromBoolean(intersection.IsBitSet(i));

            counter = counter.OfFixedLength(NumberOfCounterBits);

            return new[]
            {
                intersection,
                counter
            };
        }

        protected override InputPrimitiveDeclaration[] InputDeclaration
        {
            get
            {
                return new[]
                {
                    new InputPrimitiveDeclaration(PrimitiveConverter.Word(NumberOfElements), 0),
                    new InputPrimitiveDeclaration(PrimitiveConverter.Word(NumberOfElements), 1),
                    new InputPrimitiveDeclaration(PrimitiveConverter.Word(NumberOfElements), 2),
                };
            }
        }

        protected override OutputPrimitiveDeclaration[] OutputDeclaration
        {
            get
            {
                return new[]
                {
                    new OutputPrimitiveDeclaration(PrimitiveConverter.Word(NumberOfElements), IdSet.All),
                    new OutputPrimitiveDeclaration(PrimitiveConverter.Integer(NumberOfCounterBits), IdSet.All),
                };
            }
        }
    }
}
