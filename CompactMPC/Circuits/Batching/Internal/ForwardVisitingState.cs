using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardVisitingState
    {
        private readonly HashSet<ForwardGate> _gatesWithVisitingRequest;

        public ForwardVisitingState()
        {
            _gatesWithVisitingRequest = new HashSet<ForwardGate>();
        }
        
        public void AddVisitingRequest(ForwardGate gate)
        {
            _gatesWithVisitingRequest.Add(gate);
        }

        public void RemoveVisitingRequest(ForwardGate gate)
        {
            _gatesWithVisitingRequest.Remove(gate);
        }

        public bool HasVisitingRequest(ForwardGate gate)
        {
            return _gatesWithVisitingRequest.Contains(gate);
        }
    }
}
