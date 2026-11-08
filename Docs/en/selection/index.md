# Service selection: reserving a Provider for one call

[Code map](../index.md) · [中文](../../zh/selection/index.md)

Suppose the registry returns three Providers for `JobService`. The client cannot choose arbitrarily: version, group and any protocol restriction must match, and the configured load-balancing policy determines which candidate to use. This part maintains **currently callable endpoints**, not Java method signatures; signatures belong to [Metadata](../metadata/index.md).

## What happens after selection

Once [discovery](../registry/index.md) supplies endpoints, the client builds a driver for the service. If no driver exists on the first call, it discovers endpoints, creates one and retries selection. Provider changes update existing drivers. A driver chooses a suite among matching candidates and makes a **per-call reservation** before passing it to [transport](../transport/index.md). If no endpoint is currently usable, the client returns a failure result; applications do not manage drivers directly.

A reservation protects an in-use connection: a Provider going away must not retire its suite while a request is still using it. The reservation is released when dispatch ends, even if sending fails. Different services may share a suite; each driver maintains its own service candidates.

## Choosing a policy

Configure `DubboClient.DubboClientConf.NowLoadBalanceMode`, then use `QueryAsync` normally; do not construct a driver yourself. For example:

```csharp
using DubboNet.Clients;

using var client = new DubboClient(
    "127.0.0.1:2181",
    new DubboClient.DubboClientConf
    {
        NowLoadBalanceMode = DubboClient.LoadBalanceMode.LeastActive
    });
```

`LeastActive` uses in-flight activity observed by **this consumer**, not global server load. See the [configuration guide](../../../README.md#configuration) for other policies and the [client entry point](../client/index.md) for the call itself.

**Check the implementation:** [service driver](../../../Clients/DubboServiceDriver.cs), [call reservation](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs); [driver tests](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs) and [endpoint-removal regression](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs).
