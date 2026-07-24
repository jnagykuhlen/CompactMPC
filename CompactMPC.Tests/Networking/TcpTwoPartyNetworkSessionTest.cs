using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Networking;

[TestClass]
public class TcpTwoPartyNetworkSessionTest
{
    private static readonly Party FirstParty = new();
    private static readonly Party SecondParty = new();

    private static readonly IPEndPoint FirstEndPoint = new(IPAddress.Loopback, 12674);
    private static readonly IPEndPoint SecondEndPoint = new(IPAddress.Loopback, 12675);

    [TestMethod]
    public async Task TestTcpTwoPartyNetworkSession()
    {
        var firstSessionTask = TcpTwoPartyNetworkSession.EstablishAsync(FirstParty, FirstEndPoint, SecondEndPoint);
        var secondSessionTask = TcpTwoPartyNetworkSession.EstablishAsync(SecondParty, SecondEndPoint, FirstEndPoint);

        using var firstSession = await firstSessionTask;
        using var secondSession = await secondSessionTask;

        firstSession.LocalParty.Should().Be(FirstParty);
        firstSession.RemoteParty.Should().Be(SecondParty);

        secondSession.LocalParty.Should().Be(SecondParty);
        secondSession.RemoteParty.Should().Be(FirstParty);
    }
}
