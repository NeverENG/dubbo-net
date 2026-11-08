# Protocol transports

[Code map](../index.md) · [中文](../../zh/transport/index.md)

## Purpose and background

After a Provider has been selected, a transport sends the invocation to that **remote** Provider and reads its response. The `DubboService/` directory contains consumer-side actuators despite its name; it is not a .NET Provider-server entry point. Native Dubbo TCP and Triple HTTP/JSON have different wire formats, so protocol detail stays in the existing notes rather than being redefined here.

## Upstream and downstream

The [service driver](../selection/index.md) chooses an actuator from the Provider URL scheme. [`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs) dispatches a `DubboInvocation` to the selected suite. For `dubbo://`, [`NativeDubboActuatorSuite`](../../../DubboService/NativeDubboActuatorSuite.cs) uses [`NativeDubboConnection`](../../../DubboService/Native/NativeDubboConnection.cs) and [`NativeDubboCodec`](../../../DubboService/Native/NativeDubboCodec.cs). For `tri://`, [`TripleDubboActuatorSuite`](../../../DubboService/TripleDubboActuatorSuite.cs) sends HTTP/JSON. After dispatch returns or throws, the [per-call reservation](../selection/index.md) is completed.

## What crosses the boundary

- **Input:** a selected Provider connection/suite and an invocation containing service, method, exact Java parameter types and arguments. Direct actuator usage does not infer signatures or discover Providers.
- **Output:** a `DubboRequestResult` with success status, response data and error information. Different transport formats must preserve the public result contract; see the [result model](../../../DubboService/DataModle/DubboRequestResult.cs).
- **Connection:** each suite owns its transport resources according to its implementation; selection and in-flight reservation determine when a shared suite can be retired.

## How to use it

Normally use the [client API](../client/index.md) so discovery and selection happen for you. When the Provider address and Java signature are already known, direct native invocation is possible:

```csharp
using DubboNet.DubboService;
using DubboNet.DubboService.DataModle;

using var suite = new NativeDubboActuatorSuite("127.0.0.1", 20880);
var invocation = new DubboInvocation(
    "com.foo.JobService", "getJob",
    new[] { "java.lang.Integer" }, new object[] { 1234 });
var result = await suite.SendQuery(invocation);
```

This bypasses the registry, load balancing and metadata inference. For direct Triple usage see the [user guide](../../../README.md#direct-actuator-usage). For wire and compatibility detail follow [Native protocol notes (Chinese)](../../NativeDubboProtocol.md) or [Triple protocol notes (Chinese)](../../TripleProtocol.md); verify with [Native codec tests](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs), [Triple transport tests](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs) and the [real-fixture guide](../../../Tests/EndToEndTestEnvironment/README.md).
