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

        public SecureSetIntersection(SecureWord[] inputs, int numberOfCounterBits)
        {
            _intersection = SecureWord.And(inputs);
            SecureInteger counter = SecureInteger.Zero(_intersection.Builder);

            for (int i = 0; i < _intersection.Length; ++i)
                counter = counter + SecureInteger.FromBoolean(_intersection.IsBitSet(i));

            _counter = counter.OfFixedLength(numberOfCounterBits);
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
