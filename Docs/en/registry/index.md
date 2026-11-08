# Registry and discovery

[Code map](../index.md) · [中文](../../zh/registry/index.md)

## Purpose and background

Before a call can be routed, DubboNet needs the remote Providers for a Java interface, and it needs to notice when they change. The [DubboClient discovery path](../../../Clients/DubboClient.cs) uses [MyZookeeper](../../../Clients/RegistryClient/MyZookeeper.cs) or [MyNacos](../../../Clients/RegistryClient/MyNacos.cs). Registry access locates Providers; it does **not** itself execute a business RPC or provide a .NET server.

## Upstream and downstream

The [client](../client/index.md) asks for endpoints on first use or refresh; [Metadata](../metadata/index.md) also requests Provider information to locate definitions. `DubboClient.GetSeviceProviderEndPointsAsync` queries the selected backend, while `ConcurrentGetProviderEndPoints` coalesces in-flight requests and is not a second persistent Provider list. ZooKeeper's [watcher](../../../Clients/DubboClientZookeeperWatcher.cs) and the Nacos callback feed changes back to client refresh; refreshed endpoints then update the [service driver](../selection/index.md), with cached metadata refreshed as appropriate.

## What crosses the boundary

- **Input and connection:** registry address, service interface and the applicable registry root/group. A metadata-center address can be configured separately; it is not the Provider business socket.
- **Output:** Provider endpoint data (including routing attributes) and change notifications. An initial metadata lookup can use the same discovery path **without** creating a service driver.
- **Ownership:** the client retains registry references and subscriptions; the driver owns the callable state derived from discovered endpoints. Do not confuse consumer registration with publishing a Provider.

## How to use it

Normal business callers configure discovery through `DubboClient`; the `nacos://` prefix chooses Nacos rather than ZooKeeper:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("nacos://127.0.0.1:8848");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });
```

For registry groups, a separate metadata center, and optional consumer registration, see the [configuration guide](../../../README.md#configuration). If a diagnostic tool needs a raw Nacos directory rather than an RPC call, use the documented public `MyNacos.ListServicesAsync` / `GetProvidersAsync` APIs in the [direct directory example](../../../README.md#direct-nacos-directory-access); do not treat those listing calls as Driver state. Read [ZooKeeper tests](../../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs), [Nacos tests](../../../Tests/UnitTestForDubboNet/MyNacosTest.cs) and [consumer-registration tests](../../../Tests/UnitTestForDubboNet/ConsumerRegistrationTest.cs); run real fixtures via the [environment guide](../../../Tests/EndToEndTestEnvironment/README.md).
