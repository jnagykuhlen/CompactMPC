# AGENTS.md – CompactMPC

## What This Project Is
A lightweight C# library for **Secure Multi-Party Computation (MPC)** using the GMW protocol over boolean circuits with Naor-Pinkas Oblivious Transfer. Multiple parties jointly evaluate a function without revealing their private inputs.

## Architecture Overview

```
Expressions / ExpressionsNew   ← high-level C# API (SecureBoolean, SecureInteger)
        ↓
Circuits / Circuits.New        ← boolean circuit DAG (Circuit, Wire, Gate)
        ↓
Circuits/Batching              ← forward-order optimization (ForwardCircuit)
        ↓
Protocol / Protocol.New        ← GMW secret sharing (SecretSharingSecureComputation)
        ↓
ObliviousTransfer              ← Naor-Pinkas OT for AND gates
        ↓
Networking                     ← async TCP channels (TcpMultiPartyNetworkSession)
```

## Layer Details

- **`CompactMPC/Circuits/`** – Core DAG: `Circuit` → `Gate[]` → `Wire`. `CircuitBuilder` exposes `And()`, `Xor()`, `Not()`, `Or()`. `Wire` can be a constant (`Wire.Zero`/`Wire.One`) or a gate output.
- **`CompactMPC/Circuits/Batching/`** – `ForwardCircuit` topologically sorts a `Circuit` into `ForwardGate[]` for batch AND evaluation. The protocol layer consumes `IBatchEvaluableCircuit`, not `Circuit` directly.
- **`CompactMPC/Expressions/`** – Stable high-level API. `SecureBoolean`, `SecureInteger`, `SecureWord` let callers write arithmetic expressions that compile to circuits. `SecureMultiPartyProgram` wires inputs/outputs to parties.
- **`CompactMPC/Protocol/`** – `SecretSharingSecureComputation` is the entry point: masks inputs with additive XOR shares, batch-evaluates AND gates via OT, unmasks outputs. `InputPartyMapping` / `OutputPartyMapping` declare ownership.
- **`CompactMPC/ObliviousTransfer/`** – `NaorPinkasObliviousTransfer` implements 1-of-4 bit OT; `InsecureObliviousTransfer` is for tests only.
- **`CompactMPC/Networking/`** – `IMessageChannel` (async send/receive of `Message`). `TcpMultiPartyNetworkSession` creates pairwise channels between all parties.

### Ongoing Migration

- **`CompactMPC/ExpressionsNew/`**, **`CompactMPC/Circuits/New/`**, **`CompactMPC/Protocol/New/`** – Experimental next-generation API not yet adopted for production use. The goal is to replace the old `Expressions`, `Circuits`, and `Protocol` folders with a more modern, extensible design. Concrete future steps:
 1. Do not change legacy `Expressions`, `Circuits`, or `Protocol` code; implement new features in the `New/` folders and copy over legacy code as needed."
 2. When new API is fully supported, delete the old `Expressions`, `Circuits`, and `Protocol` folders and rename `New/` to the main folder name.
 3. Consider renaming high-level types "IntegerExpression" → "SecureInteger", "BooleanExpression" → "SecureBoolean", etc.

## Key Types
| Type | Location | Purpose |
|---|---|---|
| `Bit` | `CompactMPC/Bit.cs` | Single-bit value (readonly struct) |
| `BitArray` | `CompactMPC/BitArray.cs` | Packed bit array (8 bits/byte) |
| `BitQuadrupleArray` | `CompactMPC/BitQuadrupleArray.cs` | 4-option OT messages |
| `Circuit` / `CircuitBuilder` | `Circuits/` | Build and hold boolean circuits |
| `ForwardCircuit` | `Circuits/Batching/` | Batch-evaluable form of a circuit |
| `SecureBoolean` / `SecureInteger` | `Expressions/` | C#-operator-friendly secure types |
| `SecretSharingSecureComputation` | `Protocol/` | Run GMW over a network session |

## Build & Test Commands
```powershell
# Build everything
dotnet build CompactMPC.sln

# Run unit tests
dotnet test CompactMPC.Tests/CompactMPC.Tests.csproj

# Run the sample application (starts multi-party set intersection locally)
dotnet run --project Application/Application.csproj
```

## Testing Conventions
- Tests live in `CompactMPC.Tests/`, mirror the main project's folder structure.
- Multi-party tests use `LocalNetworkRunner` (in-process simulation) — see `SecureComputationTest.cs`.
- Use `InsecureObliviousTransfer` (not `NaorPinkasObliviousTransfer`) for unit tests to avoid expensive crypto.
- Framework: MSTest + FluentAssertions.

## Project-Specific Conventions
- **`Secure*`** prefix = high-level cryptographic types exposed to callers.
- **`*Gate`** suffix = concrete `Gate` subclass; internal implementations live in `Circuits/Internal/`.
- **`*Evaluator`** suffix = strategy for evaluating circuits (e.g., `LocalCircuitEvaluator`, `BatchCircuitEvaluator`).
- All network I/O is `async`/`await`; async methods are suffixed `Async`.
- Nullable reference types are enabled; `CS8600`–`CS8653` are treated as errors — always handle nullable returns.
- Target: **.NET 8.0**, C# 12, no external NuGet dependencies beyond BCL.

## Canonical Example
`SampleCircuits/SetIntersectionSecureProgram.cs` shows the full stack: `SecureMultiPartyProgram` → `SecureBoolean` gates → `IBatchEvaluableCircuit` → `SecretSharingSecureComputation`. Study this before adding new secure programs.

