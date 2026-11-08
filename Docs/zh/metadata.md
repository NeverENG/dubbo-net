# 元数据与方法签名解析

[代码地图](index.md) · [English](../en/metadata.md)

[`DubboMetadataManager`](../../Clients/DubboMetadataManager.cs) 获取并缓存服务定义；[`DubboMethodMetadataResolver`](../../Clients/DubboMethodMetadataResolver.cs) 为一次调用选择 Java 方法签名。这些文件目前位于 `Clients/`；地图并不把当前目录位置当作将来应遵循的所有权边界。

**交接点：** Manager 会回调[Provider 发现](registry.md)，以定位元数据或探测 Provider Telnet。[`DubboClient.CreateMetadataInvocationAsync`](../../Clients/DubboClient.cs) 在需要推断 Java 参数类型时使用结果。另一个 `GetRequiredProtocolAsync` 会查元数据以判断重载兼容性，即使调用方显式传入类型；两种查询的失败语义不同。注册变化后也可能刷新已缓存的元数据。

**源码与验证：** [Manager 与解析](../../Clients/DubboMetadataManager.cs) · [Telnet resolver](../../Clients/DubboTelnetMetadataResolver.cs) · [元数据测试](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs) · [契约测试](../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs) · [用法与回退策略](../../readme_cn.md)。[原生协议说明](../NativeDubboProtocol.md)提供协议背景，不作为第二份元数据规范。
