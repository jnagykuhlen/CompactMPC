using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public class PreprocessedReceiverBatch
    {
        private byte[] _packedSelectionIndices;
        private BitArray _packedSelectedOptions;
        private int _numberOfInstances;

        public PreprocessedReceiverBatch(int[] selectionIndices, BitArray selectedOptions)
        {
            if (selectionIndices.Length != selectedOptions.Length)
                throw new ArgumentException("Number of selection indices and selected options does not match.");
            
            _packedSelectionIndices = new byte[(selectionIndices.Length - 1)  / 4 + 1];
            _packedSelectedOptions = selectedOptions;
            _numberOfInstances = selectionIndices.Length;
            
            for (int j = 0; j < _numberOfInstances; ++j)
            {
                int byteIndex = j / 4;
                int slotIndex = j % 4;

                if (selectionIndices[j] < 0 || selectionIndices[j] >= 4)
                    throw new ArgumentOutOfRangeException("Selection indices must be in the range from 0 to 3.", nameof(selectionIndices));

                _packedSelectionIndices[byteIndex] |= (byte)(selectionIndices[j] << 2 * slotIndex);
            }
        }

        public int GetSelectionIndex(int instanceId)
        {
            if (instanceId < 0 || instanceId >= _numberOfInstances)
                throw new ArgumentOutOfRangeException(nameof(instanceId));

            int byteIndex = instanceId / 4;
            int slotIndex = instanceId % 4;

            return (_packedSelectionIndices[byteIndex] >> 2 * slotIndex) & 3;
        }

        public Bit GetSelectedOption(int instanceId)
        {
            if (instanceId < 0 || instanceId >= _numberOfInstances)
                throw new ArgumentOutOfRangeException(nameof(instanceId));

            return new Bit(_packedSelectedOptions[instanceId]);
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
