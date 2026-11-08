# 协议传输

[代码地图](index.md) · [English](../en/transport.md)

[服务 Driver](selection.md) 按发现的 Provider URL scheme 选择执行器，再由执行器向**远端** Provider 发送调用。`DubboService/` 是现有目录名，并非 .NET Provider 服务端入口。支持范围以[中文 README 的兼容性说明](../../readme_cn.md)为准，不能从目录名推断。

| 传输 | 源码 → 测试 → 专题文档 |
| --- | --- |
| 原生 Dubbo TCP / Hessian2 | [NativeDubboActuatorSuite](../../DubboService/NativeDubboActuatorSuite.cs) → [NativeDubboConnection](../../DubboService/Native/NativeDubboConnection.cs)／[NativeDubboCodec](../../DubboService/Native/NativeDubboCodec.cs) → [编解码测试](../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs) → [原生协议说明](../NativeDubboProtocol.md) |
| Triple HTTP/JSON | [TripleDubboActuatorSuite](../../DubboService/TripleDubboActuatorSuite.cs) → [传输测试](../../Tests/UnitTestForDubboNet/TripleDubboActuatorSuiteTest.cs) → [Triple 协议说明](../TripleProtocol.md) |

[端到端环境指南](../../Tests/EndToEndTestEnvironment/README.md)使用真实 Java Provider 测试这些调用。协议字节格式仍留在现有专题文档和实现中，此页不另写一份。
