﻿namespace CompactMPC.Circuits.Batching.Internal
{
    public abstract class UnaryForwardGate : ForwardGate
    {
        public sealed override void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
        {
            Visit(visitor);
            SendVisitingRequest(visitor, visitingState);
        }
        
        protected abstract void Visit(ICircuitVisitor visitor);
    }
}
