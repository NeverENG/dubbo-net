# Metadata: resolving Java signatures without an interface JAR

[Code map](../index.md) · [中文](../../zh/metadata/index.md)

If you pass a C# integer to `getJob`, the client still needs the corresponding **Java parameter type** to prepare a `DubboInvocation`. Metadata supplies method and type definitions; it neither holds callable Provider connections nor sends business requests. `DubboMetadataManager` currently lives under `Clients/`. That location describes the code today, not a future ownership rule.

## Where it comes from and where it goes

The client requests a signature when it needs to infer types. The manager uses [discovery](../registry/index.md) to locate Provider information, reads the metadata center, or attempts Provider Telnet under the current strategy. A resolver chooses Java parameter types using the method name and arguments; the client uses them to construct an invocation. Cached definitions may be refreshed after registry or metadata changes. The call may also consult metadata later to decide whether an overloaded method requires `dubbo://`. Explicit types skip **inference**, not necessarily all metadata checks.

To inspect a method's signatures, use the public API rather than constructing the manager:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var methods = await client.GetMethodMetadataAsync(
    "com.foo.JobService#getJob");
```

`GetServiceMethodsMetadataAsync` lists service methods; `GetServiceTypesMetadataAsync` reads type definitions (Telnet cannot supply types). Missing or ambiguous signatures block required inference. In that case provide exact Java types as shown in the [metadata user guide](../../../README.md#metadata-strategy-and-cache); the compatibility check on an explicitly typed call is a separate, best-effort path.

**Follow the code:** [manager](../../../Clients/DubboMetadataManager.cs), [signature resolver](../../../Clients/DubboMethodMetadataResolver.cs), [Telnet fallback](../../../Clients/DubboTelnetMetadataResolver.cs); compare [metadata tests](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs) and [contract tests](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs).
