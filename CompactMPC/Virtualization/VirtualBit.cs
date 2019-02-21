using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Virtualization
{
    public class VirtualBit
    {
        public static readonly VirtualBit Zero = new VirtualBit(-1);
        public static readonly VirtualBit One = new VirtualBit(-2);
        
        private int _id;

        private VirtualBit(int id)
        {
            _id = id;
        }

        public static VirtualBit FromId(int id)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            return new VirtualBit(id);
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
