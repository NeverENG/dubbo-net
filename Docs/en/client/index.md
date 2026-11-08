# Client: from a method call to a remote Provider

[Code map](../index.md) · [中文](../../zh/client/index.md)

An application knows which Java method it wants to call, but should not have to manage the registry, Provider connections and request dispatch itself. `DubboClient` is the **consumer-side entry point** for that work: it accepts a service, method and arguments, and returns a result. It does not publish .NET services.

## How the request reaches a Provider

Consider `com.foo.JobService#getJob`. With explicit Java parameter types, the client can prepare an invocation directly; without them, it asks [Metadata](../metadata/index.md) to infer the signature. Once it has a `DubboInvocation`, it checks overload/protocol restrictions when needed. For a known service it selects a suite from its driver; on first use it obtains Provider addresses through [discovery](../registry/index.md) and creates a driver. [Service selection](../selection/index.md) then reserves a suite for this call, and [transport](../transport/index.md) sends the request and reads the response.

Provider arrivals and departures trigger a separate refresh callback; they do not issue another `QueryAsync` call. The client retains registry connections, metadata and service drivers across requests, so reuse it within its owning application scope and dispose it at the end.

## Calling it from an application

When the Java signature is known, pass exact types through the public API. The registry address and method below are examples; a reachable Provider is required to run this call:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

Callers receive a `DubboRequestResult` (or a typed overload), not an internal driver or connection. Required inference may raise `DubboMetadataException` before dispatch if no unambiguous signature exists; after dispatch, inspect `QuerySuccess` and the result/error fields. See the [user guide](../../../README.md#quick-start) for other overloads and complete usage.

**Check the implementation:** [DubboClient](../../../Clients/DubboClient.cs), [result model](../../../DubboService/DataModle/DubboRequestResult.cs); [public API tests](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs) and the [fixture guide](../../../Tests/EndToEndTestEnvironment/README.md).
