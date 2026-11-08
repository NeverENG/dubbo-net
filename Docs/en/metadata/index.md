# Metadata and method signatures

[Code map](../index.md) · [中文](../../zh/metadata/index.md)

## Purpose and background

A .NET caller can invoke a Java method without its interface JAR, but automatic argument typing still needs the Provider's Java method signatures. [`DubboMetadataManager`](../../../Clients/DubboMetadataManager.cs) obtains and caches service definitions; [`DubboMethodMetadataResolver`](../../../Clients/DubboMethodMetadataResolver.cs) matches a candidate signature to the arguments. The manager currently lives under `Clients/`; that file location is not a proposed ownership boundary.

## Upstream and downstream

The [client](../client/index.md) requests signatures when it must infer Java types; later it may consult metadata again to check whether an overloaded method should be restricted to a native `dubbo://` Provider. The manager first gets Provider information via the [registry discovery callback](../registry/index.md), then reads the metadata center or, where applicable, probes Provider Telnet through the [Telnet resolver](../../../Clients/DubboTelnetMetadataResolver.cs). Refresh events may invalidate cached service definitions. A missing signature blocks required inference, whereas the compatibility check on an explicitly typed invocation is best effort; see the current code for their distinct exception paths.

## What crosses the boundary

- **Input:** service/method identity, and for signature resolution the argument values; Provider attributes help locate metadata or a Telnet endpoint.
- **Output:** method/type metadata and a chosen Java parameter signature, used to prepare the `DubboInvocation`. Method metadata is **not** the list of callable Provider connections; that belongs to the [driver](../selection/index.md).
- **Sources:** metadata-center content and a possible Telnet fallback. The metadata center may reuse the registry connection or have a separate address; neither source sends the business RPC.

## How to use it

Applications inspect metadata through the public client API; they need not instantiate the internal manager:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var methods = await client.GetMethodMetadataAsync(
    "com.foo.JobService#getJob");
```

`GetServiceMethodsMetadataAsync` lists service methods, while `GetServiceTypesMetadataAsync` reads type definitions from the metadata center (Telnet cannot supply types). To call without querying explicitly, use the [client's inferred-type overload](../client/index.md); when a signature is unavailable or ambiguous, pass exact Java types as shown in the [metadata user guide](../../../README.md#metadata-strategy-and-cache). Check [manager tests](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs), [contract tests](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs) and the existing [Native protocol notes (Chinese)](../../NativeDubboProtocol.md) for protocol context.
