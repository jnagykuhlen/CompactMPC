using System.Collections.Generic;
using CompactMPC.Buffers.Internal;

namespace CompactMPC.Buffers
{
    public class MessageComposer
    {
        private const int DefaultExpectedNumberOfComponents = 4;

        private readonly List<IMessageComponent> _components;
        private int _length;
        
        public MessageComposer(int expectedNumberOfComponents = DefaultExpectedNumberOfComponents)
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
                component.WriteToBuffer(messageBuffer, ref offset);

            return messageBuffer;
        }
    }
}
