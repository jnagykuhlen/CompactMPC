using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPC.SampleCircuits
{
    public class SetIntersectionSecureProgram : SecureMultiPartyProgram
    {
        private readonly int _numberOfParties;
        private readonly int _numberOfElements;
        private readonly int _numberOfCounterBits;

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
            
            return new SecurePrimitive[]
            {
                setIntersection.Intersection,
                setIntersection.Counter
            };
        }

        protected override InputPrimitiveDeclaration[] InputDeclaration
        {
            get
            {
                return Enumerable
                    .Range(0, _numberOfParties)
                    .Select(index => new InputPrimitiveDeclaration(PrimitiveConverter.Word(_numberOfElements), index))
                    .ToArray();
            }
        }

        protected override OutputPrimitiveDeclaration[] OutputDeclaration
        {
            get
            {
                return new[]
                {
                    new OutputPrimitiveDeclaration(PrimitiveConverter.Word(_numberOfElements), IdSet.All),
                    new OutputPrimitiveDeclaration(PrimitiveConverter.Integer(_numberOfCounterBits), IdSet.All)
                };
            }
        }
    }
}
