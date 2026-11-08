# Registry and Discovery: Addresses, Notifications, References

[Call-path overview](index.md) · [中文](../../zh/architecture/registry.md)

## Current entries and handoffs

| Entry | What it passes downstream |
| --- | --- |
| [`DubboClient.ConcurrentGetProviderEndPoints`](../../../Clients/DubboClient.cs) | Both requests and metadata lookup can discover a service here; the current client also coalesces in-flight tasks. The endpoints are not a second callable-Provider snapshot. |
| [`GetSeviceProviderEndPointsAsync`](../../../Clients/DubboClient.cs) | Looks up ZooKeeper children or Nacos Naming instances, returning endpoints and errors; the [client](client.md) decides whether to update a driver. |
| [`DubboClientZookeeperWatcher.process`](../../../Clients/DubboClientZookeeperWatcher.cs) / [`HandleNacosProviderChangedAsync`](../../../Clients/DubboClient.cs) | Changes ask the client to refresh an existing driver or a metadata-only cache; disconnections enter the reconnect path. |

Reading ZooKeeper children installs a **one-shot watch**, not an independently owned permanent subscription. The current empty-Provider-children path does not install that watch; [#37](https://github.com/apache/dubbo-net/issues/37) tracks it. “Service exists” therefore does not mean every later change is automatically observed. Nacos Naming and metadata Config subscriptions have different owners. Shared storage reuses underlying SDK connections; the client holds and releases references on `Dispose`, but does not own an exclusive connection.

## State boundary and verification

Registry lookup returns addresses and routing properties; [`DubboDriverCollection.AddDubboServiceDriver`](../../../Clients/DubboDriverCollection.cs) hands them to [selection](selection.md) for callable state. Discovery for definitions does **not** create a business-call driver; see [metadata](metadata.md).

**Read:** [ZooKeeper SDK wrapper](../../../Clients/RegistryClient/MyZookeeper.cs), [Nacos SDK wrapper](../../../Clients/RegistryClient/MyNacos.cs), [shared connection storage](../../../Clients/RegistryClient/MultiMyZookeeperStorage.cs). **Verify:** [ZooKeeper tests](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs), [Nacos tests](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs).
