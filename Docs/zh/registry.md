# 注册中心与 Provider 发现

[代码地图](index.md) · [English](../en/registry.md)

[`DubboClient.GetSeviceProviderEndPointsAsync`](../../Clients/DubboClient.cs) 通过 [`MyZookeeper`](../../Clients/RegistryClient/MyZookeeper.cs) 或 [`MyNacos`](../../Clients/RegistryClient/MyNacos.cs) 查找 Provider 端点。`ConcurrentGetProviderEndPoints` 协调在途查询，并不是第二份持久 Provider 列表。首次调用发现端点后可以建立[服务 Driver](selection.md)；[元数据](metadata.md)定位定义也会请求端点，但不会因此创建 Driver。

**变化路径：** [`DubboClientZookeeperWatcher`](../../Clients/DubboClientZookeeperWatcher.cs) 和 Nacos 回调通知[客户端](client.md)，由客户端刷新已缓存的 Driver 或元数据。要把这条事件路径与首次调用发现分开阅读。注册中心连接、消费者注册与发送业务请求的 Provider 连接不是一回事。

**源码与验证：** [发现与刷新](../../Clients/DubboClient.cs) · [ZooKeeper 测试](../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs) · [Nacos 测试](../../Tests/UnitTestForDubboNet/MyNacosTest.cs) · [消费者注册测试](../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs) · [测试环境](../../Tests/EndToEndTestEnvironment/README.md)。注册配置与支持范围请看[中文 README](../../readme_cn.md)，不在地图中复制。
