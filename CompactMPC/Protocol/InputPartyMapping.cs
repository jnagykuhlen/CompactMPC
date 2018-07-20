using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Protocol
{
    public class InputPartyMapping
    {
        private int[] _partyIds;

        public InputPartyMapping(int numberOfInputs)
        {
            _partyIds = new int[numberOfInputs];
        }

        public InputPartyMapping(IEnumerable<int> partyIds)
        {
            _partyIds = partyIds.ToArray();
        }

        public void Assign(int inputId, int partyId)
        {
            _partyIds[inputId] = partyId;
        }

        public void AssignRange(int startInputId, int numberOfInputs, int partyId)
        {
            for (int i = 0; i < numberOfInputs; ++i)
                _partyIds[startInputId + i] = partyId;
        }

        public int GetAssignedParty(int inputId)
        {
            return _partyIds[inputId];
        }

        public IEnumerable<int> GetAssignedInputs(int partyId)
        {
            for(int i = 0; i < _partyIds.Length; ++i)
            {
                if (_partyIds[i] == partyId)
                    yield return i;
            }
        }

        public int NumberOfInputs
        {
            get
            {
                return _partyIds.Length;
            }
        }
    }
}
