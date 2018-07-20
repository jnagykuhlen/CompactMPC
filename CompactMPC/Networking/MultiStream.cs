using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public abstract class MultiStream
    {
        private Stream _innerStream;

        public MultiStream(Stream innerStream)
        {
            _innerStream = innerStream;
        }

        public abstract Stream GetSubstream(int instanceId);

        public void Flush()
        {
            _innerStream.Flush();
        }

        protected Stream InnerStream
        {
            get
            {
                return _innerStream;
            }
        }

        public bool CanWrite
        {
            get
            {
                return _innerStream.CanWrite;
            }
        }

        public bool CanRead
        {
            get
            {
                return _innerStream.CanRead;
            }
        }
    }
}
