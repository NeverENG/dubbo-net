# 传输：执行器接收什么、返回什么

[调用链总览](index.md) · [English](../../en/architecture/transport.md)

## 选中之后的交接

| 源码入口 | 读它要核对的事 |
| --- | --- |
| [`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs) | 对结构化调用使用 `IDubboGenericInvocationActuator.SendQuery(invocation)`；兼容旧执行器时序列化参数并调用旧签名。发送被 [`LoadBalancedInvocation.InvokeAsync`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs) 包裹，确保预留归还。 |
| [`NativeDubboActuatorSuite`](../../../DubboService/NativeDubboActuatorSuite.cs) | `dubbo://` 的 TCP/Hessian2 请求、响应和连接；[原生协议页](../protocols/native-dubbo.md)记录帧格式及当前限制。 |
| [`TripleDubboActuatorSuite`](../../../DubboService/TripleDubboActuatorSuite.cs) | `tri://` 的 Java Interface Unary HTTP/JSON 请求与超时/取消；[Triple 协议页](../protocols/triple.md)记录请求格式及明确边界。 |

正常注册发现只接收 `dubbo://` 与 `tri://` Provider；Driver 内的其他 scheme Telnet 回退**不是**常规注册候选，也不是[元数据](metadata.md)用的短时 Telnet `ls -l` 探测。传输执行器负责收发和自身资源，不拥有服务 Driver 的候选集合，也不选择负载策略。调用方应区分发送前的签名异常和发送后的 `DubboRequestResult` 失败结果。

**核对：** [Native 编解码测试](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs)、[Triple 执行器测试](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs)、[传输可靠性测试](../../../Tests/UnitTestForDubboNet/TripleTransportReliabilityTest.cs)。如何直连执行器在[使用说明](../../../readme_cn.md#直接使用执行器--direct-actuator-usage)。
