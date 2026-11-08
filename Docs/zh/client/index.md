# 从 `DubboClient` 发起一次调用

[代码地图](../index.md) · [English](../../en/client/index.md)

应用只需提供服务、方法和参数；寻找 Provider、选择连接、发送请求这些工作由 `DubboClient` 协调。它是**消费端入口**，不负责发布 .NET 服务。通常在应用作用域内复用一个客户端，结束时释放，而不是每次请求创建一个。

## 请求怎样往下走

以调用 `com.foo.JobService#getJob` 为例：如果已知 Java 参数类型，可以直接传给 `QueryAsync`；否则客户端先通过[元数据](../metadata/index.md)推断签名。接着它按需检查方法重载的协议兼容性，从[注册中心](../registry/index.md)取得 Provider 地址，由[服务选择](../selection/index.md)选中并预留执行器，最后交给[协议传输](../transport/index.md)。服务地址变化走独立的刷新路径，而不是再次执行公开调用。

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

这里假设注册中心和 Provider 可访问。调用者得到 `DubboRequestResult`（或强类型版本），而不是内部的 Driver 或连接；需要自动推断却找不到明确签名时，可能在发送前抛出 `DubboMetadataException`。用法、其他重载和结果字段见[使用手册](../../../readme_cn.md#快速开始--quick-start)。

**从代码继续读：** [DubboClient](../../../Clients/DubboClient.cs) → [结果类型](../../../DubboService/DataModle/DubboRequestResult.cs)；[公开 API 测试](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)覆盖重载选择，[真实环境指南](../../../Tests/EndToEndTestEnvironment/README.md)说明如何运行 Java Provider。
