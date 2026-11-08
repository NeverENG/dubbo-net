# Transport: sending the call to a remote Provider

[Code map](../index.md) · [中文](../../zh/transport/index.md)

By this point the client has prepared the method and Java parameter types, and [service selection](../selection/index.md) has chosen a Provider for this call. A transport suite encodes the request, sends it to that **remote** Provider and reads the response. Despite the name `DubboService/`, these suites are consumer-side code, not .NET Provider servers.

## How the request is sent

Registry discovery currently accepts `dubbo://` and `tri://` Provider URLs: the former uses the native Dubbo TCP connection and codec, while the latter uses the Triple HTTP/JSON suite. Each suite manages its transport resources; applications still receive a `DubboRequestResult` from `DubboClient`. The [selection reservation](../selection/index.md) is released at the end of the call, including when sending fails.

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

**Implementation note:** The driver retains a Telnet-suite fallback for other schemes, but registry discovery does not add those addresses as normal candidates. This fallback is distinct from the Telnet signature probe on the [metadata page](../metadata/index.md).

**Check the implementation:** [suite selection](../../../Clients/DubboServiceDriver.cs), [native suite](../../../DubboService/NativeDubboActuatorSuite.cs), [Triple suite](../../../DubboService/TripleDubboActuatorSuite.cs); [codec tests](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs) and [Triple tests](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs).
