# 注册中心：找到 Provider，并跟上变化

[代码地图](../index.md) · [English](../../en/registry/index.md)

客户端知道要调用 `JobService`，但并不知道它在哪台机器上。注册中心回答的是“有哪些 Provider、各自的地址和路由信息”，**不是**替客户端发送业务请求。当前实现支持 ZooKeeper 与 Nacos；在客户端构造函数里传入 `nacos://127.0.0.1:8848` 可选择 Nacos，不带该前缀的常规地址走 ZooKeeper。完整配置见[使用手册](../../../readme_cn.md#客户端配置--configuration)。

首次调用尚未建立服务 Driver 时，客户端按服务名查询端点，把结果交给[服务选择](../selection/index.md)。[元数据查询](../metadata/index.md)也可能借此定位 Provider，但仅查询元数据不会因此建立业务调用的 Driver。多个并发查询同一服务时，客户端复用在途发现任务；这并不是另一份长期保存的 Provider 列表。

Provider 上下线后，ZooKeeper watcher 或 Nacos 回调将变化送回客户端，刷新可调用端点，并按需刷新元数据。这条更新路径与 `QueryAsync` 请求路径分开。客户端持有注册连接与订阅；Driver 持有从发现结果构建的可调用状态。可选的**消费者注册**也不意味着发布了 Provider 服务。

需要查看 Nacos 原始目录而非发起 RPC？公开的 `MyNacos.ListServicesAsync` 和 `GetProvidersAsync` 用法见[目录示例](../../../readme_cn.md#直接读取-nacos-服务目录)。

**从代码继续读：** [DubboClient 的发现逻辑](../../../Clients/DubboClient.cs)、[ZooKeeper 实现](../../../Clients/RegistryClient/MyZookeeper.cs)、[Nacos 实现](../../../Clients/RegistryClient/MyNacos.cs)、[变化监听](../../../Clients/DubboClientZookeeperWatcher.cs)；对照 [ZooKeeper 测试](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)、[Nacos 测试](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs)及[消费者注册测试](../../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs)。
