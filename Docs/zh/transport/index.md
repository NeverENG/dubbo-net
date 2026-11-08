# 协议传输：把请求发给远端 Provider

[代码地图](../index.md) · [English](../../en/transport/index.md)

到这一步，客户端已经准备好方法、Java 参数类型，并[选定 Provider](../selection/index.md)。执行器负责把调用编码、发送并读取响应：`dubbo://` 使用原生 Dubbo TCP，`tri://` 使用 Triple HTTP/JSON。虽然目录叫 `DubboService/`，这里的执行器属于**消费端**，不是 .NET Provider 服务端。

`DubboClient` 将准备好的调用交给选中的执行器；执行器管理对应的传输资源，结果返回 `DubboRequestResult`。无论发送成功还是失败，选择阶段的在途预留都需要完成。正常应用只需用[客户端 API](../client/index.md)，不必自行建立协议连接。

如果已经知道 Provider 的地址和准确的 Java 签名，也能绕开注册发现、元数据推断和负载均衡，直接使用原生执行器：

```csharp
using DubboNet.DubboService;
using DubboNet.DubboService.DataModle;

using var suite = new NativeDubboActuatorSuite("127.0.0.1", 20880);
var invocation = new DubboInvocation(
    "com.foo.JobService", "getJob",
    new[] { "java.lang.Integer" }, new object[] { 1234 });
var result = await suite.SendQuery(invocation);
```

Triple 直连见[使用手册](../../../readme_cn.md#直接使用执行器--direct-actuator-usage)。这不是另一套自动发现入口；地址和签名须由调用方提供。

**从代码继续读：** [原生执行器](../../../DubboService/NativeDubboActuatorSuite.cs)、[连接](../../../DubboService/Native/NativeDubboConnection.cs)、[编解码](../../../DubboService/Native/NativeDubboCodec.cs)、[Triple 执行器](../../../DubboService/TripleDubboActuatorSuite.cs)；协议细节见[原生协议](../../NativeDubboProtocol.md)、[Triple 协议](../../TripleProtocol.md)，对照[编解码测试](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs)和 [Triple 测试](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs)。
