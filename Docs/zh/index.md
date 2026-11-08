# 代码地图：消费端调用链

[English](../en/index.md) · [中文 README](../../readme_cn.md) · [English README](../../README.md)

这里提供读代码的路线，不另写一套运行行为规范。具体行为以所链接的代码和测试为准。下述 DubboNet 运行链路调用远端 Provider；`Tests/` 下的 Java Provider 是测试环境，不是 .NET Provider 端实现。

## 1. 从公开调用进入

- [DubboClient.QueryAsync](../../Clients/DubboClient.cs) 接收服务方法入口或 `DubboInvocation`；各重载最终进入调用准备和 `QueryInternalAsync`。[API 测试](../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)可用于辨认公开调用形式。
- 需要推断 Java 参数类型时，`CreateMetadataInvocationAsync` 使用 [DubboMetadataManager](../../Clients/DubboMetadataManager.cs) 和 [DubboMethodMetadataResolver](../../Clients/DubboMethodMetadataResolver.cs)。显式指定类型可跳过推断，但 `QueryInternalAsync` 仍可能查元数据以判断重载兼容性。对照[元数据测试](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)和[契约测试](../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs)。

## 2. 发现 Provider 与读取元数据

- [DubboClient.GetSeviceProviderEndPointsAsync](../../Clients/DubboClient.cs) 通过 [MyZookeeper](../../Clients/RegistryClient/MyZookeeper.cs) 或 [MyNacos](../../Clients/RegistryClient/MyNacos.cs) 发现端点；同服务的并发发现会共享正在执行的任务，这不是第二份持久 Provider 列表。对照 [ZooKeeper 测试](../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)和 [Nacos 测试](../../Tests/UnitTestForDubboNet/MyNacosTest.cs)。
- [DubboMetadataManager.LoadServiceMetadataAsync](../../Clients/DubboMetadataManager.cs) 需要先了解 Provider，才能定位元数据或探测 Provider Telnet；传入的 Provider resolver 会回调 `DubboClient` 发现。**读取元数据不等于创建服务 Driver**。另一来源可看 [Telnet resolver](../../Clients/DubboTelnetMetadataResolver.cs)及[元数据测试](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)。
- 变化由 [ZooKeeper watcher](../../Clients/DubboClientZookeeperWatcher.cs) 或 `DubboClient` 中的 Nacos 回调进入；[ReflushProviderAsync](../../Clients/DubboClient.cs) 更新已缓存 Driver 并刷新已缓存元数据。这条异步事件路径与正常调用分开看。

## 3. 选择、发送与释放

- [DubboDriverCollection](../../Clients/DubboDriverCollection.cs) 保存服务到 Driver 的对应关系，并在发现后创建／更新 Driver。[DubboServiceDriver](../../Clients/DubboServiceDriver.cs) 保存服务端点状态、构建候选集、选择并预留 Provider；对照 [Driver 测试](../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)和[负载均衡测试](../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs)。
- `QueryInternalAsync` 通过选中的执行器发送。[LoadBalancedInvocation](../../Clients/LoadBalancing/LoadBalancedInvocation.cs) 在成功或失败后归还预留；[在途调用与端点移除回归测试](../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs)覆盖这一交接。
- `dubbo://`：从 [NativeDubboActuatorSuite](../../DubboService/NativeDubboActuatorSuite.cs) 继续读 [NativeDubboConnection](../../DubboService/Native/NativeDubboConnection.cs) 与 [NativeDubboCodec](../../DubboService/Native/NativeDubboCodec.cs)，再看[编解码测试](../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs)及[原生协议说明](../NativeDubboProtocol.md)。
- `tri://`：从 [TripleDubboActuatorSuite](../../DubboService/TripleDubboActuatorSuite.cs) 读到[传输测试](../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs)和 [Triple 协议说明](../TripleProtocol.md)。

## 其他入口

- 用法与兼容边界仍看[中文 README](../../readme_cn.md)，这里不复制配置和协议限制。
- 真实 Java 测试环境看[端到端环境指南](../../Tests/EndToEndTestEnvironment/README.md)，测试 Provider 看 [JavaProviders](../../Tests/EndToEndTestEnvironment/JavaProviders/README.md)。
- 设计原因参见 [RFC 约定](../RFC.md)；发布与待发布变更参见 [Git 索引式 Changelog](../../CHANGELOG.md)。两者均不代替源码和测试。
