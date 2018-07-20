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

        public Party(int id, string name)
        {
            _id = id;
            _name = name;
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
