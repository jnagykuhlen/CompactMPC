using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Expressions
{
    public class OutputPrimitiveDeclaration
    {
        private PrimitiveConverter _converter;
        private IdSet _partyIds;

        public OutputPrimitiveDeclaration(PrimitiveConverter converter, IdSet partyIds)
        {
            _converter = converter;
            _partyIds = partyIds;
        }

        public PrimitiveConverter Converter
        {
            get
            {
                return _converter;
            }
        }

        public IdSet PartyIds
        {
            get
            {
                return _partyIds;
            }
        }
    }
}
