using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A random oracle as canonically defined.
    /// 
    /// As per the canonical definition of the random oracle, it produces a random but deterministic output
    /// for each unique query it is invoked on, i.e., the output is unpredictably random over different queries
    /// but providing the same query will always yield the same output sequence.
    /// 
    /// The RandomOracle class also provides a Mask message that masks a given message with the output of the
    /// random oracle for a given query. In that case the query can be seen as a shared key that allows for
    /// masking and unmasking (encryption/decryption) of a message.
    /// 
    /// Naturally, a true random oracle cannot be implemented so the outputs of all implementations of
    /// RandomOracle will not be perfectly random but computationally indistinguishable from true randomness
    /// by the usual cryptographic standards.
    /// </summary>
    public abstract class RandomOracle
    {
        /// <summary>
        /// Supplies the response of the random oracle to the given query.
        /// 
        /// As per the canonical definition of the random oracle, it produces a random but deterministic output
        /// for each unique query, i.e., the output is unpredictably random over different queries but
        /// providing the same query will always yield the same output sequence.
        /// </summary>
        /// <param name="query">The query for the random oracle.</param>
        /// <returns></returns>
        public abstract IEnumerable<byte> Invoke(byte[] query);
        
        /// <summary>
        /// Masks a given message by applying bitwise XOR with the random oracle output stream.
        /// </summary>
        /// <param name="message">The message to be masked with the random oracle output.</param>
        /// <param name="query">The query for the random oracle.</param>
        /// <returns>Byte array containing the masked message.</returns>
        /// <exception cref="ArgumentException">Thrown when the random oracle does not provide enough data to mask the given message.</exception>
        public byte[] Mask(byte[] message, byte[] query)
        {
            byte[] result = new byte[message.Length];
            int index = 0;

            foreach (byte maskByte in Invoke(query).Take(message.Length))
            {
                result[index] = (byte)(message[index] ^ maskByte);
                index++;
            }

            if (index < message.Length)
                throw new ArgumentException("Random oracle invokation does not provide enough data to mask the given message.", nameof(query));
            
            return result;
        }
    }
}
