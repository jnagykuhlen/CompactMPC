namespace CompactMPC.Networking
{
    public class Party
    {
        public int Id { get; }
        public string Name { get; }
        
        public Party(int id)
        {
            Id = id;
            Name = "Party " + (id + 1);
        }

        public Party(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} (id: {Id})";
        }

        public override bool Equals(object? other)
        {
            if (other is Party otherParty)
                return Id == otherParty.Id && Name == otherParty.Name;

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 321773176;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            return hashCode;
        }
    }
}
