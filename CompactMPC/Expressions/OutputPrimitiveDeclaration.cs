namespace CompactMPC.Expressions
{
    public class OutputPrimitiveDeclaration
    {
        public PrimitiveConverter Converter { get; }
        public IdSet PartyIds { get; }
        
        public OutputPrimitiveDeclaration(PrimitiveConverter converter, IdSet partyIds)
        {
            Converter = converter;
            PartyIds = partyIds;
        }
    }
}
