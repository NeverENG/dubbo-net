# 源码导航：从入口追踪调用与更新

[文档首页](../index.md) · [English](../../en/architecture/index.md) · [使用说明](../../../readme_cn.md)

这张地图以 **当前代码实现**为准，介绍当前实现，不是目标架构。DubboNet 是调用 Java Provider 的消费端。下面是两条可分别阅读的路径；只有标明条件的步骤才会发生。想学习如何使用 API，请去[使用说明](../../../readme_cn.md)；协议字节在[原生](../protocols/native-dubbo.md)和[Triple](../protocols/triple.md)页面。

## 请求路径：`QueryAsync` 到结果

```text
DubboClient.QueryAsync
  ├─ Java 类型未给出 → DubboInvocationMetadataResolver.CreateAsync → DubboMetadataManager → 签名解析
  └─ QueryInternalAsync → GetRequiredProtocolAsync（尽力检查重载约束）
       → DubboDriverCollection.GetDubboActuatorSuite(reserve: true)
          ├─ 无 Driver → ProviderDiscovery.GetProviderEndPointsAsync → AddDubboServiceDriver → 再次选择
          ├─ Driver 存在但 Provider 为空 → 重新发现；有节点才更新、重选
          ├─ 有候选但执行器不可用 → 返回失败结果
          └─ 已选中 → LoadBalancedInvocation.InvokeAsync → 执行器 SendQuery → DubboRequestResult
```

| 从这里开始读 | 问题与下一站 |
| --- | --- |
| [`DubboClient.QueryAsync` / `QueryInternalAsync`](../../../Clients/DubboClient.cs) | 参数如何进入？何时推断签名、检查协议、触发首次发现？→ [客户端](client.md) |
| [`DubboInvocationMetadataResolver.CreateAsync`](../../../Metadata/DubboInvocationMetadataResolver.cs) / [`DubboMetadataManager.GetServiceMethodsAsync`](../../../Metadata/DubboMetadataManager.cs) | Java 签名从何而来，缺失时怎样处理？→ [元数据](metadata.md) |
| [`ProviderDiscovery.GetProviderEndPointsAsync` / `DiscoverAsync`](../../../Registry/ProviderDiscovery.cs) | 谁查地址、谁安装订阅？→ [注册与发现](registry.md) |
| [`DubboDriverCollection.GetDubboActuatorSuite`](../../../Clients/DubboDriverCollection.cs) / [`DubboServiceDriver.SelectAndReserve`](../../../Clients/DubboServiceDriver.cs) | 哪份状态可用于选择、谁保护在途调用？→ [选择与预留](selection.md) |
| [`LoadBalancedInvocation.InvokeAsync`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs) | 预留怎样包裹实际发送与释放？→ [传输](transport.md) |

元数据查询**也可能先发现 Provider**；已有 Driver 的调用不必每次查询注册中心。上图是分支导航，不是每次请求的固定执行顺序。

## 更新路径：事件到已有状态

```text
ZooKeeper: DubboClientZookeeperWatcher.process ─┐
Nacos: HandleNacosProviderChangedAsync ───────────┼─ 已有 Driver → ReflushProviderAsync
                                                  │                  → 发现地址 → 更新 Driver／元数据缓存
                                                  └─ 仅有元数据缓存 → RefreshCachedMetadataAsync
断线恢复: StartConnecTaskAsync → TryDoConnectRegistryTaskAsync → ReLoadDubboDriverCollection
关闭: DubboClient.Dispose → 取消订阅／释放连接引用、Driver 与执行器
```

[注册与发现](registry.md)说明通知和订阅，[客户端](client.md)说明重连与释放。这里不宣称所有事件、异常和关闭并发路径都已正确收敛；恢复刷新与空节点 watch 的待验证问题见 [#37](https://github.com/apache/dubbo-net/issues/37)。

## 状态归属（当前代码）

| 状态 | 当前持有者 | 不等于 |
| --- | --- | --- |
| 可调用 Provider 候选、选择与预留 | [`DubboServiceDriver`](../../../Clients/DubboServiceDriver.cs) 及 [`DubboDriverCollection`](../../../Clients/DubboDriverCollection.cs) | 注册发现的查询结果缓存 |
| Java 方法与类型定义 | [`DubboMetadataManager`](../../../Metadata/DubboMetadataManager.cs) | 可调用 Provider 列表 |
| 发现中的任务与 Provider 订阅 | [`ProviderDiscovery`](../../../Registry/ProviderDiscovery.cs) | Driver 持有的可调用候选 |
| 注册连接引用及客户端执行器寿命 | [`DubboClient`](../../../Clients/DubboClient.cs)；底层连接由共享 storage 复用 | 将连接独占或把 watcher 当成可单独退订 |

先从[公开 API 测试](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)、[元数据测试](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)和[Driver 测试](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)核对对应路径；真实 Provider 的运行条件在[端到端环境指南](../../../Tests/EndToEndTestEnvironment/README.md)。
