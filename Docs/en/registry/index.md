# Registry: finding Providers and tracking changes

[Code map](../index.md) · [中文](../../zh/registry/index.md)

The client knows it wants `JobService`, but not which host serves it. The registry answers “which Providers exist, at what addresses, with which routing attributes”; it **does not** send the business request. The current implementation supports ZooKeeper and Nacos. Pass `nacos://127.0.0.1:8848` to the client constructor to select Nacos; an ordinary address without that prefix uses ZooKeeper. See the [user guide](../../../README.md#configuration) for full configuration.

On a first call without a service driver, the client looks up endpoints by service name and hands them to [Selection](../selection/index.md). A [metadata lookup](../metadata/index.md) may also discover Providers, but metadata discovery alone does not create a business-call driver. Concurrent lookups for the same service share an in-flight task; this is not a second persistent Provider list.

When Providers come or go, a ZooKeeper watcher or Nacos callback feeds changes back to the client, updating callable endpoints and refreshing metadata as needed. This update path is separate from `QueryAsync`. The client holds registry connections and subscriptions; a driver holds callable state built from discovery. Optional **consumer registration** does not publish a Provider service.

Need the raw Nacos directory rather than an RPC call? See the [directory example](../../../README.md#direct-nacos-directory-access) for the public `MyNacos.ListServicesAsync` and `GetProvidersAsync` APIs.

**Follow the code:** [client discovery](../../../Clients/DubboClient.cs), [ZooKeeper](../../../Clients/RegistryClient/MyZookeeper.cs), [Nacos](../../../Clients/RegistryClient/MyNacos.cs), [change watcher](../../../Clients/DubboClientZookeeperWatcher.cs); compare the [ZooKeeper tests](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs), [Nacos tests](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs), and [consumer-registration tests](../../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs).
