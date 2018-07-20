using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public class PreprocessedSenderBatch
    {
        private BitArray _packedOptions;
        private int _numberOfInstances;

        public PreprocessedSenderBatch(Quadruple<byte[]>[] options)
        {
            _packedOptions = new BitArray(4 * options.Length);
            _numberOfInstances = options.Length;

            for (int j = 0; j < _numberOfInstances; ++j)
            {
                for (int i = 0; i < 4; ++i)
                {
                    byte[] option = options[j][i];
                    if (option.Length != 1 || (option[0] & ~1) != 0)
                        throw new ArgumentException("Options must contain exactly one bit of data.", nameof(options));

                    _packedOptions[4 * j + i] = new Bit(option[0]).Value;
                }
            }
        }

        public BitQuadruple GetOptions(int instanceId)
        {
            if (instanceId < 0 || instanceId >= _numberOfInstances)
                throw new ArgumentOutOfRangeException(nameof(instanceId));

            return new BitQuadruple(_packedOptions, 4 * instanceId);
        }

        public int NumberOfInstances
        {
            get
            {
                return _numberOfInstances;
            }
        }
    }
}
