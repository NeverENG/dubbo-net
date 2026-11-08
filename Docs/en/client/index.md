# Starting a call with `DubboClient`

[Code map](../index.md) · [中文](../../zh/client/index.md)

The application supplies a service, method and arguments; `DubboClient` coordinates Provider discovery, connection selection and dispatch. It is a **consumer-side entry point**, not a .NET service publisher. Reuse a client within its owning application scope and dispose it at the end, rather than creating one per request.

## Where the request goes

Consider `com.foo.JobService#getJob`: if you know its Java parameter types, pass them to `QueryAsync`. Otherwise the client asks [Metadata](../metadata/index.md) to infer a signature. It may then check overload/protocol compatibility, get Provider addresses from [Registry](../registry/index.md), choose and reserve a suite via [Selection](../selection/index.md), and hand the call to [Transport](../transport/index.md). Address changes follow a separate refresh path; they do not invoke the public call again.

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

This assumes a reachable registry and Provider. Callers receive `DubboRequestResult` (or its typed version), not an internal driver or connection. If required inference cannot find an unambiguous signature, `DubboMetadataException` may be raised before dispatch. See the [user guide](../../../README.md#quick-start) for other overloads and result fields.

**Follow the code:** [DubboClient](../../../Clients/DubboClient.cs) → [result model](../../../DubboService/DataModle/DubboRequestResult.cs); [public API tests](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs) cover overload behavior, and the [fixture guide](../../../Tests/EndToEndTestEnvironment/README.md) shows how to run the Java Provider.
