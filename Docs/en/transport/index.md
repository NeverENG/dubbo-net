# Transport: sending to a remote Provider

[Code map](../index.md) · [中文](../../zh/transport/index.md)

At this point the client has prepared the method and Java parameter types and [selected a Provider](../selection/index.md). The suite encodes and sends the call, then reads its response: `dubbo://` uses native Dubbo TCP; `tri://` uses Triple HTTP/JSON. Despite the directory name `DubboService/`, these suites are **consumer-side**, not .NET Provider servers.

`DubboClient` hands the prepared invocation to the selected suite, which manages its own transport resources and returns a `DubboRequestResult`. The in-flight selection reservation must be completed whether dispatch succeeds or fails. Most applications should use the [client API](../client/index.md) rather than establish protocol connections themselves.

If you already know a Provider address and the exact Java signature, you can bypass discovery, inference and load balancing and use a native suite directly:

```csharp
using DubboNet.DubboService;
using DubboNet.DubboService.DataModle;

using var suite = new NativeDubboActuatorSuite("127.0.0.1", 20880);
var invocation = new DubboInvocation(
    "com.foo.JobService", "getJob",
    new[] { "java.lang.Integer" }, new object[] { 1234 });
var result = await suite.SendQuery(invocation);
```

For direct Triple usage, see the [user guide](../../../README.md#direct-actuator-usage). This is not another auto-discovery entry point: the caller supplies the address and signature.

**Follow the code:** [native suite](../../../DubboService/NativeDubboActuatorSuite.cs), [connection](../../../DubboService/Native/NativeDubboConnection.cs), [codec](../../../DubboService/Native/NativeDubboCodec.cs), [Triple suite](../../../DubboService/TripleDubboActuatorSuite.cs); for wire details see the [Native notes (Chinese)](../../NativeDubboProtocol.md) and [Triple notes (Chinese)](../../TripleProtocol.md); compare [codec tests](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs) and [Triple tests](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs).
