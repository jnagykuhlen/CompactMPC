using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public static class LocalNetworkRunner
    {
        private const int DefaultStartPort = 16741;

        public static async Task RunMultiPartyNetwork(int numberOfParties, Func<IMultiPartyNetworkSession, Task> perPartyAction, int startPort = DefaultStartPort)
        {
            IEnumerable<Task<TcpMultiPartyNetworkSession>> sessionTasks = Enumerable
                .Range(0, numberOfParties)
                .Select(partyId => TcpMultiPartyNetworkSession.EstablishLoopbackAsync(new Party(partyId), startPort, numberOfParties));
            
            await RunAndDispose(sessionTasks, perPartyAction);
        }
        
        public static async Task RunTwoPartyNetwork(Func<ITwoPartyNetworkSession, Task> perPartyAction, int startPort = DefaultStartPort)
        {
            Party firstParty = new Party(0);
            Party secondParty = new Party(1);

            IPEndPoint firstEndPoint = new IPEndPoint(IPAddress.Loopback, startPort);
            IPEndPoint secondEndPoint = new IPEndPoint(IPAddress.Loopback, startPort + 1);
            
            Task<TcpTwoPartyNetworkSession>[] sessionTasks =
            {
                TcpTwoPartyNetworkSession.EstablishAsync(firstParty, firstEndPoint, secondEndPoint),
                TcpTwoPartyNetworkSession.EstablishAsync(secondParty, secondEndPoint, firstEndPoint)
            };

            await RunAndDispose(sessionTasks, perPartyAction);
        }

        private static async Task RunAndDispose<T>(IEnumerable<Task<T>> sessionTasks, Func<T, Task> perPartyAction)
            where T : IDisposable
        {
            T[] sessions = await Task.WhenAll(sessionTasks);
            await Task.WhenAll(sessions.Select(perPartyAction));
            foreach (T session in sessions)
            {
                session.Dispose();
            }
        }
    }
}
