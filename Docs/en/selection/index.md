# Service selection: choosing one Provider

[Code map](../index.md) · [中文](../../zh/selection/index.md)

Discovering several Providers for `JobService` does not tell us where to send this request. The client filters candidates by version, group and any protocol restriction, then applies the configured load-balancing policy. It maintains a driver per service holding callable endpoint state, not a duplicate of Java method metadata.

If the first call has no driver, the client [discovers endpoints](../registry/index.md), creates one and retries selection. Registry changes update existing drivers. A successful selection yields a suite and a **per-call reservation**, which accompanies dispatch to [Transport](../transport/index.md). The reservation is released even if sending fails, so removing an endpoint cannot retire a connection still used by an in-flight call. If no endpoint is usable, the client returns a failure result; applications do not manipulate drivers directly.

To change policy, configure `DubboClient.DubboClientConf.NowLoadBalanceMode`, then call `QueryAsync` as usual; do not construct a driver yourself. For example, choose `DubboClient.LoadBalanceMode.LeastActive`. That policy uses in-flight calls observed by **this consumer**, not global server load. See the [configuration guide](../../../README.md#configuration) and [client entry point](../client/index.md) for complete usage.

**Follow the code:** [driver collection](../../../Clients/DubboDriverCollection.cs), [service driver](../../../Clients/DubboServiceDriver.cs), [call reservation](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs); compare [driver tests](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs), [load-balancing tests](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs), and [endpoint-removal regression](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs).
