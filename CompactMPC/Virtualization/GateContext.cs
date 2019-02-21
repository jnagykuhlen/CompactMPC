using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Virtualization
{
    public class GateContext
    {
        private int _id;

        public GateContext(int id)
        {
            _id = id;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }
    }
}
