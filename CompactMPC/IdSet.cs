using System;
using System.Collections.Generic;

namespace CompactMPC
{
    public readonly struct IdSet
    {
        public static readonly IdSet None = new IdSet(0u);
        public static readonly IdSet All = new IdSet(~0u);

        private readonly long _bitMask;

        private IdSet(long bitMask)
        {
            _bitMask = bitMask;
        }

        public static IdSet From(int id)
        {
            return new IdSet(1L << ValidatedId(id));
        }

        public static IdSet From(IEnumerable<int> ids)
        {
            long bitMask = 0L;
            foreach (int id in ids)
                bitMask |= 1L << ValidatedId(id);

            return new IdSet(bitMask);
        }

        public IdSet With(int id)
        {
            return new IdSet(_bitMask | (1L << ValidatedId(id)));
        }

        public IdSet Without(int id)
        {
            return new IdSet(_bitMask & ~(1L << ValidatedId(id)));
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
            return new IdSet(_bitMask | other._bitMask);
        }

        public IdSet IntersectWith(IdSet other)
        {
            return new IdSet(_bitMask & other._bitMask);
        }

        public IdSet Without(IdSet other)
        {
            return new IdSet(_bitMask & ~other._bitMask);
        }

        private static int ValidatedId(int id)
        {
            if (id < 0 || id >= 64)
                throw new ArgumentException("IdSet only supports non-negative identifiers smaller than 64.", nameof(id));

            return id;
        }

        public override bool Equals(object other)
        {
            return other is IdSet otherIdSet && this == otherIdSet;
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
