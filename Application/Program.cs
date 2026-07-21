using System;
using System.Threading.Tasks;
using CompactMPC.Expressions;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;

int[] inputs = [5, 6, 3, 4, 5];

await LocalNetworkRunner.RunMultiPartyNetwork(2, PerformSecureComputation);

async Task PerformSecureComputation(IMultiPartyNetworkSession networkSession)
{
    var localInput = inputs[networkSession.LocalParty.Id];

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
    public Input<IntegerExpression> Input { get; } = IntegerExpression.Input(15);
    public Output<IntegerExpression> Output { get; } = IntegerExpression.Output();

    public override void Compile(ISecureProgramContext context)
    {
        var allInputs = context.Share(Input);
        context.Reveal(Output, IntegerExpression.Sum(allInputs));
    }
}
