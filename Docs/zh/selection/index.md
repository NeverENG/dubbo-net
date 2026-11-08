# 服务选择：从候选中预留一次调用

[代码地图](../index.md) · [English](../../en/selection/index.md)

假设注册中心返回了三个 `JobService` Provider，客户端不能随便挑一个：版本、分组和可能的协议限制必须匹配，还要按配置的负载均衡策略作出选择。这部分维护的是**当前可调用的端点**，不是 Java 方法签名；方法签名属于[元数据](../metadata/index.md)。

## 选中之后发生什么

[注册发现](../registry/index.md)交来端点后，客户端为服务建立 Driver；首次调用尚无 Driver 时会先发现、建立，再尝试选择。Provider 变化则更新已有 Driver。Driver 从符合条件的候选中选出执行器，并为本次调用作一次**预留**，随后交给[协议传输](../transport/index.md)。如果暂时没有可用端点，客户端会返回失败结果，而不是让应用直接处理 Driver。

预留的作用是保护正在使用的连接：即使某个 Provider 此时下线，也不能在请求尚未结束时提前清理执行器。发送成功或失败后，预留都会归还。不同服务可能共享执行器，Driver 管理的是各自服务的候选状态。

## 如何选择策略

应用通过 `DubboClient.DubboClientConf.NowLoadBalanceMode` 配置策略，然后照常使用 `QueryAsync`，不用直接创建 Driver。例如：

```csharp
using DubboNet.Clients;

using var client = new DubboClient(
    "127.0.0.1:2181",
    new DubboClient.DubboClientConf
    {
        NowLoadBalanceMode = DubboClient.LoadBalanceMode.LeastActive
    });
```

`LeastActive` 依据**当前消费者**看到的在途调用量，不表示全局服务端负载。其他策略见[配置说明](../../../readme_cn.md#客户端配置--configuration)，调用方式见[客户端入口](../client/index.md)。

**核对实现：** [Driver 集合](../../../Clients/DubboDriverCollection.cs)、[服务 Driver](../../../Clients/DubboServiceDriver.cs)、[调用预留](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs)；[Driver 测试](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)、[负载均衡测试](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs)与[端点移除回归测试](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs)。
