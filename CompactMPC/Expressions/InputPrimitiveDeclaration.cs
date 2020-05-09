namespace CompactMPC.Expressions
{
    public class InputPrimitiveDeclaration
    {
        public PrimitiveConverter Converter { get; }
        public int PartyId { get; }
        
        public InputPrimitiveDeclaration(PrimitiveConverter converter, int partyId)
        {
            Converter = converter;
            PartyId = partyId;
        }
    }
}
