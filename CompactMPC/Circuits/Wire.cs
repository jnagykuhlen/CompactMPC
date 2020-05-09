using System;

namespace CompactMPC.Circuits
{
    public sealed class Wire
    {
        public static readonly Wire Zero = new Wire(null);
        public static readonly Wire One = new Wire(null);

        private readonly Gate? _gate;

        private Wire(Gate? gate)
        {
            _gate = gate;
        }

        public static Wire FromGate(Gate gate)
        {
            return new Wire(gate);
        }

        public Gate Gate
        {
            get
            {
                return _gate ?? throw new NotSupportedException("Constant wires are not associated with a gate.");
            }
        }
    }
}
