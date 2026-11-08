# 服务 Driver 与 Provider 选择

[代码地图](../index.md) · [English](../../en/selection/index.md)

## 功能与背景

注册中心可能为同一服务返回多个 Provider。发送请求前，客户端需要筛选候选、选中一个，并在本次在途调用期间保留其连接。[`DubboDriverCollection`](../../../Clients/DubboDriverCollection.cs)保存服务到 Driver 的对应关系；[`DubboServiceDriver`](../../../Clients/DubboServiceDriver.cs)保存该服务的端点状态、候选集和选择逻辑。这是可调用状态，不是第二份元数据缓存。

## 上下游如何衔接

[服务发现](../registry/index.md)提供端点快照。首次使用时，[`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs)发现 Driver 缺失会创建它并重新选择；注册变化事件更新既有 Driver。`DubboServiceDriver.SelectAndReserve` 选择匹配的 Provider，构造 [`LoadBalancedInvocation`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs)。选中的执行器进入[协议传输](../transport/index.md)；无论发送成功或失败，调用完成后都会归还预留。移除端点不能提前释放已预留调用需要的连接。

## 输入、产物与所有权

- **输入：** 服务名、端点快照、调用路由属性、可选协议限制、负载均衡配置和请求键。
- **产物：** 内部选择结果，包含执行器及单次调用的预留；找不到可用执行器时带有相应原因。应用仍通过 `DubboClient` 获取 `DubboRequestResult`，不会直接持有 Driver。
- **所有权：** Driver 管服务候选；不同服务可能共享执行器。客户端的在途发现任务不是另一份候选缓存。

## 如何使用

通过客户端配置选择策略，不需要直接创建内部 Driver：

```csharp
using DubboNet.Clients;

using var client = new DubboClient(
    "127.0.0.1:2181",
    new DubboClient.DubboClientConf
    {
        NowLoadBalanceMode = DubboClient.LoadBalanceMode.LeastActive
    });
```

随后按[客户端导读](../client/index.md)调用 `client.QueryAsync(...)`。此模式依据当前消费者观察到的在途调用，并不代表全局服务端负载。其他策略见[配置说明](../../../readme_cn.md#客户端配置--configuration)；当前选择与资源契约看 [Driver 测试](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)、[负载均衡测试](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs)及[在途端点移除测试](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs)。
