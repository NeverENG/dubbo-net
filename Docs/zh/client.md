# 客户端入口

[代码地图](index.md) · [English](../en/client.md)

[`DubboClient`](../../Clients/DubboClient.cs) 提供公开的 `QueryAsync`，装配注册中心与元数据连接，并协调发现、选择、发送、刷新和关闭。先读 `QueryWithInferredTypesAsync`／`QueryAsync(DubboInvocation)`，再读 `QueryInternalAsync`：前者准备调用对象，后者选择 Provider，在需要时发现端点并发送。这是在描述当前职责，并非认可所有职责都应继续放在同一处。

**交接点：** 签名推断进入[元数据](metadata.md)，Provider 查询进入[注册与发现](registry.md)，选中 Driver 后进入[选择与预留](selection.md)与[协议传输](transport.md)。[ZooKeeper watcher](../../Clients/DubboClientZookeeperWatcher.cs)和 Nacos 回调走独立的刷新路径；调整 `DubboClient.Dispose` 的释放顺序前应先看清各组件的所有权。

**源码与验证：** [DubboClient 源码](../../Clients/DubboClient.cs) · [公开 API 测试](../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs) · [用法与配置](../../readme_cn.md) · [端到端环境指南](../../Tests/EndToEndTestEnvironment/README.md)。指南中的 Java Provider 是测试设施，不是 .NET 服务端实现。
