# Registry and discovery: where is this service?

[Code map](../index.md) · [中文](../../zh/registry/index.md)

When the application requests `JobService`, the client does not yet know which hosts provide it. A registry supplies Provider addresses and routing attributes and reports changes to those addresses. It answers **who can be reached**; it does not send business requests. Registering as a consumer does not publish a .NET Provider service.

## From first lookup to updates

On a first business call without a service driver, `DubboClient` discovers endpoints by service name and hands them to [service selection](../selection/index.md) to establish callable state. Concurrent discovery for the same service shares an in-flight lookup. A [metadata lookup](../metadata/index.md) also needs Provider information, but discovery for definitions alone does not create a business-call driver.

When Providers come or go, a ZooKeeper watcher or Nacos callback feeds changes back to the client. The client updates existing callable endpoints and refreshes cached metadata as appropriate. This **update path** runs separately from `QueryAsync`. The client retains registry connections and subscriptions; the driver holds service candidates derived from discovery.

## Connecting to a registry

Pass a registry address when constructing `DubboClient`: a regular `host:2181` address uses ZooKeeper, while the `nacos://host:8848` prefix selects Nacos. Applications then call `QueryAsync` as usual; they need not look up Providers manually. See the [configuration guide](../../../README.md#configuration) for registry groups, a separate metadata center and optional consumer registration.

To inspect a raw Nacos directory without sending an RPC, use the public `MyNacos.ListServicesAsync` or `GetProvidersAsync` methods; see the [directory example](../../../README.md#direct-nacos-directory-access). Directory listings are not themselves the driver's callable state.

**Check the implementation:** [client discovery](../../../Clients/DubboClient.cs), [ZooKeeper](../../../Clients/RegistryClient/MyZookeeper.cs), [Nacos](../../../Clients/RegistryClient/MyNacos.cs); [ZooKeeper tests](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs) and [Nacos tests](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs).
