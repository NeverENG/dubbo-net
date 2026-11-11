# Registry and Discovery: Where Addresses Come From

[Call-path overview](index.md) · [中文](../../zh/architecture/registry.md)

## Responsibility and boundary

Registry discovery answers “which Provider addresses currently serve this Java interface?” It reads classic interface-level ZooKeeper `providers` nodes or Nacos Naming instances, returns endpoints to its caller, and establishes a path for change notifications. **A discovery result is not a second cache of callable Providers**: the [service driver](selection.md) still owns and selects the candidates for business calls. Java method definitions belong to [metadata](metadata.md); sending requests belongs to [transport](transport.md).

On the current `main`, these operations still live in `DubboClient`: `ConcurrentGetProviderEndPoints` tracks in-flight lookups, `GetSeviceProviderEndPointsAsync` queries the registry, and `ReflushProviderAsync` decides when to update a driver. The `Clients/` location describes today's source, not a permanent ownership decision. [#36](https://github.com/apache/dubbo-net/pull/36), still under review, changes source ownership; this page does not present it as merged behavior.

## First call and metadata lookup

If `QueryInternalAsync` cannot find a service driver, it looks up Providers by service name through `ConcurrentGetProviderEndPoints`. Only a successful discovery leads to `DubboDriverCollection.AddDubboServiceDriver`, followed by another selection attempt. A call with an existing driver but no endpoints also queries once: it updates and reselects if Providers appear, otherwise returns a failure result. **An existing candidate set with no available actuator** is a different selection failure; repeatedly discovering Providers is not its remedy. Follow [`DubboClient.QueryInternalAsync`](../../../Clients/DubboClient.cs) and [`DubboDriverCollection.GetDubboActuatorSuite`](../../../Clients/DubboDriverCollection.cs).

Before reading Java method definitions, [`DubboMetadataManager`](../../../Clients/DubboMetadataManager.cs) may also use `GetProviderEndPointsForMetadataAsync` to locate Providers. This reuses the discovery path but **does not create a business-call driver**. Looking up a service for metadata therefore does not imply that it has callable candidates.

| Source | Current query and result | Change notification |
| --- | --- | --- |
| ZooKeeper | Checks the interface's `providers` path, reading and parsing `dubbo://` and `tri://` children. Missing nodes/endpoints return error messages; exhausted connection retries can throw. | The nonempty-children branch calls `GetChildrenAsync` with `DubboClientZookeeperWatcher`, installing a **one-shot** children watch. |
| Nacos Naming | Finds services for an interface and group, converting only healthy, enabled instances to endpoints. No healthy endpoints and query failures produce errors. | `MyNacos.GetProviderEndPointsAsync` registers a Naming change callback under this client. |

Both sources meet in [`GetSeviceProviderEndPointsAsync`](../../../Clients/DubboClient.cs); Nacos filtering and subscription live in [`MyNacos.GetProviderEndPointsAsync`](../../../Clients/RegistryClient/MyNacos.cs). A returned error message becomes a failure result on the call path, while an exhausted ZooKeeper SDK retry may still throw; not every failure is an empty Provider list. The current in-flight task reuse should also not be read as a guarantee that exactly one SDK query starts under every concurrency timing.

## Change notifications and state refresh

On a Provider-children event, `DubboClientZookeeperWatcher.process` parses the service path: an existing driver leads to `ReflushProviderAsync` by path; a metadata-only cache leads to a cache refresh; with neither, it does not build a new driver. Nacos `HandleNacosProviderChangedAsync` uses the same existing-driver/metadata-only distinction. **A change event does not replay the business `QueryAsync`.** Follow the [ZooKeeper watcher](../../../Clients/DubboClientZookeeperWatcher.cs) and [Nacos callback](../../../Clients/DubboClient.cs).

When discovery returns new endpoints, `ReflushProviderAsync` updates the driver and requests a refresh of any cached service metadata. An explicit “no Provider / no healthy Provider” result updates an existing driver's candidates to empty; an ordinary lookup error is logged rather than interpreted as “all Providers are down.” Disconnection and session expiration enter the watcher's reconnect path; after recovery it attempts to rediscover each service. Callback and reconnect are separate asynchronous entries; this does not imply that they are fully drained during client disposal.

## Subscriptions and shutdown

The current `DubboClient` registers Nacos Naming subscriptions under its own subscriber identity. On `Dispose`, it calls `RemoveSubscriber(this)` before releasing its registry connection reference. The metadata manager owns and removes its separate Nacos **Config** subscriber; removing a Naming subscriber must not remove Config subscriptions. `MultiMyZookeeperStorage` / `MultiMyNacosStorage` reuse underlying connections by address and count references. The metadata center may reuse a same-address registry connection; a distinct address holds another reference. Disposal releases **this client's references**, not the connections of other clients sharing them.

ZooKeeper child watches are one-shot notifications without a per-watcher unsubscribe API; the callback checks whether the client is disposed. If the Provider path **exists but has zero children**, the current discovery method returns before installing a children watch. A Provider appearing later might not trigger automatic refresh; the next business call still attempts active discovery. The failed-reload guard and shutdown-interleaving risks remain open in [#37](https://github.com/apache/dubbo-net/issues/37). This page records the current behavior, not a completed fix.

## Source and verification

| File | Responsibility relevant here |
| --- | --- |
| [`Clients/DubboClient.cs`](../../../Clients/DubboClient.cs) | Query handoff, errors, driver/metadata refresh routing, Nacos subscriber identity, and registry-reference release. |
| [`Clients/DubboClientZookeeperWatcher.cs`](../../../Clients/DubboClientZookeeperWatcher.cs) | Children, disconnection, and expired-session events; does not own callable Provider candidates. |
| [`Clients/RegistryClient/MyZookeeper.cs`](../../../Clients/RegistryClient/MyZookeeper.cs) / [`MyNacos.cs`](../../../Clients/RegistryClient/MyNacos.cs) | SDK queries, connection retries, and Nacos Naming notifications keyed by subscriber. |
| [`Clients/RegistryClient/MultiMyZookeeperStorage.cs`](../../../Clients/RegistryClient/MultiMyZookeeperStorage.cs) / [`MultiMyNacosStorage.cs`](../../../Clients/RegistryClient/MultiMyNacosStorage.cs) | Address-keyed connection reuse and reference counts, not the service driver's candidate set. |

[ZooKeeper unit tests](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs) check the unavailable-connection error boundary; [Nacos tests](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs) cover instance lookup, subscriber isolation, and directory changes. They do not replace a live test of empty-node transitions and full reconnect; see the [end-to-end environment guide](../../../Tests/EndToEndTestEnvironment/README.md) for prerequisites.
