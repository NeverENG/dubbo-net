# 元数据：Java 方法定义如何变成调用签名

[调用链总览](index.md) · [English](../../en/architecture/metadata.md)

## 模块职责

元数据模块为 Java 接口获取方法名、参数类型和重载定义，并将调用方提供的参数匹配到具体签名。它保存**方法定义的缓存**，不保存可调用 Provider 候选，也不负责协议收发；Provider 的发现和选择分别交给[注册与发现](registry.md)和[服务选择](selection.md)。

## 定义获取与签名解析

`DubboMetadataManager` 按服务获取并缓存方法定义，根据 Provider 的版本和可用来源从元数据中心或短时 Telnet `ls -l` 查询，必要时用 Telnet 补全不完整的定义。定位 Provider 时可以复用注册发现，但元数据查询不会创建业务调用的 Driver。元数据中心既可与注册中心共用地址，也可单独配置。

当 `QueryAsync` 未指定 Java 参数类型，`DubboClient.CreateMetadataInvocationAsync` 获取定义，由 `DubboMethodMetadataResolver.Resolve` 匹配参数和重载，构造带 Java 类型的 `DubboInvocation` 后交给客户端发送。如果缺少可用定义或签名存在歧义，可能在发送之前抛出 `DubboMetadataException`。调用方已明确指定类型时，客户端可以直接构造调用；同名重载的协议约束仍可能被尽力检查，而不是要求每次调用都先成功读取元数据。

## 缓存与订阅生命周期

Manager 按服务缓存定义；Provider 或元数据变化只刷新**已有缓存项**，不会把定义缓存变成 Driver 的地址缓存。Nacos Config 的元数据订阅由 Manager 持有并在释放时移除，与 Provider 发现所用的 Nacos Naming 订阅分开。一次调用的签名异常属于发送前的错误，不等于传输后返回的 `DubboRequestResult` 失败。

## 源码入口

| 文件 | 职责 |
| --- | --- |
| `DubboMetadataManager.cs` | 方法定义的获取、来源选择、按服务缓存、刷新和 Config 订阅。 |
| `DubboMethodMetadataResolver.cs` | 用实参匹配 Java 方法签名和重载。 |
| `DubboTelnetMetadataResolver.cs` | 解析并补全 Telnet 返回的方法定义。 |
| `DubboClient.cs` | 需要推断类型时构造调用，以及显式调用的协议检查。 |
