using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public struct IdSet
    {
        public static readonly IdSet None = new IdSet(0u);
        public static readonly IdSet All = new IdSet(~0u);

        private long _bitMask;

        private IdSet(long bitMask)
        {
            _bitMask = bitMask;
        }

        public static IdSet From(int id)
        {
            CheckRange(id);
            return new IdSet(1L << id);
        }

        public static IdSet From(IEnumerable<int> ids)
        {
            long bitMask = 0L;
            foreach (int id in ids)
            {
                CheckRange(id);
                bitMask |= 1L << id;
            }

            return new IdSet(bitMask);
        }

        public IdSet With(int id)
        {
            CheckRange(id);
            return new IdSet(_bitMask | (1L << id));
        }

        public IdSet Without(int id)
        {
            CheckRange(id);
            return new IdSet(_bitMask &= ~(1L << id));
        }

        public bool Contains(int id)
        {
            return (_bitMask & (1L << id)) != 0;
        }

        public bool Contains(IdSet other)
        {
            return (_bitMask & other._bitMask) == other._bitMask;
        }

        public IdSet UnionWith(IdSet other)
        {
            other._bitMask |= _bitMask;
            return other;
        }

        public IdSet IntersectWith(IdSet other)
        {
            other._bitMask &= _bitMask;
            return other;
        }

        public IdSet Without(IdSet other)
        {
            other._bitMask = _bitMask & (~other._bitMask);
            return other;
        }

        private static void CheckRange(int id)
        {
            if (id < 0 || id >= 64)
                throw new ArgumentException("IdSet only supports non-negative identifiers smaller than 64.", nameof(id));
        }

        public override bool Equals(object other)
        {
            return other is IdSet && this == (IdSet)other;
        }
        public override int GetHashCode()
        {
            return _bitMask.GetHashCode();
        }
        public static bool operator ==(IdSet left, IdSet right)
        {
            return left._bitMask == right._bitMask;
        }
        public static bool operator !=(IdSet left, IdSet right)
        {
            return !(left == right);
        }

        public bool IsEmpty
        {
            get
            {
                return _bitMask == 0;
            }
        }
    }
}
