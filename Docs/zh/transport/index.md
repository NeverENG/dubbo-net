# 协议传输：把调用交给远端 Provider

[代码地图](../index.md) · [English](../../en/transport/index.md)

到这一步，客户端已经准备好方法和 Java 参数类型，[服务选择](../selection/index.md)也已经确定本次调用使用哪个 Provider。传输执行器负责按协议编码请求、发送给**远端** Provider，再读取响应。目录虽然叫 `DubboService/`，其中的执行器仍是消费端代码，不是 .NET Provider 服务端。

## 同一次调用，两种传输方式

Provider 地址为 `dubbo://` 时，原生执行器使用 Dubbo TCP 连接与编解码；`tri://` 则交给 Triple HTTP/JSON 执行器。这些线协议由各自实现处理，不改变应用从 `DubboClient` 取得 `DubboRequestResult` 的用法。执行器负责传输资源；调用结束时，[服务选择的预留](../selection/index.md)会归还，即使发送失败也一样。

正常应用通过[客户端 API](../client/index.md)调用，不需要自己创建执行器。只有已知 Provider 地址及准确 Java 签名、且**有意绕过**注册发现、元数据推断和负载均衡时，才直接使用执行器。例如原生 Dubbo：

```csharp
using DubboNet.DubboService;
using DubboNet.DubboService.DataModle;

using var suite = new NativeDubboActuatorSuite("127.0.0.1", 20880);
var invocation = new DubboInvocation(
    "com.foo.JobService", "getJob",
    new[] { "java.lang.Integer" }, new object[] { 1234 });
var result = await suite.SendQuery(invocation);
```

地址和方法仅作示例，运行时需有相应 Provider。Triple 直连见[使用手册](../../../readme_cn.md#直接使用执行器--direct-actuator-usage)；线协议细节分别见[原生协议](../../NativeDubboProtocol.md)与 [Triple 协议](../../TripleProtocol.md)。

**核对实现：** [原生执行器](../../../DubboService/NativeDubboActuatorSuite.cs)、[连接](../../../DubboService/Native/NativeDubboConnection.cs)、[编解码](../../../DubboService/Native/NativeDubboCodec.cs)、[Triple 执行器](../../../DubboService/TripleDubboActuatorSuite.cs)；[编解码测试](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs)与 [Triple 测试](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs)。
