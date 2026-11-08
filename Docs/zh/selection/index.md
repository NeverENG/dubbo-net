# 服务选择：从多个 Provider 里选一个

[代码地图](../index.md) · [English](../../en/selection/index.md)

发现 `JobService` 有多个 Provider 之后，还不能直接发送请求：要按版本、分组及可能的协议限制筛选候选，再用配置的负载均衡策略选出一个。客户端为每个服务维护 Driver；它持有可调用端点状态，而不是重复保存 Java 方法元数据。

首次请求如果还没有该服务的 Driver，客户端会先[发现端点](../registry/index.md)，创建 Driver 后再尝试选择；注册变化则更新现有 Driver。一次成功选择会产生执行器和**单次调用预留**，交给[传输层](../transport/index.md)。即使发送失败，调用结束也会归还预留，这样下线的端点不会在已有请求仍使用连接时被提前清理。选不到可用端点时，客户端返回相应失败结果；应用不直接操纵 Driver。

要改变选择策略，配置 `DubboClient.DubboClientConf.NowLoadBalanceMode`，然后照常调用 `QueryAsync`，不需要手动创建 Driver。例如设为 `DubboClient.LoadBalanceMode.LeastActive`；该策略根据**当前消费者**观察到的在途请求选择，并不测量全局服务端负载。完整配置和调用示例分别见[使用手册](../../../readme_cn.md#客户端配置--configuration)与[客户端入口](../client/index.md)。

**从代码继续读：** [Driver 集合](../../../Clients/DubboDriverCollection.cs)、[服务 Driver](../../../Clients/DubboServiceDriver.cs)、[单次调用预留](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs)；对照 [Driver 测试](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)、[负载均衡测试](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs)与[端点移除回归测试](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs)。
