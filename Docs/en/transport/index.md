# Transport: sending the call to a remote Provider

[Code map](../index.md) · [中文](../../zh/transport/index.md)

By this point the client has prepared the method and Java parameter types, and [service selection](../selection/index.md) has chosen a Provider for this call. A transport suite encodes the request, sends it to that **remote** Provider and reads the response. Despite the name `DubboService/`, these suites are consumer-side code, not .NET Provider servers.

## One call, two transports

For a `dubbo://` Provider, the native suite uses a Dubbo TCP connection and codec; `tri://` goes through the Triple HTTP/JSON suite. Each implementation handles its own wire protocol without changing how the application receives a `DubboRequestResult` from `DubboClient`. Suites manage transport resources; the [selection reservation](../selection/index.md) is released at the end of the call, including when sending fails.

Most applications should call the [client API](../client/index.md), not construct a suite. Use a suite directly only if you know the Provider address and exact Java signature and **intend to bypass** registry discovery, metadata inference and load balancing. For native Dubbo:

```csharp
using DubboNet.DubboService;
using DubboNet.DubboService.DataModle;

using var suite = new NativeDubboActuatorSuite("127.0.0.1", 20880);
var invocation = new DubboInvocation(
    "com.foo.JobService", "getJob",
    new[] { "java.lang.Integer" }, new object[] { 1234 });
var result = await suite.SendQuery(invocation);
```

The address and method are examples; a matching Provider is needed to run this call. For direct Triple usage, see the [user guide](../../../README.md#direct-actuator-usage). For wire details, see the [Native protocol notes (Chinese)](../../NativeDubboProtocol.md) and [Triple notes (Chinese)](../../TripleProtocol.md).

**Check the implementation:** [native suite](../../../DubboService/NativeDubboActuatorSuite.cs), [connection](../../../DubboService/Native/NativeDubboConnection.cs), [codec](../../../DubboService/Native/NativeDubboCodec.cs), [Triple suite](../../../DubboService/TripleDubboActuatorSuite.cs); [codec tests](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs) and [Triple tests](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs).
