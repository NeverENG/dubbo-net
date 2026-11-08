# Transport: Actuator Inputs and Results

[Call-path overview](index.md) · [中文](../../zh/architecture/transport.md)

## Handoff after selection

| Source entry | What to verify |
| --- | --- |
| [`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs) | Structured calls use `IDubboGenericInvocationActuator.SendQuery(invocation)`; legacy actuators receive serialized arguments through the older signature. [`LoadBalancedInvocation.InvokeAsync`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs) wraps dispatch and releases the reservation. |
| [`NativeDubboActuatorSuite`](../../../DubboService/NativeDubboActuatorSuite.cs) | TCP/Hessian2 requests, responses, and connections for `dubbo://`; the [native protocol page](../protocols/native-dubbo.md) covers frames and limits. |
| [`TripleDubboActuatorSuite`](../../../DubboService/TripleDubboActuatorSuite.cs) | Java Interface Unary HTTP/JSON requests and deadlines/cancellation for `tri://`; the [Triple protocol page](../protocols/triple.md) covers the format and explicit boundaries. |

Normal registry discovery accepts `dubbo://` and `tri://` Providers. Other scheme-based Telnet fallback inside the driver is **not** a normal registered candidate, nor is it the one-shot Telnet `ls -l` [metadata](metadata.md) probe. An actuator handles I/O and its own resources; it does not own the service driver's candidate set or choose load-balancing policy. Distinguish pre-dispatch signature exceptions from `DubboRequestResult` failures returned after dispatch.

**Verify:** [native codec tests](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs), [Triple actuator tests](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs), [transport reliability tests](../../../Tests/UnitTestForDubboNet/TripleTransportReliabilityTest.cs). [Direct actuator usage](../../../README.md#direct-actuator-usage) belongs in the user guide.
