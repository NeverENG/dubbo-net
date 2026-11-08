# Metadata and signature resolution

[Code map](index.md) · [中文](../zh/metadata.md)

[`DubboMetadataManager`](../../Clients/DubboMetadataManager.cs) retrieves and caches service definitions; [`DubboMethodMetadataResolver`](../../Clients/DubboMethodMetadataResolver.cs) chooses a Java signature for an invocation. These files currently live under `Clients/`; this map does not treat that directory placement as a proposed ownership boundary.

**Handoffs:** the manager calls back into [Provider discovery](registry.md) to locate metadata or a Provider Telnet probe. [`DubboClient.CreateMetadataInvocationAsync`](../../Clients/DubboClient.cs) uses the result when Java parameter types need inference. A separate `GetRequiredProtocolAsync` check consults metadata for overload compatibility even when the caller supplied explicit types; those two queries have different failure semantics. Cached metadata may also be refreshed after registry changes.

**Read and verify:** [manager and parsing](../../Clients/DubboMetadataManager.cs) · [Telnet resolver](../../Clients/DubboTelnetMetadataResolver.cs) · [metadata tests](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs) · [metadata contract tests](../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs) · [metadata usage and fallback](../../README.md). See [Native protocol notes (Chinese)](../NativeDubboProtocol.md) for protocol context, not for a second metadata specification.
