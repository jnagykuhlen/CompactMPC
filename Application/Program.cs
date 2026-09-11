using System;
using CompactMPC;
using CompactMPC.Application;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;

const int numberOfParties = 3;

string[] inputs =
[
    "111101",
    "110101",
    "010110"
];

Console.WriteLine($"Start this application locally {numberOfParties} times to begin computation.");

var (session, localPartyIndex) = await LocalNetworkRunner.RunMultiPartyNetworkSinglePartyAsync(numberOfParties);

var localInput = BitArray.FromBinaryString(inputs[localPartyIndex]);

var securityParameters = new SecurityParameters(47, 23, 4, 1, 1);
var secureComputation = new SecretSharingSecureComputation(session, securityParameters);

var (intersection, counter) = await secureComputation.Run(new SetIntersectionSecureProgram(localInput.Length))
    .WithInput(program => program.Input, localInput)
    .EvaluateOutputsAsync(
        program => program.IntersectionOutput,
        program => program.CounterOutput
    );

Console.WriteLine($"\nOutput: intersection {intersection}, counter {counter}");
