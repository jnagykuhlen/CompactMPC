using System;

namespace CompactMPC.Protocol;

public class ProtocolException(string message) : Exception(message);
