# 注册与发现：这个服务在哪里

[代码地图](../index.md) · [English](../../en/registry/index.md)

应用请求 `JobService` 时，客户端还不知道有哪些机器提供该服务。注册中心给出 Provider 的地址和路由属性，并在这些地址变化时发出通知。它解决的是**找得到谁**，不是替应用发送业务请求；注册为消费者也不等于发布 .NET Provider 服务。

## 从查询到更新

首次业务调用没有服务 Driver 时，`DubboClient` 按服务名发现端点，把结果交给[服务选择](../selection/index.md)建立可调用状态。同一服务的并发发现会共用在途查询任务。查[元数据](../metadata/index.md)也需要 Provider 信息，但这次发现只是为了定位定义，不会因此建立业务调用的 Driver。

Provider 上下线后，ZooKeeper watcher 或 Nacos 回调把变化送回客户端。客户端更新已存在的可调用端点，必要时刷新缓存的元数据；这是与 `QueryAsync` 并行的**更新路径**。注册连接和订阅由客户端持有，Driver 保存的是从发现结果构建的服务候选。

## 怎样接入

创建 `DubboClient` 时传入注册中心地址即可：常规 `host:2181` 地址使用 ZooKeeper，`nacos://host:8848` 前缀使用 Nacos。之后应用照常调用 `QueryAsync`，无需手工查询 Provider。注册分组、独立的元数据中心及可选消费者注册见[配置说明](../../../readme_cn.md#客户端配置--configuration)。

如果只是诊断 Nacos 目录、不发送 RPC，可以用公开的 `MyNacos.ListServicesAsync` 或 `GetProvidersAsync`；用法见[目录示例](../../../readme_cn.md#直接读取-nacos-服务目录)。目录查询结果本身不是 Driver 的可调用状态。

**核对实现：** [客户端发现入口](../../../Clients/DubboClient.cs)、[ZooKeeper](../../../Clients/RegistryClient/MyZookeeper.cs)、[Nacos](../../../Clients/RegistryClient/MyNacos.cs)、[变化监听](../../../Clients/DubboClientZookeeperWatcher.cs)；[ZooKeeper 测试](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)、[Nacos 测试](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs)与[消费者注册测试](../../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs)。
