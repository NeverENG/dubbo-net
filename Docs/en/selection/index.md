# Service drivers and Provider selection

[Code map](../index.md) · [中文](../../zh/selection/index.md)

## Purpose and background

A registry can return several Providers for one service. Before sending a request, the client needs to filter candidates, choose one and keep its connection alive for this in-flight call. [`DubboDriverCollection`](../../../Clients/DubboDriverCollection.cs) maps services to drivers; [`DubboServiceDriver`](../../../Clients/DubboServiceDriver.cs) holds one service's endpoint state, candidate sets and selection logic. This is callable state, not a duplicate metadata cache.

## Upstream and downstream

[Discovery](../registry/index.md) supplies endpoint snapshots. On first use, [`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs) creates the service driver if missing and tries selection again; registry-change events update existing drivers. `DubboServiceDriver.SelectAndReserve` chooses a matching Provider and creates a [`LoadBalancedInvocation`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs). The selected suite goes to [Transport](../transport/index.md); completion releases the reservation even if dispatch fails. Endpoint removal must not dispose a connection still needed by an already reserved call.

## What crosses the boundary

- **Input:** service name, endpoint snapshot, invocation routing attributes, optional protocol requirement, configured load-balancing mode and request key.
- **Output:** an internal selection result with the chosen suite and per-call reservation, or a reason no usable actuator was found; callers receive a `DubboRequestResult` through `DubboClient`, not a public driver handle.
- **Ownership:** the driver owns service candidates; suites can be shared across services. The client's in-flight discovery tasks are not another candidate cache.

## How to use it

Select a policy through client configuration, not by directly constructing an internal driver:

```csharp
using DubboNet.Clients;

using var client = new DubboClient(
    "127.0.0.1:2181",
    new DubboClient.DubboClientConf
    {
        NowLoadBalanceMode = DubboClient.LoadBalanceMode.LeastActive
    });
```

Then call `client.QueryAsync(...)` as in the [client guide](../client/index.md). This mode uses this consumer's in-flight statistics; it does not measure global server load. See the [configuration guide](../../../README.md#configuration) for other policies and [driver tests](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs), [load-balancing tests](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs) and [in-flight removal regression](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs) for the current selection and lifetime contracts.
