using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Networking.Internal;

namespace CompactMPC.Networking
{
    public class MessageComposer
    {
        private const int DefaultExpectedNumberOfComponents = 4;

        private List<IMessageComponent> _components;
        private int _length;

        public MessageComposer()
            : this(DefaultExpectedNumberOfComponents) { }

        public MessageComposer(int expectedNumberOfComponents)
        {
            _components = new List<IMessageComponent>(expectedNumberOfComponents);
            _length = 0;
        }

        public void Write(byte[] buffer)
        {
            AddComponent(new BufferMessageComponent(buffer));
        }

        public void Write(int value)
        {
            AddComponent(new IntMessageComponent(value));
        }

        private void AddComponent(IMessageComponent component)
        {
            _components.Add(component);
            _length += component.Length;
        }

        public byte[] Compose()
        {
            byte[] messageBuffer = new byte[_length];

            int offset = 0;
            foreach (IMessageComponent component in _components)
            {
                component.WriteToBuffer(messageBuffer, offset);
                offset += component.Length;
            }

            return messageBuffer;
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }
    }
}
