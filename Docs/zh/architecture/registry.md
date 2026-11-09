# 注册与发现：地址、通知、连接引用

[调用链总览](index.md) · [English](../../en/architecture/registry.md)

## 当前入口与交接

| 入口 | 交给下游什么 |
| --- | --- |
| [`ProviderDiscovery.GetProviderEndPointsAsync`](../../../Registry/ProviderDiscovery.cs) | 请求路径与元数据查询都可经此查服务；发现层按服务和路径模式复用在途任务。查到的端点不是第二份“可调用 Provider”状态。 |
| [`ProviderDiscovery.DiscoverAsync`](../../../Registry/ProviderDiscovery.cs) | 根据注册中心类型查询 ZooKeeper 子节点或 Nacos Naming；返回地址及错误，再由[客户端](client.md)决定是否更新 Driver。 |
| [`DubboClientZookeeperWatcher.process`](../../../Clients/DubboClientZookeeperWatcher.cs) / [`HandleNacosProviderChangedAsync`](../../../Clients/DubboClient.cs) | 地址变化通知客户端：已有 Driver 则刷新其候选，仅有元数据缓存则刷新定义；断线事件进入重连路径。 |

ZooKeeper 查询在读取子节点时注册一次性 watch；不是长期独立订阅。现有空 Provider 子节点路径没有安装这一 watch，[#37](https://github.com/apache/dubbo-net/issues/37) 单独跟踪，不能把“服务存在”概括为“所有变化都会自动刷新”。Nacos 的 Naming 订阅与元数据 Config 订阅也不是同一份订阅。底层连接由共享 storage 复用，客户端持有并在 `Dispose` 时释放相应引用，不等于自己独占 SDK 连接。

## 状态边界与验证

注册查询只提供地址/路由信息；[`DubboDriverCollection.AddDubboServiceDriver`](../../../Clients/DubboDriverCollection.cs) 将其交给[选择模块](selection.md)建立或刷新可调用状态。元数据借发现定位定义时**不创建**业务 Driver；详见[元数据](metadata.md)。

**看代码：** [ZooKeeper SDK 封装](../../../Registry/MyZookeeper.cs)、[Nacos SDK 封装](../../../Registry/MyNacos.cs)、[共享连接存储](../../../Registry/MultiMyZookeeperStorage.cs)。**核对：** [ZooKeeper 测试](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)、[Nacos 测试](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs)。
