# 注册中心与服务发现

[代码地图](../index.md) · [English](../../en/registry/index.md)

## 功能与背景

业务调用前，DubboNet 需要找到某个 Java 接口对应的远端 Provider，并跟踪它们的变化。[`DubboClient` 的发现路径](../../../Clients/DubboClient.cs)使用 [MyZookeeper](../../../Clients/RegistryClient/MyZookeeper.cs) 或 [MyNacos](../../../Clients/RegistryClient/MyNacos.cs)。注册中心负责定位 Provider，**并不**执行业务 RPC，也不把 .NET 应用变成 Provider 服务端。

## 上下游如何衔接

[客户端](../client/index.md)在首次使用或刷新时请求端点；[元数据](../metadata/index.md)定位定义时也需要 Provider 信息。`DubboClient.GetSeviceProviderEndPointsAsync` 查询所选注册中心，`ConcurrentGetProviderEndPoints` 合并在途请求，不是第二份持久 Provider 列表。[ZooKeeper watcher](../../../Clients/DubboClientZookeeperWatcher.cs)与 Nacos 回调将变化送回客户端刷新；更新后的端点进入[服务 Driver](../selection/index.md)，必要时刷新已缓存元数据。

## 输入、产物与连接

- **输入／连接：** 注册中心地址、服务接口名与适用的注册根路径／分组。元数据中心地址可单独配置；它不是 Provider 的业务连接。
- **产物：** Provider 端点信息（包含路由属性）与变更通知。首次元数据查询也可以复用发现路径，**但不会**因此创建服务 Driver。
- **所有权：** 客户端保留注册连接引用和订阅，Driver 保存由发现结果派生的可调用状态；消费者注册不等于发布 Provider 服务。

## 如何使用

正常业务调用通过 `DubboClient` 配置服务发现；`nacos://` 前缀选择 Nacos 而不是 ZooKeeper：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("nacos://127.0.0.1:8848");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });
```

注册分组、独立元数据中心和可选消费者注册见[配置说明](../../../readme_cn.md#客户端配置--configuration)。诊断工具若要读取原始 Nacos 目录，而不是发 RPC，可按[目录示例](../../../readme_cn.md#直接读取-nacos-服务目录)调用公开的 `MyNacos.ListServicesAsync`／`GetProvidersAsync`；这些列表查询不是 Driver 状态。对照 [ZooKeeper 测试](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)、[Nacos 测试](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs)与[消费者注册测试](../../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs)；真实环境见[端到端指南](../../../Tests/EndToEndTestEnvironment/README.md)。
