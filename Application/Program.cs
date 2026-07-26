using System;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.Protocol.Expressions;

int[] inputs = [5, 6, 3, 4, 5];

await LocalNetworkRunner.RunMultiPartyNetworkAsync(2, PerformSecureComputation);

async Task PerformSecureComputation(IMultiPartyNetworkSession networkSession, int partyIndex)
{
    var localInput = inputs[partyIndex];

    var obliviousTransfer = new NaorPinkasObliviousTransfer(
        new SecurityParameters(47, 23, 4, 1, 1)
    );

    var multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(obliviousTransfer);

    var secureComputation = new SecretSharingSecureComputation(
        networkSession,
        multiplicativeSharing
    );

    var output = await secureComputation.Run(new SumSecureProgram())
        .WithInput(program => program.Input, localInput)
        .EvaluateOutputAsync(program => program.Output);

    Console.WriteLine($"Output: {output}");
}

public class SumSecureProgram : SecureProgram
{
    public Input<SecureInteger> Input { get; } = SecureInteger.Input(15);
    public Output<SecureInteger> Output { get; } = SecureInteger.Output();

    public override void Compile(ISecureProgramContext context)
    {
        var allInputs = context.Share(Input);
        context.Reveal(Output, SecureInteger.Sum(allInputs));
    }
}
