# Client entry

[Code map](index.md) · [中文](../zh/client.md)

[`DubboClient`](../../Clients/DubboClient.cs) owns the public `QueryAsync` entry points, assembles registry and metadata connections, and coordinates discovery, selection, dispatch, refresh and shutdown. Read its `QueryWithInferredTypesAsync` / `QueryAsync(DubboInvocation)` before `QueryInternalAsync`; the former prepares an invocation, while the latter selects a Provider, discovers one when needed, and dispatches. This page describes existing responsibilities, not an endorsement of their present placement.

**Handoffs:** signature inference calls [Metadata](metadata.md); Provider lookup calls [Registry](registry.md); a selected service driver hands off to [Selection](selection.md) and then [Transports](transport.md). The [ZooKeeper watcher](../../Clients/DubboClientZookeeperWatcher.cs) and Nacos callback enter a separate refresh path. `DubboClient.Dispose` coordinates releases; follow the linked owners before changing the order.

**Read and verify:** [DubboClient source](../../Clients/DubboClient.cs) · [public API tests](../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs) · [usage and configuration](../../README.md) · [end-to-end guide](../../Tests/EndToEndTestEnvironment/README.md). The Java Providers in the guide are test fixtures, not an implementation of a .NET server.
