# Code map

[中文](../zh/index.md) · [English README](../../README.md) · [Chinese README](../../readme_cn.md)

Choose a module below, then follow its source, tests, and existing detailed guides. This index is navigation, not another implementation or user manual. The pages describe the **current consumer-side code**, not a proposed reorganization of namespaces or directories.

| Module | Why read it | Continue |
| --- | --- | --- |
| Client entry | Public calls, invocation preparation, and lifecycle | [Client](client/index.md) |
| Registry and discovery | Find Providers and receive changes | [Registry](registry/index.md) |
| Metadata | Find method/type definitions and resolve signatures | [Metadata](metadata/index.md) |
| Service driver and selection | Maintain callable candidates and reserve a dispatch | [Selection](selection/index.md) |
| Protocol transports | Send the selected call over Dubbo TCP or Triple HTTP/JSON | [Transports](transport/index.md) |

**Reading order for a call:** [Client](client/index.md) → [Metadata](metadata/index.md) (when needed) → [Registry](registry/index.md) → [Selection](selection/index.md) → [Transports](transport/index.md). Metadata also uses discovery to locate Provider metadata; registry-change callbacks follow a separate route into client refresh. These links are reading aids, not a claim that every call performs every step.

For usage and compatibility consult the [README](../../README.md); for architecture decisions see [RFC conventions](../RFC.md), and for verified Git history see the [changelog](../../CHANGELOG.md). The [end-to-end test guide](../../Tests/EndToEndTestEnvironment/README.md) explains how to exercise the Java Provider fixtures; they are not a .NET Provider server.
