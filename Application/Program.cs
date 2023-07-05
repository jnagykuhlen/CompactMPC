using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
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
        
        private static readonly BitArray[] Inputs = {
            BitArray.FromBinaryString("0111010011"),
            BitArray.FromBinaryString("1101100010"),
            BitArray.FromBinaryString("0111110011")
        };

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                RunCoordinator();
            }
            else if (args.Length == 1)
            {
                int localPartyId = int.Parse(args[0]);
                RunSecureComputationParty(localPartyId, Inputs[localPartyId]);
            }
            else
            {
                Console.WriteLine("Invalid number of arguments.");
            }
        }

        private static void RunCoordinator()
        {
            Console.WriteLine("Starting parties...");

            string executablePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            Process[] processes = Enumerable
                .Range(0, NumberOfParties)
                .Select(partyId => StartProcess(executablePath, partyId.ToString()))
                .ToArray();
            
            Console.WriteLine("Successfully started parties.");
            Console.WriteLine("Waiting for parties to finish computation...");
            
            foreach (Process process in processes)
                process.WaitForExit();
            
            Console.WriteLine("Computation finished.");
        }

        private static void RunSecureComputationParty(int localPartyId, BitArray localInput)
        {
            RunSecureComputationPartyAsync(localPartyId, localInput).Wait();
        }
        
        private static async Task RunSecureComputationPartyAsync(int localPartyId, BitArray localInput)
        {
            Console.WriteLine($"Starting party {localPartyId}...");
            
            IPEndPoint[] endpoints = Enumerable
                .Range(StartPort, NumberOfParties)
                .Select(port => IPAddress.Loopback.BoundToPort(port))
                .ToArray();
            
            using IMultiPartyNetworkSession session = await TcpMultiPartyNetworkSession.EstablishAsync(
                new Party(localPartyId),
                endpoints
            );

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
            object[] outputPrimitives = await secureProgram.EvaluateAsync(computation, new object[] {localInput});
            BitArray intersection = (BitArray) outputPrimitives[0];
            BigInteger count = (BigInteger) outputPrimitives[1];

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

        private static Process StartProcess(string fileName, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments) { UseShellExecute = true };
            return Process.Start(startInfo)!;
        }
    }
}
