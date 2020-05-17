using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class TcpTwoPartyNetworkSession : ITwoPartyNetworkSession
    {
        private readonly TcpClient _client;

        private TcpTwoPartyNetworkSession(TcpClient client, Party localParty, Party remoteParty)
        {
            _client = client;
            Channel = new StreamMessageChannel(client.GetStream());
            LocalParty = localParty;
            RemoteParty = remoteParty;
        }

        public static Task<TcpTwoPartyNetworkSession> ConnectLoopbackAsync(Party localParty, int port)
        {
            return ConnectAsync(localParty, new IPEndPoint(IPAddress.Loopback, port));
        }
        
        public static async Task<TcpTwoPartyNetworkSession> ConnectAsync(Party localParty, IPEndPoint endpoint)
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(endpoint.Address, endpoint.Port);
            return CreateFromPartyInformationExchange(localParty, client);
        }
        
        public static Task<TcpTwoPartyNetworkSession[]> ConnectAsync(Party localParty, IPEndPoint[] remoteEndPoints, int numberOfSessions)
        {
            return Task.WhenAll(remoteEndPoints.Take(numberOfSessions).Select(endpoint => ConnectAsync(localParty, endpoint)));
        }

        public static Task<TcpTwoPartyNetworkSession> AcceptLoopbackAsync(Party localParty, int port)
        {
            return AcceptAsync(localParty, new IPEndPoint(IPAddress.Loopback, port));
        }
        
        public static async Task<TcpTwoPartyNetworkSession> AcceptAsync(Party localParty, IPEndPoint localEndPoint)
        {
            return (await AcceptAsync(localParty, localEndPoint, 1)).First();
        }
        
        public static async Task<TcpTwoPartyNetworkSession[]> AcceptAsync(Party localParty, IPEndPoint localEndPoint, int numberOfSessions)
        {
            TcpListener listener = new TcpListener(localEndPoint) { ExclusiveAddressUse = true };
            
            listener.Start();
            try
            {
                TcpTwoPartyNetworkSession[] sessions = new TcpTwoPartyNetworkSession[numberOfSessions];
                for (int i = 0; i < numberOfSessions; ++i)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    sessions[i] = CreateFromPartyInformationExchange(localParty, client);
                }

                return sessions;
            }
            finally
            {
                listener.Stop();
            }
        }

        private static TcpTwoPartyNetworkSession CreateFromPartyInformationExchange(Party localParty, TcpClient client)
        {
            WritePartyInformation(client, localParty);
            Party remoteParty = ReadPartyInformation(client);
            return new TcpTwoPartyNetworkSession(client, localParty, remoteParty);
        }

        private static void WritePartyInformation(TcpClient client, Party party)
        {
            using BinaryWriter writer = new BinaryWriter(client.GetStream(), Encoding.UTF8, true);
            writer.Write(party.Id);
            writer.Write(party.Name);
        }

        private static Party ReadPartyInformation(TcpClient client)
        {
            using BinaryReader reader = new BinaryReader(client.GetStream(), Encoding.UTF8, true);
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            return new Party(id, name);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public IMessageChannel Channel { get; }

        public Party LocalParty { get; }

        public Party RemoteParty { get; }
    }
}
