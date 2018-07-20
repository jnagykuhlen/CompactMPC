using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class Wire
    {
        public static readonly Wire Zero = new Wire(-1);
        public static readonly Wire One = new Wire(-2);

        private int _id;

        private Wire(int id)
        {
            _id = id;
        }

        public static Wire FromId(int id)
        {
            if (id < 0)
                throw new ArgumentException("Wire identifier must be non-negative.", nameof(id));

            return new Wire(id);
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public bool IsConstant
        {
            get
            {
                return _id < 0;
            }
        }
    }
}
