using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer;

public class InsecureObliviousTransfer : ObliviousTransfer
{
    protected override Task InternalSendAsync(IMessageChannel channel, Quadruple<Message>[] options, int numberOfInvocations, int numberOfMessageBytes)
    {
        var packedOptions = new Message(4 * numberOfInvocations * numberOfMessageBytes);
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            for (var j = 0; j < 4; ++j)
                packedOptions = packedOptions.Write(options[i][j]);
        }

        return channel.WriteMessageAsync(packedOptions);
    }

    protected override async Task<Message[]> InternalReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
    {
        var packedOptions = await channel.ReadMessageAsync();
        if (packedOptions.Length != 4 * numberOfInvocations * numberOfMessageBytes)
            throw new DesynchronizationException("Received incorrect number of options.");

        var selectedOptions = new Message[numberOfInvocations];
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            for (var j = 0; j < 4; ++j)
            {
                packedOptions = packedOptions.ReadMessage(numberOfMessageBytes, out var option);
                if (j == selectionIndices[i])
                    selectedOptions[i] = option;
            }
        }

        return selectedOptions;
    }
}
