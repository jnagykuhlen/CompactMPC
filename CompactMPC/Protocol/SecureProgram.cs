using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public abstract class SecureProgram
{
    protected abstract void Compile(ISecureProgramContext context);

    public CompiledSecureProgram Compile(MultiPartySessionDescription sessionDescription)
    {
        var context = new SecureProgramContext(sessionDescription);
        Compile(context);
        return context.CreateCompiledSecureProgram();
    }
}
