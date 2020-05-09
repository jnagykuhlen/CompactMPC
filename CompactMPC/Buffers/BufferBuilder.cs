namespace CompactMPC.Buffers
{
    public class BufferBuilder
    {
        private readonly MessageComposer _composer;

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
