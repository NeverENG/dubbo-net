# Service driver and selection

[Code map](index.md) · [中文](../zh/selection.md)

[`DubboDriverCollection`](../../Clients/DubboDriverCollection.cs) maps service names to drivers and creates or updates them after [Provider discovery](registry.md). [`DubboServiceDriver`](../../Clients/DubboServiceDriver.cs) owns each service's endpoint state and derived candidates; its `SelectAndReserve` chooses a Provider and reserves an in-flight call. The client's in-flight discovery tasks and the driver's Provider state serve different purposes.

**Handoff:** [`DubboClient.QueryInternalAsync`](../../Clients/DubboClient.cs) selects via the collection, then [dispatches](transport.md) through the chosen suite. [`LoadBalancedInvocation`](../../Clients/LoadBalancing/LoadBalancedInvocation.cs) completes the reservation after success or failure; a Provider change must not prematurely dispose the connection needed by a reserved call. The driver/collection disposal path matters when the client shuts down.

**Read and verify:** [driver tests](../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs) · [load-balancing tests](../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs) · [in-flight removal regression](../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs). For public load-balancing configuration, use the [README](../../README.md).
