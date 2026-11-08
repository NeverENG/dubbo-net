# 元数据：调用 Java 方法前，先弄清签名

[代码地图](../index.md) · [English](../../en/metadata/index.md)

.NET 调用方通常没有 Java 接口 JAR。假设应用把一个整数传给 `getJob`，客户端仍要知道这个方法要求什么 **Java 参数类型**；仅凭 C# 实参，未必能从多个重载中选对方法。元数据解决的就是“这个服务有哪些方法、各自接受什么类型”的问题。它提供调用描述所需的定义，不负责选择业务 Provider 或发送请求。

## 一次调用中，它在哪里起作用

调用方没有指定 Java 类型时，`DubboClient` 把服务名、方法名和实参交给元数据处理。`DubboMetadataManager` 取得并缓存服务定义，`DubboMethodMetadataResolver` 据此确定参数签名；客户端再用签名构造 `DubboInvocation`，继续走[服务选择](../selection/index.md)和[协议传输](../transport/index.md)。**调用方无需先手动查询元数据。**

获取定义前，Manager 先经[注册发现](../registry/index.md)取得 Provider 信息，再按 Provider 情况读取元数据中心；若无法取得方法定义，还可尝试 Provider Telnet。元数据中心可以与注册中心共用连接，也可单独配置。Provider 或元数据发生变化时，已有缓存会按监听结果刷新。这里发现 Provider 是为了**找定义**，不等于已经创建业务调用的服务 Driver。

如果推断所需的签名不可用或不明确，请传入精确 Java 类型；这类调用可能在发请求前抛出 `DubboMetadataException`。显式给出类型可以跳过推断，但客户端仍可能尽力查询元数据，判断重载方法是否需要限制使用原生 Dubbo 协议；查询失败不会把这个兼容性检查变成必需依赖。

## 想自己查看签名时

通过 `DubboClient` 的公开 API 即可，无需创建内部 Manager。下面的地址、接口和方法仅作示例，运行时需替换为可访问的服务：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var methods = await client.GetMethodMetadataAsync(
    "com.foo.JobService#getJob");
```

`GetMethodMetadataAsync` 返回发现的该方法各个签名；`GetServiceMethodsMetadataAsync` 查看整个服务的方法，`GetServiceTypesMetadataAsync` 则读取类型定义。最后一个 API 需要元数据中心，Telnet 无法提供类型定义。更多配置与显式传参方式见[元数据使用说明](../../../readme_cn.md#元数据策略与缓存--metadata-strategy-and-cache)。

**核对实现：** [客户端入口](../../../Clients/DubboClient.cs)、[Manager](../../../Clients/DubboMetadataManager.cs)、[签名解析器](../../../Clients/DubboMethodMetadataResolver.cs)；[元数据测试](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)和[契约测试](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs)。Manager 目前位于 `Clients/`，此处描述现状，不预设未来的代码层级。
