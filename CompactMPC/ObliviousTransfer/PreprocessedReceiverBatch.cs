using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public class PreprocessedReceiverBatch
    {
        private QuadrupleIndexArray _selectionIndices;
        private BitArray _selectedOptions;

        public PreprocessedReceiverBatch(QuadrupleIndexArray selectionIndices, BitArray selectedOptions)
        {
            if (selectionIndices.Length != selectedOptions.Length)
                throw new ArgumentException("Number of selection indices and selected options does not match.");

            _selectionIndices = selectionIndices;
            _selectedOptions = selectedOptions;
        }

        public int GetSelectionIndex(int instanceId)
        {
            return _selectionIndices[instanceId];
        }

        public Bit GetSelectedOption(int instanceId)
        {
            return _selectedOptions[instanceId];
        }

        public int NumberOfInstances
        {
            get
            {
                return _selectionIndices.Length;
            }
        }
    }
}
