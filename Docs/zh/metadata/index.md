# 元数据与方法签名

[代码地图](../index.md) · [English](../../en/metadata/index.md)

## 功能与背景

.NET 调用方无需持有 Java 接口 JAR，但自动推断参数仍需要 Provider 的 Java 方法签名。[`DubboMetadataManager`](../../../Clients/DubboMetadataManager.cs)获取并缓存服务定义，[`DubboMethodMetadataResolver`](../../../Clients/DubboMethodMetadataResolver.cs)根据参数选择候选签名。Manager 目前在 `Clients/` 目录下；这个文件位置并不代表我们已确定将来的所属层。

## 上下游如何衔接

[客户端](../client/index.md)在需要推断 Java 类型时获取签名；之后还可能再查元数据，判断重载方法是否只能选择原生 `dubbo://` Provider。Manager 先通过[注册发现回调](../registry/index.md)获取 Provider 信息，再读取元数据中心，或按当前策略通过 [Telnet resolver](../../../Clients/DubboTelnetMetadataResolver.cs)探测 Provider。刷新事件可能使已缓存的定义失效。推断所需签名缺失会阻断调用；显式类型调用中的兼容性检查则尽力而为，具体异常路径以源码为准。

## 输入、产物与连接

- **输入：** 服务／方法标识；签名解析还需要参数值。Provider 属性用于定位元数据或 Telnet 端点。
- **产物：** 方法／类型元数据和选中的 Java 参数签名，用于构造 `DubboInvocation`。方法元数据**不是**可调用的 Provider 连接列表，后者由[服务 Driver](../selection/index.md)管理。
- **来源：** 元数据中心内容与可能的 Telnet 回退。元数据中心可与注册中心共用连接，也可使用独立地址；两者都不是业务 RPC 发送端。

## 如何使用

应用通过客户端公开 API 查询，不需要自行创建内部 Manager：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var methods = await client.GetMethodMetadataAsync(
    "com.foo.JobService#getJob");
```

`GetServiceMethodsMetadataAsync` 获取服务方法列表；`GetServiceTypesMetadataAsync` 从元数据中心读取类型定义（Telnet 无法提供 types）。无需预先显式查询也可使用[客户端自动推断重载](../client/index.md)；签名缺失或有歧义时按[元数据使用说明](../../../readme_cn.md#元数据策略与缓存--metadata-strategy-and-cache)传入精确 Java 类型。对照 [Manager 测试](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)、[契约测试](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs)以及提供协议背景的[原生协议说明](../../NativeDubboProtocol.md)。
