# 注册与发现：地址从哪里来，变化交给谁

[调用链总览](index.md) · [English](../../en/architecture/registry.md)

## 职责与边界

注册与发现回答的是“某个 Java 接口现在有哪些 Provider 地址”。它从 ZooKeeper 的接口级 `providers` 节点或 Nacos Naming 读取实例，交给调用方一份端点结果，并为后续变化建立通知路径。**发现结果不是第二份可调用 Provider 缓存**：业务调用能否选中节点，仍由[服务 Driver](selection.md)持有和判断。Java 方法定义归[元数据](metadata.md)，请求发送归[传输](transport.md)。

当前 `main` 中，这些动作尚集中在 `DubboClient`：`ConcurrentGetProviderEndPoints` 管理在途查询，`GetSeviceProviderEndPointsAsync` 实际读取注册中心，`ReflushProviderAsync` 决定何时更新 Driver。文件位于 `Clients/` 是现状，不代表发现必须永远由客户端入口实现；正在评审的 [#36](https://github.com/apache/dubbo-net/pull/36) 会改变源码归属。本页以下只描述当前 `main`，不提前把它写成已合并的设计。

## 首次调用与元数据查询

当 `QueryInternalAsync` 找不到服务 Driver，它经 `ConcurrentGetProviderEndPoints` 按服务名查 Provider；成功后才调用 `DubboDriverCollection.AddDubboServiceDriver`，再尝试选择与发送。已有 Driver 却没有端点时，请求也会主动查一次：发现到新节点才更新 Driver 并重新选择；查不到则返回失败结果。**Driver 有候选但执行器不可用**属于另一种选择失败，不靠重复发现解决。入口见 [`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs)，状态判别见 [`DubboDriverCollection.GetDubboActuatorSuite`](../../../Clients/DubboDriverCollection.cs)。

[`DubboMetadataManager`](../../../Clients/DubboMetadataManager.cs) 取得 Java 方法定义前，也可能通过 `GetProviderEndPointsForMetadataAsync` 查 Provider，以决定从哪里读取定义。这条查询复用发现路径，但**不会因此创建业务 Driver**。因此，“曾经为元数据查过该服务”不等于“该服务已有可调用节点”。

| 来源 | 当前查询与结果 | 后续通知 |
| --- | --- | --- |
| ZooKeeper | 检查接口的 `providers` 路径，读取并解析 `dubbo://`、`tri://` 子节点；路径或端点不存在时返回错误信息；连接重试耗尽可能抛异常。 | 非空子节点分支调用带 `DubboClientZookeeperWatcher` 的 `GetChildrenAsync`，安装**一次性**子节点 watch。 |
| Nacos Naming | 按接口及分组查对应服务，只把健康、已启用的实例转换为端点；无健康端点或查询异常形成错误信息。 | `MyNacos.GetProviderEndPointsAsync` 为此客户端登记 Naming 变更回调。 |

两种来源在 [`GetSeviceProviderEndPointsAsync`](../../../Clients/DubboClient.cs) 汇合，Nacos 实例过滤和订阅见 [`MyNacos.GetProviderEndPointsAsync`](../../../Clients/RegistryClient/MyNacos.cs)。这里的“错误信息”由调用路径转为失败结果；ZooKeeper SDK 查询重试耗尽仍可能抛异常，不能把所有故障都说成空 Provider 列表。当前在途查询复用也不应被解释成对所有并发时序保证“恰好只启动一次 SDK 查询”。

## 变化通知与状态刷新

`DubboClientZookeeperWatcher.process` 收到 Provider 子节点变化后，先从路径识别服务：有 Driver 时按路径调用 `ReflushProviderAsync`；只有元数据缓存时只刷新该缓存；两者都没有就不建立新 Driver。Nacos 的 `HandleNacosProviderChangedAsync` 也按“已有 Driver / 仅有元数据缓存”分流。**变化事件不是重放一次业务 `QueryAsync`。** 入口分别见 [ZooKeeper watcher](../../../Clients/DubboClientZookeeperWatcher.cs) 和 [Nacos 回调](../../../Clients/DubboClient.cs)。

刷新发现到新端点时，`ReflushProviderAsync` 更新已有 Driver，再请求刷新已缓存的服务元数据。若返回明确的“没有 Provider / 没有健康 Provider”，已有 Driver 的候选会更新为空；一般查询错误只记录，不应误当成“所有 Provider 都已下线”。断线和会话过期另走 watcher 的重连路径，连接恢复后逐服务尝试重新发现。这里涉及回调与重连两个异步入口，不保证它们与客户端关闭已经完全排空。

## 订阅与关闭

Nacos Naming 订阅由当前 `DubboClient` 以自身为订阅标识建立；`Dispose` 先调用 `RemoveSubscriber(this)`，再释放持有的注册连接引用。元数据 Manager 的 Nacos **Config** 订阅有自己的标识和释放动作，不应随 Provider Naming 订阅一起误删。`MultiMyZookeeperStorage` / `MultiMyNacosStorage` 按地址复用底层连接并维护引用计数；同地址元数据中心可复用注册连接，独立地址才另持有引用。这里释放的是**本客户端的引用**，不表示强行关闭其他客户端共用的连接。

ZooKeeper 子节点 watch 是一次性通知，没有按 watcher 单独退订的接口；回调入口检查客户端是否已关闭。当前 Provider 路径**存在但子节点数为零**时，发现方法直接返回，没有安装子节点 watch；Provider 后来重新出现不一定能靠事件自动刷新，下一次业务调用仍会尝试主动发现。恢复刷新抛错后防重入状态的风险与关闭交错也尚未修复，见 [#37](https://github.com/apache/dubbo-net/issues/37)。本页记录代码现状，不把这些限制写成已经解决。

## 从文件进入并验证

| 文件 | 本页应读的职责 |
| --- | --- |
| [`Clients/DubboClient.cs`](../../../Clients/DubboClient.cs) | 查询汇合、错误分类、Driver/Metadata 分流、Nacos 订阅标识与连接引用释放。 |
| [`Clients/DubboClientZookeeperWatcher.cs`](../../../Clients/DubboClientZookeeperWatcher.cs) | 子节点事件、断线/会话过期事件的入口；并不保存可调用 Provider 列表。 |
| [`Clients/RegistryClient/MyZookeeper.cs`](../../../Clients/RegistryClient/MyZookeeper.cs) / [`MyNacos.cs`](../../../Clients/RegistryClient/MyNacos.cs) | SDK 查询、连接重试及按订阅者管理的 Nacos Naming 通知。 |
| [`Clients/RegistryClient/MultiMyZookeeperStorage.cs`](../../../Clients/RegistryClient/MultiMyZookeeperStorage.cs) / [`MultiMyNacosStorage.cs`](../../../Clients/RegistryClient/MultiMyNacosStorage.cs) | 按地址复用 SDK 连接及引用计数，不保存服务 Driver 的候选状态。 |

[ZooKeeper 单元测试](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)覆盖不可用连接的错误边界；[Nacos 测试](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs)覆盖实例查询、订阅者隔离与目录变化。它们不能替代空节点变化与完整重连的运行测试；实际环境要求见[端到端环境指南](../../../Tests/EndToEndTestEnvironment/README.md)。
