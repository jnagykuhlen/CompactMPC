using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Protocol
{
    public class OutputPartyMapping
    {
        private readonly IdSet[] _partyIds;

        public OutputPartyMapping(int numberOfOutputs)
        {
            _partyIds = new IdSet[numberOfOutputs];
        }

        public OutputPartyMapping(IEnumerable<IdSet> partyIds)
        {
            _partyIds = partyIds.ToArray();
        }
        
        public void Assign(int outputId, IdSet partyIds)
        {
            _partyIds[outputId] = partyIds;
        }

        public void AssignRange(int startOutputId, int numberOfOutputs, IdSet partyIds)
        {
            for (int i = 0; i < numberOfOutputs; ++i)
                _partyIds[startOutputId + i] = partyIds;
        }

        public IdSet GetAssignedParties(int outputId)
        {
            return _partyIds[outputId];
        }

        public IEnumerable<int> GetAssignedOutputs(int partyId)
        {
            for(int i = 0; i < _partyIds.Length; ++i)
            {
                if (_partyIds[i].Contains(partyId))
                    yield return i;
            }
        }

        public int NumberOfOutputs
        {
            get
            {
                return _partyIds.Length;
            }
        }
    }
}
