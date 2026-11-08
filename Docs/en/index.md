# Code map: consumer call path

[中文](../zh/index.md) · [English README](../../README.md) · [Chinese README](../../readme_cn.md)

This is a reading route, not a second specification of current behavior. Follow the linked code and tests for details. DubboNet's runtime path shown here calls remote Providers; the Java Providers under `Tests/` are test fixtures, not a .NET Provider implementation.

## 1. Start at the public call

- [DubboClient.QueryAsync](../../Clients/DubboClient.cs) accepts an endpoint or a `DubboInvocation`. The overloads converge on invocation preparation and `QueryInternalAsync`; [API tests](../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs) show the public call shapes.
- When Java parameter types must be inferred, `CreateMetadataInvocationAsync` uses [DubboMetadataManager](../../Clients/DubboMetadataManager.cs) and [DubboMethodMetadataResolver](../../Clients/DubboMethodMetadataResolver.cs). When types are explicit, inference can be skipped, but `QueryInternalAsync` can still consult metadata to check overload compatibility. Read [metadata tests](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs) and [contract tests](../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs).

## 2. Discover Providers and read metadata

- [DubboClient.GetSeviceProviderEndPointsAsync](../../Clients/DubboClient.cs) discovers endpoints through [MyZookeeper](../../Clients/RegistryClient/MyZookeeper.cs) or [MyNacos](../../Clients/RegistryClient/MyNacos.cs). The client's in-flight discovery task is shared by concurrent requests for the same service; it is not a second persistent Provider list. Start with [ZooKeeper tests](../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs) and [Nacos tests](../../Tests/UnitTestForDubboNet/MyNacosTest.cs).
- [DubboMetadataManager.LoadServiceMetadataAsync](../../Clients/DubboMetadataManager.cs) needs Provider information to locate metadata or probe Provider Telnet. Its Provider-resolver callback returns to `DubboClient` discovery; reading metadata is **not** the same step as installing a service driver. The [Telnet resolver](../../Clients/DubboTelnetMetadataResolver.cs) and [metadata tests](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs) cover the other source.
- Changes enter via the [ZooKeeper watcher](../../Clients/DubboClientZookeeperWatcher.cs) or the Nacos callback in `DubboClient`; [ReflushProviderAsync](../../Clients/DubboClient.cs) updates a cached driver and refreshes cached metadata. These asynchronous event paths are separate from a normal call.

## 3. Select, dispatch, release

- [DubboDriverCollection](../../Clients/DubboDriverCollection.cs) maps services to drivers and creates/updates them after discovery. [DubboServiceDriver](../../Clients/DubboServiceDriver.cs) maintains the service's endpoint state, derives candidates and selects/reserves a Provider; see [driver tests](../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs) and [load-balancing tests](../../Tests/UnitTestForDubboNet/LoadBalancingTest.cs).
- `QueryInternalAsync` dispatches through the selected suite. [LoadBalancedInvocation](../../Clients/LoadBalancing/LoadBalancedInvocation.cs) completes the reservation on success or failure; [in-flight removal regression tests](../../Tests/UnitTestForDubboNet/ImportReviewRegressionTest.cs) cover this handoff.
- For `dubbo://`, follow [NativeDubboActuatorSuite](../../DubboService/NativeDubboActuatorSuite.cs) → [NativeDubboConnection](../../DubboService/Native/NativeDubboConnection.cs) and [NativeDubboCodec](../../DubboService/Native/NativeDubboCodec.cs); see [codec tests](../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs) and the [Native protocol notes (Chinese)](../NativeDubboProtocol.md).
- For `tri://`, follow [TripleDubboActuatorSuite](../../DubboService/TripleDubboActuatorSuite.cs); see [transport tests](../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs) and the [Triple protocol notes (Chinese)](../TripleProtocol.md).

## Other entry points

- For usage and compatibility, return to the [README](../../README.md), rather than copying its configuration and protocol limits here.
- For running the real Java fixtures, see the [end-to-end environment guide](../../Tests/EndToEndTestEnvironment/README.md); for the fixtures themselves, see [JavaProviders](../../Tests/EndToEndTestEnvironment/JavaProviders/README.md).
- For design rationale, see the [RFC convention](../RFC.md). For released history and pending changes, see the [Git-indexed changelog](../../CHANGELOG.md); neither replaces the source or the linked tests.
