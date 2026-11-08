# Where to start with DubboNet

[中文](../zh/index.md) · [User guide](../../README.md)

DubboNet primarily lets a .NET application **call a remote Java Provider**. To follow a `QueryAsync` call, start at the [client entry point](client/index.md), then read its downstream pieces:

1. [Metadata and signatures](metadata/index.md): prepare an invocation when Java parameter types are unknown.
2. [Registry and discovery](registry/index.md): find Providers and track address changes.
3. [Service selection](selection/index.md): choose among Providers and protect in-flight calls.
4. [Protocol transports](transport/index.md): send over Dubbo TCP or Triple and read the result.

This is a **reading route**, not a fixed sequence on every request. A metadata lookup may itself need Provider discovery, and calls to known services need not discover them again each time. Registry changes update state through a separate callback path.

To run code, use the [user guide](../../README.md). For design history, see the [RFC conventions](../RFC.md) and [changelog](../../CHANGELOG.md). The [end-to-end test guide](../../Tests/EndToEndTestEnvironment/README.md) describes the Java Provider fixture, not a .NET server implementation.
