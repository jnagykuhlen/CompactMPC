using CompactMPC.Expressions;

namespace CompactMPC.SampleCircuits
{
    public class SecureSetIntersection
    {
        public SecureWord Intersection { get; }
        public SecureInteger Counter { get; }

        public SecureSetIntersection(SecureWord[] inputs, int numberOfCounterBits)
        {
            Intersection = SecureWord.And(inputs);
            SecureInteger counter = SecureInteger.Zero(Intersection.Builder);

            for (int i = 0; i < Intersection.Length; ++i)
                counter += SecureInteger.FromBoolean(Intersection.IsBitSet(i));

            Counter = counter.OfFixedLength(numberOfCounterBits);
        }
    }
}
