# Client entry

[Code map](../index.md) · [中文](../../zh/client/index.md)

## Purpose and background

Applications need one long-lived entry point instead of managing registry connections, metadata lookup and Provider connections per call. [`DubboClient`](../../../Clients/DubboClient.cs) exposes `QueryAsync` and coordinates those parts. It is a **consumer-side** entry: the Java Providers in the test environment are remote servers, not services published by DubboNet. This page describes the current responsibilities; it does not claim they should all remain in one class.

## Upstream and downstream

An application supplies a service/method and arguments, optionally exact Java parameter types. Without those types, `QueryWithInferredTypesAsync` or `QueryAsync(DubboInvocation)` consults [Metadata](../metadata/index.md) to prepare a `DubboInvocation`. `QueryInternalAsync` may consult metadata again for overload compatibility, asks [Registry](../registry/index.md) for endpoints when needed, then hands a call to [Selection](../selection/index.md) and [Transport](../transport/index.md). A Provider-change callback follows a separate refresh path into the client; it is not another public `QueryAsync` call.

## What crosses the boundary

- **Input:** a method endpoint or `DubboInvocation` (service, method, Java parameter types, arguments, and optional version/group/attachments).
- **Output:** `DubboRequestResult` or a typed result; callers inspect `QuerySuccess`, result and error information. Metadata inference can raise `DubboMetadataException` before dispatch. See the [result contract](../../../DubboService/DataModle/DubboRequestResult.cs) for the precise fields.
- **Lifetime:** one client holds registry/metadata references and service drivers across calls. Dispose it when the owning application scope ends; the [connection and registration behavior](../../../README.md#configuration) is described in the user guide.

## How to use it

For a known Java signature, pass types explicitly; this example assumes a reachable registry and Provider for the indicated service:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

For automatic type inference, other overloads, and complete result handling, follow the [English README](../../../README.md#quick-start). The [public API tests](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs) clarify which overload receives an array as one argument. For end-to-end calls, see the [environment guide](../../../Tests/EndToEndTestEnvironment/README.md).
