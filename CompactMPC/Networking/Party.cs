using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class Party
    {
        private int _id;
        private string _name;

        public Party(int id)
        {
            _id = id;
            _name = "Party " + (id + 1);
        }

        public Party(int id, string name)
        {
            _id = id;
            _name = name;
        }

        public override string ToString()
        {
            return String.Format("{0} (id: {1})", _name, _id);
        }

        public override bool Equals(object other)
        {
            Party otherParty = other as Party;
            if (otherParty != null)
                return _id == otherParty.Id && _name == otherParty.Name;

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 321773176;
            hashCode = hashCode * -1521134295 + _id.GetHashCode();
            hashCode = hashCode * -1521134295 + _name.GetHashCode();
            return hashCode;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
