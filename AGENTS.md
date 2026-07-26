# AGENTS.md – CompactMPC

## What This Project Is
A lightweight C# library for **Secure Multi-Party Computation (MPC)** using the GMW protocol over boolean circuits with Naor-Pinkas Oblivious Transfer. Multiple parties jointly evaluate a function without revealing their private inputs.

## Architecture Overview

```
Protocol.Primitives            ← high-level C# API (SecureBoolean, SecureInteger, SecureBitArray)
        ↓
Protocol                       ← GMW secret sharing (SecretSharingSecureComputation, SecureProgram)
        ↓
Circuits                       ← boolean circuit DAG (Wire, ForwardGate) and batch evaluation
        ↓
ObliviousTransfer              ← Naor-Pinkas OT for AND gates
        ↓
Networking                     ← async TCP channels (TcpMultiPartyNetworkSession)
```

## Layer Details

- **`CompactMPC/Circuits/`** – Boolean circuit DAG built directly from `Wire` static factory methods: `Wire.And()`, `Wire.Xor()`, `Wire.Not()`, `Wire.Or()`. `Wire` is either a constant (`Wire.Zero`/`Wire.One`), an assignable input (`Wire.Assignable()`), or a gate output. `ForwardCircuitEvaluation` drives topological batch evaluation via `IAsyncBatchCircuitEvaluator`. Gate implementations live in `Circuits/Internal/`.
- **`CompactMPC/Protocol/`** – `SecretSharingSecureComputation` is the entry point: masks inputs with additive XOR shares, batch-evaluates AND gates via OT, and unmasks outputs. Programs are defined by subclassing `SecureProgram` and implementing `Compile(ISecureProgramContext)`. `Input<T>` / `Output<T>` declare typed circuit inputs and outputs. The fluent `SecureComputationRun<T>` API wires party inputs and collects outputs.
- **`CompactMPC/Protocol/Primitives/`** – High-level secure types: `SecureBoolean`, `SecureInteger`, `SecureBitArray`. These implement `IExpression` and expose C#-operator-friendly APIs that build the circuit wire DAG transparently.
- **`CompactMPC/ObliviousTransfer/`** – `NaorPinkasObliviousTransfer` implements 1-of-4 bit OT; `InsecureObliviousTransfer` is for tests only.
- **`CompactMPC/Networking/`** – `IMessageChannel` (async send/receive of `Message`). `TcpMultiPartyNetworkSession` creates pairwise channels between all parties.

## Key Types
| Type | Location | Purpose |
|---|---|---|
| `Bit` | `CompactMPC/Bit.cs` | Single-bit value (readonly struct) |
| `BitArray` | `CompactMPC/BitArray.cs` | Packed bit array (8 bits/byte) |
| `BitQuadrupleArray` | `CompactMPC/BitQuadrupleArray.cs` | 4-option OT messages |
| `Wire` | `Circuits/` | Node in the boolean circuit DAG; built via static factory methods |
| `ForwardCircuitEvaluation` | `Circuits/` | Drives topological batch evaluation of the wire DAG |
| `SecureBoolean` / `SecureInteger` / `SecureBitArray` | `Protocol/Primitives/` | C#-operator-friendly secure types |
| `SecureProgram` | `Protocol/` | Base class for defining a secure computation |
| `SecretSharingSecureComputation` | `Protocol/` | Run GMW over a network session |

## Build & Test Commands
```powershell
# Build everything
dotnet build CompactMPC.sln

# Run unit tests
dotnet test CompactMPC.Tests/CompactMPC.Tests.csproj

# Run the sample application (starts multi-party sum locally)
dotnet run --project Application/Application.csproj
```

## Testing Conventions
- Tests live in `CompactMPC.Tests/`, mirror the main project's folder structure.
- Multi-party tests use `LocalNetworkRunner` (in-process simulation) — see `Protocol/` tests.
- Use `InsecureObliviousTransfer` (not `NaorPinkasObliviousTransfer`) for unit tests to avoid expensive crypto.
- Framework: MSTest + FluentAssertions.

## Project-Specific Conventions
- **`Secure*`** prefix = high-level cryptographic types exposed to callers.
- **`*Gate`** suffix = concrete `Gate` subclass; internal implementations live in `Circuits/Internal/`.
- **`*Evaluator`** suffix = strategy for evaluating circuits (e.g., `LocalCircuitEvaluator`, `BatchCircuitEvaluator`).
- All network I/O is `async`/`await`; async methods are suffixed `Async`.
- Nullable reference types are enabled; `CS8600`–`CS8653` are treated as errors — always handle nullable returns.
- Target: **.NET 10.0**, C# 14, no external NuGet dependencies beyond BCL.

## Canonical Example
`Application/Program.cs` shows the full stack: subclass `SecureProgram` → declare `Input<T>`/`Output<T>` → use `SecureInteger`/`SecureBoolean` operators to build the circuit → run via `SecretSharingSecureComputation`. Study this before adding new secure programs.

## Code Style
- NEVER use abbreviations: `message` instead of `msg`, `exception` instead of `ex`.
- Use default C# naming conventions: PascalCase for types, methods, and properties; camelCase for local variables and parameters.