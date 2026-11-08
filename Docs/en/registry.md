# Registry and Provider discovery

[Code map](index.md) · [中文](../zh/registry.md)

[`DubboClient.GetSeviceProviderEndPointsAsync`](../../Clients/DubboClient.cs) asks [`MyZookeeper`](../../Clients/RegistryClient/MyZookeeper.cs) or [`MyNacos`](../../Clients/RegistryClient/MyNacos.cs) for Provider endpoints. Its `ConcurrentGetProviderEndPoints` coordinates in-flight lookups; it is not another persistent Provider list. On an initial call the discovered endpoints can populate a [service driver](selection.md). [Metadata](metadata.md) also requests endpoints to locate a definition, without thereby creating a driver.

**Changes:** [`DubboClientZookeeperWatcher`](../../Clients/DubboClientZookeeperWatcher.cs) and the Nacos callback notify the [client](client.md), which may refresh a cached driver or cached metadata. Follow that event path separately from first-call discovery. Registration connections and consumer registration are different from the business connection to a Provider.

**Read and verify:** [discovery and refresh](../../Clients/DubboClient.cs) · [ZooKeeper tests](../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs) · [Nacos tests](../../Tests/UnitTestForDubboNet/MyNacosTest.cs) · [consumer-registration tests](../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs) · [test environment](../../Tests/EndToEndTestEnvironment/README.md). For registry configuration and supported variants, consult the [README](../../README.md), not this code map.
