using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IStatelessFourChoicesBitObliviousTransfer
    {
        // todo(lumip): we could consider just integrating this into the "generalized" (arbitrary message length)
        //  interfaces. it's mostly a convenience overload and relies on the generalized implementation.
        //  are there optimized 1-bit OT implementations that we need to consider so that splitting this into a
        //  separate interface makes sense?
        Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations);
    }
}
