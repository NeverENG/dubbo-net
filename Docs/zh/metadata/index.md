# 元数据：没有 Java JAR，怎样确定方法签名

[代码地图](../index.md) · [English](../../en/metadata/index.md)

例如传入一个 C# 整数调用 `getJob`，客户端仍需要知道对应的 **Java 参数类型**，才能准备 `DubboInvocation`。元数据提供服务的方法和类型定义；它不保存可调用的 Provider 连接，也不发送业务请求。当前 `DubboMetadataManager` 位于 `Clients/`，这只是现状，不是对未来层级的约定。

## 从哪里获取、交给谁

客户端在需要推断类型时请求方法签名；Manager 借助[服务发现](../registry/index.md)得到 Provider 信息，从元数据中心读取定义，或按当前策略尝试 Provider Telnet。签名解析器再用方法名和实参选出 Java 参数类型，交还客户端组成调用描述。已缓存定义可能随注册或元数据变化刷新。调用中还可能再次查询元数据，以决定重载方法是否需要限定 `dubbo://` 协议；显式传入类型可跳过**推断**，不等于完全跳过元数据检查。

如果只想查看某个方法有哪些签名，使用公开 API，而不必直接创建 Manager：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var methods = await client.GetMethodMetadataAsync(
    "com.foo.JobService#getJob");
```

`GetServiceMethodsMetadataAsync` 列出服务方法；`GetServiceTypesMetadataAsync` 读取类型定义（Telnet 不能提供 types）。自动推断所需签名缺失或有歧义会阻断调用，此时可按[元数据使用说明](../../../readme_cn.md#元数据策略与缓存--metadata-strategy-and-cache)提供精确 Java 类型；显式类型调用的兼容性检查是尽力而为的另一条路径。

**从代码继续读：** [Manager](../../../Clients/DubboMetadataManager.cs)、[签名解析](../../../Clients/DubboMethodMetadataResolver.cs)、[Telnet 回退](../../../Clients/DubboTelnetMetadataResolver.cs)；对照[元数据测试](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)和[契约测试](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs)。
