# Client: Entry and Orchestration Boundary

[Call-path overview](index.md) · [中文](../../zh/architecture/client.md)

## Follow the entry points

| Source location | Question to answer |
| --- | --- |
| [`DubboClient.QueryAsync`](../../../Clients/DubboClient.cs) | How do the public overloads construct a `DubboInvocation`, and when is [metadata](metadata.md) needed to infer Java types? |
| [`QueryInternalAsync`](../../../Clients/DubboClient.cs) | How is a protocol restriction checked and a [service driver](selection.md) asked to reserve a call? When does a missing driver/candidate lead to [discovery](registry.md) and another selection, or to a failure result? |
| [`ReflushProviderAsync` / `HandleNacosProviderChangedAsync`](../../../Clients/DubboClient.cs) | How does a Provider event update an existing driver and, when needed, cached metadata rather than replaying the business call? |
| [`TryDoConnectRegistryTaskAsync` / `ReLoadDubboDriverCollection`](../../../Clients/DubboClient.cs) | Who attempts ZooKeeper reconnect and reloads services? Events enter through [`DubboClientZookeeperWatcher.process`](../../../Clients/DubboClientZookeeperWatcher.cs). |
| [`Dispose`](../../../Clients/DubboClient.cs) | When are consumer registration, the metadata manager, registry references, drivers, and shared actuators released? |

## Handoffs and exclusions

The current client handles the entry point, discovery orchestration, reconnect, and disposal. This page describes **existing code**, not a completed decomposition. [Metadata](metadata.md) supplies Java signature definitions; [selection](selection.md) owns callable Provider state and reservations; [transport](transport.md) encodes requests. Drawing refresh and shutdown paths here does not mean their failure or concurrent-close edge cases are solved; see [#37](https://github.com/apache/dubbo-net/issues/37).

**Verify:** [public API contract tests](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs), [driver tests](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs); live connections need the [end-to-end environment](../../../Tests/EndToEndTestEnvironment/README.md). Usage examples belong in the [user guide](../../../README.md), not on this architecture page.
