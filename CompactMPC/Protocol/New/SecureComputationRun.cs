using System;
using System.Threading.Tasks;
using CompactMPC.ExpressionsNew;

namespace CompactMPC.Protocol.New;

public class SecureComputationRun<TProgram>(SecretSharingSecureComputation secureComputation, TProgram program)
    where TProgram : SecureProgram
{
    private readonly SecureProgramInput _input = new();

    public SecureComputationRun<TProgram> WithInput<T>(InputLocator<TProgram, T> inputLocator, T value) where T : notnull
    {
        _input.SetValue(inputLocator(program), value);
        return this;
    }

    public Task<SecureProgramOutput> EvaluateOutputsAsync() => secureComputation.RunAsync(program, _input);

    public async Task<T> EvaluateOutputsAsync<T>(Func<SecureProgramOutput, T> outputSelector) =>
        outputSelector(await EvaluateOutputsAsync());

    public Task<T> EvaluateOutputAsync<T>(OutputLocator<TProgram, T> outputLocator) =>
        EvaluateOutputsAsync(secureProgramOutput => secureProgramOutput.Value(outputLocator(program)));

    public Task<(TFirst, TSecond)> EvaluateOutputsAsync<TFirst, TSecond>(
        OutputLocator<TProgram, TFirst> firstOutputLocator,
        OutputLocator<TProgram, TSecond> secondOutputLocator
    ) => EvaluateOutputsAsync(secureProgramOutput => (
        secureProgramOutput.Value(firstOutputLocator(program)),
        secureProgramOutput.Value(secondOutputLocator(program))
    ));

    public Task<(TFirst, TSecond, TThird)> EvaluateOutputsAsync<TFirst, TSecond, TThird>(
        OutputLocator<TProgram, TFirst> firstOutputLocator,
        OutputLocator<TProgram, TSecond> secondOutputLocator,
        OutputLocator<TProgram, TThird> thirdOutputLocator
    ) => EvaluateOutputsAsync(secureProgramOutput => (
        secureProgramOutput.Value(firstOutputLocator(program)),
        secureProgramOutput.Value(secondOutputLocator(program)),
        secureProgramOutput.Value(thirdOutputLocator(program))
    ));
}

public delegate IInput<IInputExpression<T>> InputLocator<in TProgram, in T>(TProgram program)
    where TProgram : SecureProgram
    where T : notnull;

public delegate IOutput<IOutputExpression<T>> OutputLocator<in TProgram, out T>(TProgram program)
    where TProgram : SecureProgram;
