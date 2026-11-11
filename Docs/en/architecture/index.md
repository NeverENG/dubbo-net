# Source Navigation: Calls and Updates

[Documentation home](../index.md) · [中文](../../zh/architecture/index.md) · [User guide](../../../README.md)

DubboNet is a consumer of remote Java Providers. The two paths below can be read independently; conditional steps do not run on every request. For API usage, start with the [user guide](../../../README.md); wire details belong to the [native](../protocols/native-dubbo.md) and [Triple](../protocols/triple.md) pages.

## Request path: `QueryAsync` to result

```text
DubboClient.QueryAsync
  ├─ Java types missing → CreateMetadataInvocationAsync → DubboMetadataManager → signature resolution
  └─ QueryInternalAsync → GetRequiredProtocolAsync (best-effort overload check)
       → DubboDriverCollection.GetDubboActuatorSuite(reserve: true)
          ├─ no driver → ConcurrentGetProviderEndPoints → AddDubboServiceDriver → select again
          ├─ driver with no Providers → discover again; retry only if Providers appear
          ├─ candidates present but no available actuator → failure result
          └─ selected → LoadBalancedInvocation.InvokeAsync → actuator SendQuery → DubboRequestResult
```

| Start reading here | Question and next page |
| --- | --- |
| [`DubboClient.QueryAsync` / `QueryInternalAsync`](../../../Clients/DubboClient.cs) | Where do arguments enter? When are types inferred, protocol checked, and initial discovery triggered? → [Client](client.md) |
| [`CreateMetadataInvocationAsync`](../../../Clients/DubboClient.cs) / [`DubboMetadataManager.GetServiceMethodsAsync`](../../../Clients/DubboMetadataManager.cs) | Where do Java signatures come from, and what if they are unavailable? → [Metadata](metadata.md) |
| [`ConcurrentGetProviderEndPoints` / `GetSeviceProviderEndPointsAsync`](../../../Clients/DubboClient.cs) | Who looks up addresses and installs subscriptions? → [Registry and discovery](registry.md) |
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

[Registry and discovery](registry.md) covers address lookup and notifications; [Client](client.md) covers reconnect and resource release.

## State ownership

| State | Owner | Not the same as |
| --- | --- | --- |
| Callable Provider candidates, selection, and reservation | [`DubboServiceDriver`](../../../Clients/DubboServiceDriver.cs) and [`DubboDriverCollection`](../../../Clients/DubboDriverCollection.cs) | A cache of registry lookup results |
| Java method and type definitions | [`DubboMetadataManager`](../../../Clients/DubboMetadataManager.cs) | A list of callable Providers |
| Registry connection references and client actuator lifetime | [`DubboClient`](../../../Clients/DubboClient.cs); the underlying connections are shared by storage | Exclusive connection ownership or an individually removable ZooKeeper watch |

Check the [public API](../../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs), [metadata](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs), and [driver](../../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs) tests for each path; the [end-to-end environment guide](../../../Tests/EndToEndTestEnvironment/README.md) covers live Provider prerequisites.
