using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static CompactMPC.Networking.TcpMultiPartyNetworkSession;

namespace CompactMPC.Networking;

[TestClass]
public class TcpMultiPartyNetworkSessionTest
{
    private static readonly IPEndPoint FirstEndPoint = new(IPAddress.Loopback, 12840);
    private static readonly IPEndPoint SecondEndPoint = new(IPAddress.Loopback, 12841);
    private static readonly IPEndPoint ThirdEndPoint = new(IPAddress.Loopback, 12842);

    private static readonly Party FirstParty = new(0);
    private static readonly Party SecondParty = new(1);
    private static readonly Party ThirdParty = new(2);

    [TestMethod]
    public async Task TestTcpMultiPartyNetworkSession()
    {
        var firstSessionTask = Delay(0, () => EstablishAsync(FirstParty, FirstEndPoint, [SecondEndPoint, ThirdEndPoint]));
        var secondSessionTask = Delay(200, () => EstablishAsync(SecondParty, SecondEndPoint, [FirstEndPoint, ThirdEndPoint]));
        var thirdSessionTask = Delay(400, () => EstablishAsync(ThirdParty, ThirdEndPoint, [FirstEndPoint, SecondEndPoint]));

        using var firstSession = await firstSessionTask;
        using var secondSession = await secondSessionTask;
        using var thirdSession = await thirdSessionTask;

        firstSession.LocalParty.Should().Be(FirstParty);
        firstSession.RemotePartySessions.Select(session => session.RemoteParty)
            .Should()
            .BeEquivalentTo([SecondParty, ThirdParty]);

        secondSession.LocalParty.Should().Be(SecondParty);
        secondSession.RemotePartySessions.Select(session => session.RemoteParty)
            .Should()
            .BeEquivalentTo([FirstParty, ThirdParty]);

        thirdSession.LocalParty.Should().Be(ThirdParty);
        thirdSession.RemotePartySessions.Select(session => session.RemoteParty)
            .Should()
            .BeEquivalentTo([FirstParty, SecondParty]);
    }

    private static async Task<T> Delay<T>(int millisecondsDelay, Func<Task<T>> taskFactory)
    {
        await Task.Delay(millisecondsDelay);
        return await taskFactory();
    }
}
