# 代码地图

[English](../en/index.md) · [中文 README](../../readme_cn.md) · [English README](../../README.md)

先选择模块，再从模块介绍进入源码、测试和现有专题文档。此页只做导航，不另写实现规范或使用手册；模块页介绍的是**当前消费端代码**，不是将来的目录或命名空间调整方案。

| 模块 | 为什么看 | 继续阅读 |
| --- | --- | --- |
| 客户端入口 | 公开调用、调用准备与生命周期 | [客户端](client/index.md) |
| 注册中心与发现 | 寻找 Provider，接收服务变化 | [注册与发现](registry/index.md) |
| 元数据 | 获取方法／类型定义、解析方法签名 | [元数据](metadata/index.md) |
| 服务 Driver 与选择 | 维护可调用候选，选择并预留一次调用 | [选择与预留](selection/index.md) |
| 协议传输 | 通过 Dubbo TCP 或 Triple HTTP/JSON 发送请求 | [协议传输](transport/index.md) |

**一次调用的阅读顺序：** [客户端](client/index.md) → [元数据](metadata/index.md)（需要时）→ [注册与发现](registry/index.md) → [选择与预留](selection/index.md) → [协议传输](transport/index.md)。元数据定位本身也需要发现 Provider；注册变化事件则另外进入客户端的刷新路径。这个顺序帮助阅读，不表示每次调用都会执行全部步骤。

用法与兼容性参见[中文 README](../../readme_cn.md)；架构决策看 [RFC 约定](../RFC.md)，Git 历史看[变更索引](../../CHANGELOG.md)。[端到端测试指南](../../Tests/EndToEndTestEnvironment/README.md)解释如何运行 Java Provider 测试环境；它不是 .NET Provider 服务端。
