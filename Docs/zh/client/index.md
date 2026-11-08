# 客户端：从方法调用走到远端 Provider

[代码地图](../index.md) · [English](../../en/client/index.md)

应用知道要调用哪个 Java 方法，却不应该自己管理注册中心、Provider 连接和请求发送。`DubboClient` 是这些工作的**消费端入口**：它接收服务、方法和参数，最后把调用结果交还应用；它不发布 .NET 服务。

## 请求怎样走到 Provider

以 `com.foo.JobService#getJob` 为例。传入明确的 Java 参数类型时，客户端可以直接准备调用；不传类型时，它会查[元数据](../metadata/index.md)确定签名。准备好 `DubboInvocation` 后，客户端按需检查重载方法的协议限制：已有服务从 Driver 中挑选执行器，首次使用则先经[注册发现](../registry/index.md)取得 Provider 地址、建立 Driver。随后[服务选择](../selection/index.md)预留本次调用使用的执行器，[协议传输](../transport/index.md)发送并读取响应。

Provider 上下线由注册中心回调触发刷新，不是重新调用一次 `QueryAsync`。客户端跨请求持有注册连接、元数据和服务 Driver 等状态，所以通常在应用作用域内复用，结束时释放。

## 应用怎样调用

已知 Java 方法签名时，可通过公开 API 显式传入类型。下面的注册中心地址和方法仅作示例，运行时需有可访问的 Provider：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

调用方收到 `DubboRequestResult`（也有强类型重载），而不是内部 Driver 或连接。需要推断类型却找不到明确签名时，可能在发送前抛出 `DubboMetadataException`；发送后的结果则看 `QuerySuccess` 和结果／错误字段。其他重载与完整用法见[使用手册](../../../readme_cn.md#快速开始--quick-start)。

**核对实现：** [DubboClient](../../../Clients/DubboClient.cs)、[调用结果](../../../DubboService/DataModle/DubboRequestResult.cs)；[公开 API 测试](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)和[真实环境指南](../../../Tests/EndToEndTestEnvironment/README.md)。
