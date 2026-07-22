using System;

namespace CompactMPC.Networking;

public class NetworkConsistencyException(string message) : Exception(message);
