# 协议传输

[代码地图](../index.md) · [English](../../en/transport/index.md)

## 功能与背景

选定 Provider 后，传输执行器向该**远端** Provider 发送调用并读取响应。`DubboService/` 虽然叫这个名字，其中的执行器仍属于消费端，不是 .NET Provider 服务端入口。原生 Dubbo TCP 与 Triple HTTP/JSON 的线协议不同，协议细节留在已有专题文档，不在此重新定义。

## 上下游如何衔接

[服务 Driver](../selection/index.md)按 Provider URL scheme 选执行器。[`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs)把 `DubboInvocation` 交给执行器。对于 `dubbo://`，[`NativeDubboActuatorSuite`](../../../DubboService/NativeDubboActuatorSuite.cs)使用 [`NativeDubboConnection`](../../../DubboService/Native/NativeDubboConnection.cs)与 [`NativeDubboCodec`](../../../DubboService/Native/NativeDubboCodec.cs)；对于 `tri://`，[`TripleDubboActuatorSuite`](../../../DubboService/TripleDubboActuatorSuite.cs)使用 HTTP/JSON。发送正常返回或抛出异常后，都会完成[本次调用的预留](../selection/index.md)。

## 输入、产物与连接

- **输入：** 已选中的 Provider 连接／执行器，以及包含服务、方法、精确 Java 参数类型和参数值的调用对象。直接使用执行器时，不会自动发现 Provider 或推断签名。
- **产物：** 包含成功状态、响应数据与错误信息的 `DubboRequestResult`；传输格式虽不同，对外仍遵循[结果类型](../../../DubboService/DataModle/DubboRequestResult.cs)的契约。
- **连接：** 执行器按实现管理传输资源；选择与在途预留决定共享执行器何时可以退出。

## 如何使用

通常使用[客户端 API](../client/index.md)，让它负责发现和选择。如果已知 Provider 地址与 Java 签名，也可以直接调用原生执行器：

```csharp
using DubboNet.DubboService;
using DubboNet.DubboService.DataModle;

using var suite = new NativeDubboActuatorSuite("127.0.0.1", 20880);
var invocation = new DubboInvocation(
    "com.foo.JobService", "getJob",
    new[] { "java.lang.Integer" }, new object[] { 1234 });
var result = await suite.SendQuery(invocation);
```

这种方式绕过注册中心、负载均衡与元数据推断。Triple 直连示例见[使用手册](../../../readme_cn.md#直接使用执行器--direct-actuator-usage)。线协议与兼容细节分别看[原生协议说明](../../NativeDubboProtocol.md)和 [Triple 协议说明](../../TripleProtocol.md)；以[原生编解码测试](../../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs)、[Triple 传输测试](../../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs)和[真实环境指南](../../../Tests/EndToEndTestEnvironment/README.md)核对实现。
