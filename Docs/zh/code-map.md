# 从一次调用读 DubboNet 代码

[中文首页](index.md) · [English](../en/code-map.md) · [使用手册](../../readme_cn.md)

DubboNet 是**调用远端 Java Provider 的 .NET 消费端**，不是 .NET 服务端。下面按一次 `QueryAsync` 的阅读顺序找代码；它不是每次请求都执行的固定流水线：显式传入 Java 类型可跳过签名推断，已有服务 Driver 时也不必重新发现 Provider。注册中心的变更回调则是另一条路径。

## 1. 入口与调用描述

应用通过 `DubboClient.QueryAsync` 传入服务、方法及参数，得到 `DubboRequestResult`；`DubboInvocation` 保存 Java 类型、版本、分组和 attachments。入口负责协调，不直接实现协议编解码。已知精确 Java 类型时可显式传入：

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

这只是示意地址与方法，需要可访问的 Provider。其他重载和错误处理见[使用手册](../../readme_cn.md#快速开始--quick-start)。**看代码：** [客户端入口](../../Clients/DubboClient.cs)、[结果模型](../../DubboService/DataModle/DubboRequestResult.cs)和[公开 API 测试](../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs)。

## 2. 元数据：按需确定 Java 签名

没有指定 Java 类型时，客户端通过元数据 Manager 取得服务定义，再由签名解析器确定参数类型；元数据无法提供明确签名时，可能在发送前抛出 `DubboMetadataException`。显式给出类型可跳过推断，但客户端仍可能尽力检查重载方法的协议限制，检查失败不会成为显式调用的必需依赖。

Manager 会按 Provider 情况读取元数据中心，必要时尝试 Telnet `ls -l`；元数据中心可与注册中心共用连接或单独配置。**找定义所用的 Provider 查询**不等于建立业务调用的 Driver。查看签名可用公开的 `GetMethodMetadataAsync`；详见[元数据使用说明](../../readme_cn.md#元数据策略与缓存--metadata-strategy-and-cache)。**看代码：** [Manager](../../Clients/DubboMetadataManager.cs)、[解析器](../../Clients/DubboMethodMetadataResolver.cs)、[测试](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs)。

## 3. 注册与发现：找到 Provider

首次调用没有服务 Driver 时，客户端按服务名从 ZooKeeper 或 Nacos 获取 Provider 地址；元数据查询也可能借用发现结果定位定义。注册中心负责回答“有哪些地址”，**不持有可调用的 Provider 状态**。Provider 上下线时，ZooKeeper watcher 或 Nacos 回调通知客户端刷新已有状态；这条更新路径与 `QueryAsync` 并行，不保证每次请求都重新查询注册中心。

应用只需创建客户端，不用手动查询 Provider。注册分组、独立的元数据中心和可选消费者注册见[配置说明](../../readme_cn.md#客户端配置--configuration)。**看代码：** [发现入口](../../Clients/DubboClient.cs)、[ZooKeeper](../../Clients/RegistryClient/MyZookeeper.cs)、[Nacos](../../Clients/RegistryClient/MyNacos.cs)及其[注册中心测试](../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs)。

## 4. 服务选择与预留

Driver 持有每个服务的可调用候选；它按版本、分组和必要的协议限制筛选，再按配置的策略**选择并预留**一次调用。如果还没有 Driver，入口先发现并建立它，再尝试选择。预留保护在途调用，即使 Provider 在发送期间下线，也不能提前释放它使用的执行器；调用结束后归还。不同服务仍可能共享执行器。应用通过 `NowLoadBalanceMode` 配置策略，不必直接操作 Driver。

**看代码：** [服务 Driver](../../Clients/DubboServiceDriver.cs)、[预留](../../Clients/LoadBalancing/LoadBalancedInvocation.cs)、[Driver 测试](../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs)。

## 5. 协议传输与结果

注册发现接受的 `dubbo://` Provider 使用原生 Dubbo TCP/Hessian2 执行器，`tri://` 使用 Triple Unary HTTP/JSON；执行器发送请求并读取响应，应用仍通过客户端取得结果。已知 Provider 地址和精确 Java 签名时可以[直接使用执行器](../../readme_cn.md#直接使用执行器--direct-actuator-usage)，但这会绕过发现、元数据推断和负载均衡。Driver 内保留的其他 scheme Telnet 回退不属于正常注册发现，也不同于 Telnet **元数据探测**。

具体线格式与限制见[原生协议](protocols/native-dubbo.md)和[Triple 协议](protocols/triple.md)。**看代码：** [原生执行器](../../DubboService/NativeDubboActuatorSuite.cs)、[Triple 执行器](../../DubboService/TripleDubboActuatorSuite.cs)、[编解码测试](../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs)。

## 另一条路径：变化通知与关闭

注册中心事件会尝试刷新 Provider 或已缓存的元数据；客户端负责释放注册连接引用、服务 Driver 和执行器。与业务请求不同，事件处理还涉及重连与一次性 watch 的重新安装；具体行为应核对[客户端刷新入口](../../Clients/DubboClient.cs)和[ZooKeeper watcher](../../Clients/DubboClientZookeeperWatcher.cs)，不能只从请求路径推断。想运行跨端测试，参见[端到端环境指南](../../Tests/EndToEndTestEnvironment/README.md)。
