# 客户端：入口与编排边界

[调用链总览](index.md) · [English](../../en/architecture/client.md)

## 沿入口读

| 代码位置 | 读它要回答的问题 |
| --- | --- |
| [`DubboClient.QueryAsync`](../../../Clients/DubboClient.cs) | 公开重载怎样得到 `DubboInvocation`，何时要求[元数据](metadata.md)推断 Java 类型？ |
| [`QueryInternalAsync`](../../../Clients/DubboClient.cs) | 怎样检查协议约束、向[服务 Driver](selection.md)请求预留？没有 Driver 或没有候选时，何时转向[发现](registry.md)并再次选择？发现失败如何返回？ |
| [`ReflushProviderAsync` / `HandleNacosProviderChangedAsync`](../../../Clients/DubboClient.cs) | Provider 事件怎样更新已有 Driver、按需刷新元数据缓存，而不是重新执行调用？ |
| [`TryDoConnectRegistryTaskAsync` / `ReLoadDubboDriverCollection`](../../../Clients/DubboClient.cs) | ZooKeeper 断线后谁尝试重连、谁逐服务恢复？事件从 [`DubboClientZookeeperWatcher.process`](../../../Clients/DubboClientZookeeperWatcher.cs) 进入。 |
| [`Dispose`](../../../Clients/DubboClient.cs) | 何时释放消费者注册、元数据管理器、注册连接引用、Driver 与共享执行器？ |

## 交接与不负责的事

客户端现在同时承担入口、发现协调、重连与释放；这张文档只描述**当前代码**，不宣称上述职责已完成拆分。Java 签名的定义由[元数据](metadata.md)处理；可调用 Provider 的状态和预留由[服务选择](selection.md)处理；请求编码由[传输执行器](transport.md)处理。现有的刷新防重入与关闭竞态不应仅因这里画出路径就被视作已解决，跟踪见 [#37](https://github.com/apache/dubbo-net/issues/37)。

**核对：** [公开 API 契约测试](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)、[Driver 状态测试](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)；真实连接需要[端到端环境](../../../Tests/EndToEndTestEnvironment/README.md)。使用示例留在[使用说明](../../../readme_cn.md)，不在架构页重复。
