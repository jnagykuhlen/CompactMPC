using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPC.SampleCircuits
{
    public class SecureSetIntersection
    {
        private SecureWord _intersection;
        private SecureInteger _counter;

        public SecureSetIntersection(SecureWord[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            if (inputs.Length == 0)
                throw new ArgumentException("Secure set intersection requires at least one input set.", nameof(inputs));

            _intersection = SecureWord.And(inputs);
            SecureInteger counter = SecureInteger.Zero;

            for (int i = 0; i < _intersection.Length; ++i)
                counter = counter + SecureInteger.FromBoolean(_intersection.IsBitSet(i));

            int numberOfElements = inputs[0].Length;
            int numberOfCounterBits = RequiredNumberOfBits(numberOfElements);

            _counter = counter.OfFixedLength(numberOfCounterBits);
        }

        private static int RequiredNumberOfBits(int maximumValue)
        {
            int numberOfBits = 1;
            while ((1 << numberOfBits) <= maximumValue)
                numberOfBits++;

            return numberOfBits;
        }

        public SecureWord Intersection
        {
            get
            {
                return _intersection;
            }
        }

        public SecureInteger Counter
        {
            get
            {
                return _counter;
            }
        }
    }
}
