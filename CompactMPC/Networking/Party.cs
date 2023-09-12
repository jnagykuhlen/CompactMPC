using System;

namespace CompactMPC.Networking
{
    public class Party
    {
        public int Id { get; }
        public string Name { get; }
        public Guid Guid { get; }

        public Party(int id) : this(id, $"Party {id + 1}", Guid.NewGuid())
        { }

        public Party(int id, string name, Guid guid)
        {
            Id = id;
            Name = name;
            Guid = guid;
        }

        public override string ToString()
        {
            return $"{Name} (id: {Id})";
        }

        public override bool Equals(object? other)
        {
            if (other is Party otherParty)
                return Id == otherParty.Id && Name == otherParty.Name && Guid == otherParty.Guid;

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 321773176;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public static bool operator>(Party first, Party second) {
            return first.Guid.CompareTo(second.Guid) > 0;
        }
        
        public static bool operator<(Party first, Party second) {
            return second > first;
        }
    }
}
