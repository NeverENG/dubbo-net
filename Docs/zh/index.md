# 从哪里读 DubboNet

[English](../en/index.md) · [使用手册](../../readme_cn.md)

DubboNet 目前主要负责从 .NET 应用**调用远端 Java Provider**。如果你想知道一次 `QueryAsync` 是怎么发出去的，先读[客户端入口](client/index.md)，再沿着它的下游看：

1. [元数据与签名](metadata/index.md)：不知道 Java 参数类型时，怎样准备调用描述。
2. [注册与发现](registry/index.md)：怎样找到 Provider，并在地址变化时更新。
3. [服务选择](selection/index.md)：有多个 Provider 时怎样选一个，如何保护在途调用。
4. [协议传输](transport/index.md)：怎样通过 Dubbo TCP 或 Triple 发送并读回结果。

这是一条**阅读路线**，不是每次请求都走完的固定流程：元数据查询本身可能需要发现 Provider；已有服务的调用也不必每次重新发现。注册中心变化则从另一条回调路径更新状态。

想直接运行代码，请看[使用手册](../../readme_cn.md)；想追溯设计和变更，请看 [RFC 约定](../RFC.md)与[变更索引](../../CHANGELOG.md)。[端到端测试指南](../../Tests/EndToEndTestEnvironment/README.md)介绍测试用的 Java Provider，并不是 .NET 服务端实现。
