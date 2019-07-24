using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPC.SampleCircuits
{
    /// <summary>
    /// Circuit which computes the intersection of sets given in a binary word representation.
    /// </summary>
    /// <remarks></remarks>
    /// In addition to the intersection result, a counter giving the cardinality of the intersection
    /// is also calculated.
    /// </remarks>
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

        /// <summary>
        /// The binary encoding of the intersection set.
        /// </summary>
        public SecureWord Intersection
        {
            get
            {
                return _intersection;
            }
        }

        /// <summary>
        /// The number of elements in the intersection set.
        /// </summary>
        public SecureInteger Counter
        {
            get
            {
                return _counter;
            }
        }
    }
}
