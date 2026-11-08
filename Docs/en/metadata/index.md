# Metadata: knowing the Java signature before a call

[Code map](../index.md) · [中文](../../zh/metadata/index.md)

A .NET caller often does not have the Java interface JAR. Suppose the application passes an integer to `getJob`: the client still needs its **Java parameter type**, and the C# argument alone may not identify the right overload. Metadata answers “which methods does this service expose, and what types do they accept?” It supplies definitions for an invocation; it does not select a business Provider or send the request.

## Where it fits in a call

When the caller has not supplied Java types, `DubboClient` passes the service, method and arguments into metadata handling. `DubboMetadataManager` obtains and caches service definitions; `DubboMethodMetadataResolver` selects a parameter signature. The client uses that signature to build a `DubboInvocation`, then continues to [service selection](../selection/index.md) and [transport](../transport/index.md). **Callers do not need to query metadata themselves first.**

To find definitions, the manager first gets Provider information via [registry discovery](../registry/index.md), then reads the metadata center when appropriate. If it cannot obtain method definitions there, it can try Provider Telnet. The metadata center can share the registry connection or be configured separately. Provider or metadata changes can refresh cached definitions through listeners. Discovering a Provider here is about **finding definitions**; it does not itself create a service driver for business calls.

If a signature needed for inference is unavailable or ambiguous, supply exact Java types; such calls may raise `DubboMetadataException` before dispatch. Explicit types skip inference, but the client may still try metadata to decide whether an overloaded method requires native Dubbo. Failure of this best-effort compatibility check does not make metadata a hard dependency for an explicitly typed call.

## If you want to inspect signatures

Use the public `DubboClient` API; there is no need to construct the internal manager. The address, interface and method below are examples; replace them with a reachable service when running it:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var methods = await client.GetMethodMetadataAsync(
    "com.foo.JobService#getJob");
```

`GetMethodMetadataAsync` returns discovered signatures for this method. Each item's `ParameterTypes` lists Java type names in order, matching the explicit type array passed to `QueryAsync`. If the service metadata is readable but the method is absent, `GetMethodMetadataAsync` returns an empty list.

`GetServiceMethodsMetadataAsync` lists the service's methods; `GetServiceTypesMetadataAsync` reads type definitions and requires a metadata center, since Telnet cannot supply them. See the [metadata user guide](../../../README.md#metadata-strategy-and-cache) for configuration and explicit argument types.

**Check the implementation:** [client entry](../../../Clients/DubboClient.cs), [manager](../../../Clients/DubboMetadataManager.cs), [signature resolver](../../../Clients/DubboMethodMetadataResolver.cs); [metadata tests](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs) and [contract tests](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs). The manager currently lives under `Clients/`; this describes today's layout, not a future ownership rule.
