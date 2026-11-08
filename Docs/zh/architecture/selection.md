# 服务选择：候选、预留与归还

[调用链总览](index.md) · [English](../../en/architecture/selection.md)

## 从发现结果到一次发送

| 源码入口 | 此处拥有的状态或动作 |
| --- | --- |
| [`DubboDriverCollection.AddDubboServiceDriver`](../../../Clients/DubboDriverCollection.cs) | [注册发现](registry.md)把端点交来后，按服务建立或更新 Driver；注册查询结果本身不参与负载均衡。 |
| [`DubboDriverCollection.GetDubboActuatorSuite`](../../../Clients/DubboDriverCollection.cs) | [客户端](client.md)以 `reserve: true` 请求可用执行器；结果区分无 Driver、无候选、资源不可用与已选中。 |
| [`DubboServiceDriver.SelectAndReserve`](../../../Clients/DubboServiceDriver.cs) | 从当前候选中按协议、版本、分组及负载策略选端点，同时增加在途引用；选中与预留在同一同步边界内。 |
| [`LoadBalancedInvocation.InvokeAsync`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs) | 包裹[传输](transport.md)调用；成功或失败都完成统计并归还预留，避免 Provider 刚下线时提前销毁正在使用的连接。 |

Driver 是**当前可调用 Provider 候选的所有者**；元数据管理器保存 Java 签名，注册发现只返回地址。跨服务的执行器可能共享，服务自己的候选与方法统计仍由对应 Driver 维护。如果选不到执行器，客户端按结果类型决定是否发现/重试或返回失败；这不等于注册中心一定无节点。

**核对：** [Driver 选择测试](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)、[负载策略测试](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs)、[Provider 移除时在途调用回归](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs)。策略配置在[使用说明](../../../readme_cn.md#客户端配置--configuration)。
