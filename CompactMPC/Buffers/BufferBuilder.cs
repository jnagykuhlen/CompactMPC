using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Buffers
{
    /// <summary>
    /// Allows to build a byte buffer by composing existing buffers and integer values.
    /// </summary>
    public class BufferBuilder
    {
        private MessageComposer _composer;

        private BufferBuilder()
        {
            _composer = new MessageComposer();
        }

        public static BufferBuilder From(byte[] buffer)
        {
            return new BufferBuilder().With(buffer);
        }

        public BufferBuilder With(byte[] buffer)
        {
            _composer.Write(buffer);
            return this;
        }

        public BufferBuilder With(int value)
        {
            _composer.Write(value);
            return this;
        }

        public byte[] Create()
        {
            return _composer.Compose();
        }
    }
}
