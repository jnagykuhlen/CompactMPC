using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Expressions
{
    public class InputPrimitiveDeclaration
    {
        private PrimitiveConverter _converter;
        private int _partyId;

        public InputPrimitiveDeclaration(PrimitiveConverter converter, int partyId)
        {
            _converter = converter;
            _partyId = partyId;
        }

        public PrimitiveConverter Converter
        {
            get
            {
                return _converter;
            }
        }

        public int PartyId
        {
            get
            {
                return _partyId;
            }
        }
    }
}
