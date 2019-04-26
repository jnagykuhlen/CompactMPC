using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public class PreprocessedSenderBatch
    {
        private BitQuadrupleArray _options;

        public PreprocessedSenderBatch(BitQuadrupleArray options)
        {
            _options = options;
        }

        public BitQuadruple GetOptions(int instanceId)
        {
            return _options[instanceId];
        }

        public int NumberOfInstances
        {
            get
            {
                return _options.Length;
            }
        }
    }
}
