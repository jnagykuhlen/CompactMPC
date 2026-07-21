namespace CompactMPC.Circuits.Internal
{
    public abstract class UnaryForwardGate : ForwardGate
    {
        protected sealed override void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
        {
            Visit(visitor);
            SendVisitingRequest(visitor, visitingState);
        }
        
        protected abstract void Visit(ICircuitVisitor visitor);
    }
}
