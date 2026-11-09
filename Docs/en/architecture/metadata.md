# Metadata: From Definitions to Invocation Signatures

[Call-path overview](index.md) · [中文](../../zh/architecture/metadata.md)

## When it runs and what it hands off

| Source entry | Handoff to follow |
| --- | --- |
| [`DubboInvocationMetadataResolver.CreateAsync`](../../../Metadata/DubboInvocationMetadataResolver.cs) | When `QueryAsync` lacks Java parameter types, it requests service definitions, resolves a method signature, and constructs a `DubboInvocation`. Missing or ambiguous types may raise `DubboMetadataException` before dispatch. |
| [`DubboMetadataManager.GetServiceMethodsAsync`](../../../Metadata/DubboMetadataManager.cs) | Retrieves and caches method definitions. It may use [registry discovery](registry.md) to locate Providers before trying the metadata center or Telnet `ls -l`; this does not create a business-call driver. |
| [`DubboMethodMetadataResolver.Resolve`](../../../Metadata/DubboMethodMetadataResolver.cs) / [`DubboTelnetMetadataResolver`](../../../Metadata/DubboTelnetMetadataResolver.cs) | Resolve arguments against Java signatures and supplement partial metadata results from Telnet. |
| [`DubboClient.GetRequiredProtocolAsync`](../../../Clients/DubboClient.cs) | Even with caller-supplied types, a best-effort check can find same-name overloads requiring native Dubbo. Failure of the check does not make metadata mandatory for explicit calls. |
| [`RefreshCachedServiceAsync`](../../../Metadata/DubboMetadataManager.cs) | Registry events can refresh existing cached definitions; the result does not replace [selection](selection.md)'s callable Provider state. |

The manager owns Java method and type definitions, not actuators, business Provider snapshots, or registry connections. The metadata center may share the registry address or have its own. The manager removes its Nacos Config subscription on disposal, separately from the Naming subscription for Providers. Follow the entry path to distinguish pre-dispatch exceptions from returned failure results rather than treating them as interchangeable.

**Verify:** [metadata tests](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs), [invocation contract tests](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs). Public lookup methods and configuration are in the [user guide](../../../README.md#metadata-strategy-and-cache).
