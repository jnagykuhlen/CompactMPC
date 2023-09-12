using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;

namespace CompactMPC.Application
{
    public static class Program
    {
        private const int NumberOfParties = 3;
        private const int NumberOfElements = 10;
        private const int StartPort = 12348;

        private static readonly BitArray[] Inputs =
        {
            BitArray.FromBinaryString("0111010011"),
            BitArray.FromBinaryString("1101100010"),
            BitArray.FromBinaryString("0111110011")
        };

        private static readonly IPEndPoint[] EndPoints = Enumerable.Range(0, NumberOfParties)
            .Select(partyIndex => new IPEndPoint(IPAddress.Loopback, StartPort + partyIndex))
            .ToArray();

        public static async Task Main()
        {
            Console.WriteLine($"Start this application {NumberOfParties} times to begin computation.");
            
            SessionInfo sessionInfo = await EstablishSessionAsync();
            using IMultiPartyNetworkSession session = sessionInfo.Session;
            
            BitArray localInput = Inputs[sessionInfo.LocalPartyIndex];

            IBitObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                SecurityParameters.CreateDefault768Bit()
            );

            IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                obliviousTransfer
            );

            SecretSharingSecureComputation computation = new SecretSharingSecureComputation(
                session,
                multiplicativeSharing
            );

            Stopwatch stopwatch = Stopwatch.StartNew();

            SetIntersectionSecureProgram secureProgram = new SetIntersectionSecureProgram(NumberOfParties, NumberOfElements);
            object[] outputPrimitives = await secureProgram.EvaluateAsync(computation, new object[] { localInput });
            BitArray intersection = (BitArray)outputPrimitives[0];
            BigInteger count = (BigInteger)outputPrimitives[1];

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine($"Completed protocol as {session.LocalParty.Name} in {stopwatch.ElapsedMilliseconds} ms.");
            Console.WriteLine($"  Local input: {localInput.ToBinaryString()}");
            Console.WriteLine($"  Computed intersection: {intersection.ToBinaryString()}");
            Console.WriteLine($"  Computed number of matches: {count}");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static async Task<SessionInfo> EstablishSessionAsync(int localPartyIndex = 0)
        {
            try
            {
                IPEndPoint localEndPoint = EndPoints[localPartyIndex];
                IPEndPoint[] remoteEndPoints = EndPoints.Without(localEndPoint).ToArray();
                Party localParty = new Party(localPartyIndex);

                Console.WriteLine($"Try listening at {localEndPoint}...");

                TcpMultiPartyNetworkSession session = await TcpMultiPartyNetworkSession.EstablishAsync(
                    localParty,
                    localEndPoint,
                    remoteEndPoints
                );
                
                return new SessionInfo(session, localPartyIndex);
            }
            catch (SocketException socketException)
                when (socketException.SocketErrorCode == SocketError.AddressAlreadyInUse && localPartyIndex + 1 < NumberOfParties)
            {
                Console.WriteLine("  Address already in use");
                return await EstablishSessionAsync(localPartyIndex + 1);
            }
        }
    }

    public class SessionInfo
    {
        public IMultiPartyNetworkSession Session { get; }

        public int LocalPartyIndex { get; }

        public SessionInfo(IMultiPartyNetworkSession session, int localPartyIndex)
        {
            Session = session;
            LocalPartyIndex = localPartyIndex;
        }
    }
}