namespace CompactMPC.Circuits
{
    /// <summary>
    /// Represents an algorithm in boolean logic that can be recorded into a circuit builder.
    /// </summary>
    public interface ICircuitRecorder
    {
        /// <summary>
        /// Records an algorithm in boolean logic into a circuit builder instance.
        /// </summary>
        /// <param name="builder">Circuit builder to record into.</param>
        void Record(CircuitBuilder builder);
    }
}
