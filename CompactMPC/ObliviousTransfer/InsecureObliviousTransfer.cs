using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class InsecureObliviousTransfer : ObliviousTransfer
    {
        protected override Task GeneralizedSendAsync(IMessageChannel channel, Quadruple<Message>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            Message packedOptions = new Message(4 * numberOfInvocations * numberOfMessageBytes);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                for (int j = 0; j < 4; ++j)
                    packedOptions.Write(options[i][j]);
            }

            return channel.WriteMessageAsync(packedOptions);
        }

        protected override async Task<Message[]> GeneralizedReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            Message packedOptions = await channel.ReadMessageAsync();
            if (packedOptions.Length != 4 * numberOfInvocations * numberOfMessageBytes)
                throw new DesynchronizationException("Received incorrect number of options.");

            Message[] selectedOptions = new Message[numberOfInvocations];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    packedOptions = packedOptions.ReadMessage(numberOfMessageBytes, out Message option);
                    if (j == selectionIndices[i])
                        selectedOptions[i] = option;
                }
            }

            return selectedOptions;
        }
    }
}
