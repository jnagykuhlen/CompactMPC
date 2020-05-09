using System.IO;
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

        public static async Task<TcpTwoPartyNetworkSession> ConnectAsync(Party localParty, IPAddress address, int port)
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(address, port);
            return CreateFromPartyInformationExchange(localParty, client);
        }

        public static async Task<TcpTwoPartyNetworkSession> AcceptAsync(Party localParty, int port)
        {
            TcpTwoPartyNetworkSession session;

            TcpListener listener = new TcpListener(IPAddress.Any, port) { ExclusiveAddressUse = true };
            listener.Start();
            session = await AcceptAsync(localParty, listener);
            listener.Stop();

            return session;
        }

        public static async Task<TcpTwoPartyNetworkSession> AcceptAsync(Party localParty, TcpListener listener)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            return CreateFromPartyInformationExchange(localParty, client);
        }

        private static TcpTwoPartyNetworkSession CreateFromPartyInformationExchange(Party localParty, TcpClient client)
        {
            WritePartyInformation(client, localParty);
            Party remoteParty = ReadPartyInformation(client);
            return new TcpTwoPartyNetworkSession(client, localParty, remoteParty);
        }

        private static void WritePartyInformation(TcpClient client, Party party)
        {
            using (BinaryWriter writer = new BinaryWriter(client.GetStream(), Encoding.UTF8, true))
            {
                writer.Write(party.Id);
                writer.Write(party.Name);
            }
        }

        private static Party ReadPartyInformation(TcpClient client)
        {
            using (BinaryReader reader = new BinaryReader(client.GetStream(), Encoding.UTF8, true))
            {
                int id = reader.ReadInt32();
                string name = reader.ReadString();
                return new Party(id, name);
            }
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
