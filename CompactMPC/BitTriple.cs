using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public struct BitTriple
    {
        private byte _value;

        public BitTriple(Bit x, Bit y, Bit z)
        {
            _value = (byte)((byte)x | ((byte)y << 1) | ((byte)z << 2));
        }

        public Bit X
        {
            get
            {
                return new Bit(_value);
            }
        }

        public Bit Y
        {
            get
            {
                return new Bit((byte)(_value >> 1));
            }
        }

        public Bit Z
        {
            get
            {
                return new Bit((byte)(_value >> 2));
            }
        }
    }
}
