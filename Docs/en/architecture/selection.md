# Service Selection: Candidates, Reservation, Release

[Call-path overview](index.md) · [中文](../../zh/architecture/selection.md)

## From discovered endpoints to one dispatch

| Source entry | State or action owned here |
| --- | --- |
| [`DubboDriverCollection.AddDubboServiceDriver`](../../../Clients/DubboDriverCollection.cs) | After [discovery](registry.md) returns endpoints, creates or updates the service driver. Registry lookup results are not themselves load-balanced. |
| [`DubboDriverCollection.GetDubboActuatorSuite`](../../../Clients/DubboDriverCollection.cs) | The [client](client.md) requests an actuator with `reserve: true`; outcomes distinguish no driver, no candidates, unavailable resources, and a selected suite. |
| [`DubboServiceDriver.SelectAndReserve`](../../../Clients/DubboServiceDriver.cs) | Filters current candidates by protocol, version, group, and load-balancing mode, then increments the in-flight reference. Selection and reservation share one synchronization boundary. |
| [`LoadBalancedInvocation.InvokeAsync`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs) | Wraps [transport](transport.md); success or failure completes statistics and releases the reservation so removal of a Provider does not dispose an in-use connection too soon. |

The driver **owns callable Provider candidates**. Metadata owns Java signatures; registry discovery returns addresses. Actuators may be shared across services, while each service's candidates and method statistics remain with its driver. If no suite is selected, the client decides from the result category whether to discover/retry or return a failure; it does not always mean the registry has no node.

**Verify:** [driver selection tests](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs), [load-balancing tests](../../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs), [in-flight removal regression](../../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs). Strategy configuration belongs in the [user guide](../../../README.md#configuration).
