namespace CompactMPC
{
    public struct BitTriple
    {
        private readonly byte _value;

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
