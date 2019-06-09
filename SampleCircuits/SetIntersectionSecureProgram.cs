using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPC.SampleCircuits
{
    public class SetIntersectionSecureProgram : SecureMultiPartyProgram
    {
        private int _numberOfParties;
        private int _numberOfElements;
        private int _numberOfCounterBits;

        public SetIntersectionSecureProgram(int numberOfParties, int numberOfElements)
        {
            _numberOfParties = numberOfParties;
            _numberOfElements = numberOfElements;
            _numberOfCounterBits = CounterHelper.RequiredNumberOfBits(numberOfElements);
        }

        protected override SecurePrimitive[] Run(CircuitBuilder builder, SecurePrimitive[] inputs)
        {
            SecureWord[] inputWords = inputs.Cast<SecureWord>().ToArray();
            SecureSetIntersection setIntersection = new SecureSetIntersection(inputWords, _numberOfCounterBits);
            
            return new[]
            {
                setIntersection.Intersection,
                setIntersection.Counter
            };
        }

        protected override InputPrimitiveDeclaration[] InputDeclaration
        {
            get
            {
                return new[]
                {
                    new InputPrimitiveDeclaration(PrimitiveConverter.Word(_numberOfElements), 0),
                    new InputPrimitiveDeclaration(PrimitiveConverter.Word(_numberOfElements), 1),
                    new InputPrimitiveDeclaration(PrimitiveConverter.Word(_numberOfElements), 2),
                };
            }
        }

        protected override OutputPrimitiveDeclaration[] OutputDeclaration
        {
            get
            {
                return new[]
                {
                    new OutputPrimitiveDeclaration(PrimitiveConverter.Word(_numberOfElements), IdSet.All),
                    new OutputPrimitiveDeclaration(PrimitiveConverter.Integer(_numberOfCounterBits), IdSet.All),
                };
            }
        }
    }
}
