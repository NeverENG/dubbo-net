# 客户端入口

[代码地图](../index.md) · [English](../../en/client/index.md)

## 功能与背景

应用需要一个可长期复用的入口，而不是每次调用都自行管理注册中心连接、元数据和 Provider 连接。[`DubboClient`](../../../Clients/DubboClient.cs) 提供 `QueryAsync`，并协调这些部分。它是**消费端**入口：测试环境中的 Java Provider 是远端服务，并非 DubboNet 发布的服务。本页描述当前职责，不表示这些职责将来都必须留在同一类中。

## 上下游如何衔接

上游应用给出服务、方法、参数，也可以给出精确 Java 参数类型。未给类型时，`QueryWithInferredTypesAsync` 或 `QueryAsync(DubboInvocation)` 通过[元数据](../metadata/index.md)构造 `DubboInvocation`。随后 `QueryInternalAsync` 可能再查元数据判断重载兼容性，需要端点时进入[注册与发现](../registry/index.md)，再交给[选择与预留](../selection/index.md)和[协议传输](../transport/index.md)。Provider 变更走另一条进入客户端的刷新路径，不是再次调用公开 `QueryAsync`。

## 输入、产物与生命周期

- **输入：** 方法入口，或包含服务、方法、Java 参数类型、参数值及可选版本／分组／附件的 `DubboInvocation`。
- **产物：** `DubboRequestResult` 或强类型结果；调用方读取 `QuerySuccess`、结果和错误信息。自动推断可能在发送前抛 `DubboMetadataException`。字段以[结果类型](../../../DubboService/DataModle/DubboRequestResult.cs)为准。
- **生命周期：** 一个客户端跨调用持有注册／元数据引用和服务 Driver；在所属应用作用域结束时释放。连接与消费者注册的行为见[使用手册](../../../readme_cn.md#客户端配置--configuration)。

## 如何使用

已知 Java 方法签名时可显式传入类型；下面假设注册中心与该服务的 Provider 均可访问：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

自动推断、其他重载和完整结果处理见[中文 README](../../../readme_cn.md#快速开始--quick-start)；[公开 API 测试](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)可核对数组参数如何匹配重载。真实调用环境见[端到端指南](../../../Tests/EndToEndTestEnvironment/README.md)。
