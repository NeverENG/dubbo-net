# 服务 Driver 与选择

[代码地图](index.md) · [English](../en/selection.md)

[`DubboDriverCollection`](../../Clients/DubboDriverCollection.cs) 保存服务名到 Driver 的对应关系，在 [Provider 发现](registry.md)后创建或更新 Driver。[`DubboServiceDriver`](../../Clients/DubboServiceDriver.cs) 保存每项服务的端点状态与派生候选集；`SelectAndReserve` 选择 Provider 并预留一次在途调用。客户端的在途发现任务与 Driver 的 Provider 状态用途不同。

**交接点：** [`DubboClient.QueryInternalAsync`](../../Clients/DubboClient.cs) 通过集合选择，再交给[协议执行器](transport.md)发送。[`LoadBalancedInvocation`](../../Clients/LoadBalancing/LoadBalancedInvocation.cs) 在成功或失败后归还预留；Provider 变化不能提前释放已预留调用需要的连接。客户端关闭时也要沿 Driver 与集合的释放路径阅读。

**源码与验证：** [Driver 测试](../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs) · [负载均衡测试](../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs) · [在途调用与端点移除回归测试](../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs)。公开负载均衡配置请看[中文 README](../../readme_cn.md)。
