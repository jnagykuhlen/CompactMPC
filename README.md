# CompactMPC

## Introduction

CompactMPC is a lightweight library for *secure multi-party computation* (MPC), fully written in managed C# code with no native dependencies. It targets .NET Standard 1.3 and can therefore be easily deployed on different operating systems.

MPC is a cryptographic technique for collaboratively computing a function with a number of parties, in a way that the inputs provided by individual participants remain private. This library implements the GMW protocol [1] which is a generic protocol for secure *n*-party computation in the semi-honest setting relying on functions modelled as boolean circuits. The necessary *oblivious transfer* (OT) primitives follow the construction by Naor and Pinkas [2].

## Features

This library features a fully functional protocol for secure multi-party computation with an arbitrary number of participants. The main goal of this project is to provides a clean and extensible API that easily allows to tweak security parameters and exchange individual components, such as the oblivious transfer protocol, the underlying network layer, or even the MPC protocol itself. Securely evaluated functions can be specified directly in C# code using a rich toolset of secure data types including booleans, integers and bit arrays.

In contrast to large, extensive MPC frameworks such as [SCAPI](https://github.com/cryptobiu/libscapi) and [SPDZ](https://github.com/bristolcrypto/SPDZ-2), this library focuses on being a lightweight alternative due to the following decisions made:
- No separate language or proprietary file format for specifying circuits. Instead, boolean circuits are automatically constructed from logic expressed directly in C# code. As a result, it is incredibly easy to get started and to make applications ready for the use of MPC.
- No native code. The project is to hundred percent written in C# and only references .NET Standard libraries. This makes prototyping and deployment on different operating systems an extremely straightforward task.
- Well-defined scope. Instead of exposing a wide range of different protocols to the user, this library includes exactly one protocol for MPC and OT that can be parameterized if necessary. Therefore, the user is not required to learn about many underlying cryptographic details in order to make an informed decision. However, the API comes with all necessary abstractions that allow users to add their own protocols on different levels.
- Maintainability before performance. Existing MPC frameworks often tend to include all possible optimizations at the expense of a clean code structure and maintainability. Although this library might not be on par with existing solutions performance-wise, there are probably many application areas in which a clean codebase is favored over raw speed. This is especially true when it comes to prototyping.

## Getting Started

A small sample application can be found in the *Samples* project in this repository. It shows how to define and collaboratively evaluate a secure program that computes the intersection of sets provided by each party and also counts the number of matches.

Additional samples and further documentation are planned to be added in the future.

## License

This project is published under the [MIT license](/LICENSE).

## References

[1] Oded Goldreich, Silvio Micali, Avi Wigderson: How to Play any Mental Game or A Completeness Theorem for Protocols with Honest Majority. 1987.

[2] Moni Naor and Benny Pinkas: Computationally Secure Oblivious Transfer. 2005.
