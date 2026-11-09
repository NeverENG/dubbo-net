# 元数据：从定义到调用签名

[调用链总览](index.md) · [English](../../en/architecture/metadata.md)

## 何时进入、交给谁

| 源码入口 | 要追踪的交接 |
| --- | --- |
| [`DubboClient.CreateMetadataInvocationAsync`](../../../Clients/DubboClient.cs) | `QueryAsync` 没有得到 Java 参数类型时，请求服务定义并解析方法签名，然后构造 `DubboInvocation`；歧义或缺失可能在发送前抛出 `DubboMetadataException`。 |
| [`DubboMetadataManager.GetServiceMethodsAsync`](../../../Clients/DubboMetadataManager.cs) | 为服务取得并缓存方法定义。可能借用[注册发现](registry.md)定位 Provider，再从元数据中心或 Telnet `ls -l` 获取定义；这一步不创建业务调用的 Driver。 |
| [`DubboMethodMetadataResolver.Resolve`](../../../Clients/DubboMethodMetadataResolver.cs) / [`DubboTelnetMetadataResolver`](../../../Clients/DubboTelnetMetadataResolver.cs) | 分别定位实参与 Java 签名、决定来源顺序及 Telnet 结果的补全方式。 |
| [`DubboClient.GetRequiredProtocolAsync`](../../../Clients/DubboClient.cs) | 即使签名由调用方明确指定，仍可能尽力检查同名重载是否要求原生协议；检查失败不让显式调用强制依赖元数据。 |
| [`RefreshCachedServiceAsync`](../../../Clients/DubboMetadataManager.cs) | 注册事件只针对已缓存定义触发刷新；结果回到[客户端](client.md)，不替代[服务选择](selection.md)的 Provider 状态。 |

方法与类型定义由 Manager 管理；它们不是执行器、业务 Provider 快照或注册中心连接本身。元数据中心可与注册中心共用地址，也可分开配置；Nacos Config 订阅由元数据 Manager 在关闭时移除，与 Naming Provider 订阅分开。错误何时抛出、何时转为调用失败结果，沿入口核对，不用文档假设每个异常等价。

**核对：** [元数据测试](../../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)、[调用契约测试](../../../Tests/UnitTestForDubboNet/DubboMetadataContractTest.cs)。公共查询方式与配置在[使用说明](../../../readme_cn.md#元数据策略与缓存--metadata-strategy-and-cache)。
