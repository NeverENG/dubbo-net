# Protocol transports

[Code map](index.md) · [中文](../zh/transport.md)

The [service driver](selection.md) selects an actuator from the discovered Provider URL scheme; an actuator then sends the invocation to a **remote** Provider. `DubboService/` is an existing directory name, not a .NET Provider-server entry point. See the [README compatibility section](../../README.md) for supported capabilities rather than inferring them from the directory name.

| Transport | Read source → tests → detailed notes |
| --- | --- |
| Native Dubbo TCP / Hessian2 | [NativeDubboActuatorSuite](../../DubboService/NativeDubboActuatorSuite.cs) → [NativeDubboConnection](../../DubboService/Native/NativeDubboConnection.cs) / [NativeDubboCodec](../../DubboService/Native/NativeDubboCodec.cs) → [codec tests](../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs) → [Native protocol notes (Chinese)](../NativeDubboProtocol.md) |
| Triple HTTP/JSON | [TripleDubboActuatorSuite](../../DubboService/TripleDubboActuatorSuite.cs) → [transport tests](../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs) → [Triple protocol notes (Chinese)](../TripleProtocol.md) |

The [end-to-end guide](../../Tests/EndToEndTestEnvironment/README.md) runs real Java Provider fixtures to verify these calls. Keep detailed wire formats in the existing protocol notes and implementation, not in this navigation page.
