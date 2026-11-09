# Source Navigation: Calls and Updates

[Documentation home](../index.md) · [中文](../../zh/architecture/index.md) · [User guide](../../../README.md)

This map describes the **current implementation**, not a proposed architecture. DubboNet is a consumer of remote Java Providers. The two paths below can be read independently; conditional steps do not run on every request. For API usage, start with the [user guide](../../../README.md); wire details belong to the [native](../protocols/native-dubbo.md) and [Triple](../protocols/triple.md) pages.

## Request path: `QueryAsync` to result

```text
DubboClient.QueryAsync
  ├─ Java types missing → DubboInvocationMetadataResolver.CreateAsync → DubboMetadataManager → signature resolution
  └─ QueryInternalAsync → GetRequiredProtocolAsync (best-effort overload check)
       → DubboDriverCollection.GetDubboActuatorSuite(reserve: true)
          ├─ no driver → ProviderDiscovery.GetProviderEndPointsAsync → AddDubboServiceDriver → select again
          ├─ driver with no Providers → discover again; retry only if Providers appear
          ├─ candidates present but no available actuator → failure result
          └─ selected → LoadBalancedInvocation.InvokeAsync → actuator SendQuery → DubboRequestResult
```

| Start reading here | Question and next page |
| --- | --- |
| [`DubboClient.QueryAsync` / `QueryInternalAsync`](../../../Clients/DubboClient.cs) | Where do arguments enter? When are types inferred, protocol checked, and initial discovery triggered? → [Client](client.md) |
| [`DubboInvocationMetadataResolver.CreateAsync`](../../../Metadata/DubboInvocationMetadataResolver.cs) / [`DubboMetadataManager.GetServiceMethodsAsync`](../../../Metadata/DubboMetadataManager.cs) | Where do Java signatures come from, and what if they are unavailable? → [Metadata](metadata.md) |
| [`ProviderDiscovery.GetProviderEndPointsAsync` / `DiscoverAsync`](../../../Registry/ProviderDiscovery.cs) | Who looks up addresses and installs subscriptions? → [Registry and discovery](registry.md) |
| [`DubboDriverCollection.GetDubboActuatorSuite`](../../../Clients/DubboDriverCollection.cs) / [`DubboServiceDriver.SelectAndReserve`](../../../Clients/DubboServiceDriver.cs) | Which state is selectable, and who protects in-flight calls? → [Selection and reservation](selection.md) |
| [`LoadBalancedInvocation.InvokeAsync`](../../../Clients/LoadBalancing/LoadBalancedInvocation.cs) | How does a reservation wrap dispatch and release? → [Transport](transport.md) |

Metadata lookup **may itself discover Providers**; a call with an existing driver need not query the registry. The diagram is a map of branches, not a mandatory request pipeline.

## Update path: event to existing state

```text
ZooKeeper: DubboClientZookeeperWatcher.process ─┐
Nacos: HandleNacosProviderChangedAsync ───────────┼─ existing driver → ReflushProviderAsync
                                                  │                     → discover → update driver/metadata cache
                                                  └─ only metadata cached → RefreshCachedMetadataAsync
Reconnect: StartConnecTaskAsync → TryDoConnectRegistryTaskAsync → ReLoadDubboDriverCollection
Shutdown: DubboClient.Dispose → unsubscribe/release registry references, drivers, and actuators
```

[Registry and discovery](registry.md) covers notifications and subscriptions; [Client](client.md) covers reconnect and disposal. This map does not claim that every event, failure, or concurrent shutdown is already handled; see [#37](https://github.com/apache/dubbo-net/issues/37) for pending verification of recovery refreshes and empty-node watches.

## State ownership (current implementation)

| State | Current owner | Not the same as |
| --- | --- | --- |
| Callable Provider candidates, selection, and reservation | [`DubboServiceDriver`](../../../Clients/DubboServiceDriver.cs) and [`DubboDriverCollection`](../../../Clients/DubboDriverCollection.cs) | A cache of registry lookup results |
| Java method and type definitions | [`DubboMetadataManager`](../../../Metadata/DubboMetadataManager.cs) | A list of callable Providers |
| In-flight discovery and Provider subscriptions | [`ProviderDiscovery`](../../../Registry/ProviderDiscovery.cs) | Callable Provider candidates in the driver |
| Registry connection references and client actuator lifetime | [`DubboClient`](../../../Clients/DubboClient.cs); the underlying connections are shared by storage | Exclusive connection ownership or an individually removable ZooKeeper watch |

Check the [public API](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs), [metadata](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs), and [driver](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs) tests for each path; the [end-to-end environment guide](../../../Tests/EndToEndTestEnvironment/README.md) covers live Provider prerequisites.
