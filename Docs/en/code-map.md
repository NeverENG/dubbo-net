# Reading DubboNet Along a Call

[English home](index.md) · [中文](../zh/code-map.md) · [User guide](../../README.md)

DubboNet is a .NET **consumer of remote Java Providers**, not a .NET Provider server. Follow a `QueryAsync` call using the route below. This is not a fixed sequence for every request: explicit Java types can skip signature inference, and a cached service driver need not rediscover Providers. Registry change callbacks follow a separate path.

## 1. Entry point and invocation

An application passes a service, method, and arguments to `DubboClient.QueryAsync` and receives a `DubboRequestResult`. `DubboInvocation` carries Java types, version, group, and attachments. The entry point coordinates work rather than encoding the wire protocol itself. With known Java types, the caller can specify an exact signature:

```csharp
using DubboNet.Clients;

using var client = new DubboClient("127.0.0.1:2181");
var result = await client.QueryAsync(
    "com.foo.JobService#getJob",
    new[] { "java.lang.Integer" },
    new object[] { 1234 });

if (!result.QuerySuccess) Console.WriteLine(result.ErrorMeaasge);
```

The address and method are illustrative; a reachable Provider is required. See the [user guide](../../README.md#quick-start) for other overloads and error handling. **Read:** [client entry point](../../Clients/DubboClient.cs), [result model](../../DubboService/DataModle/DubboRequestResult.cs), and [public API tests](../../Tests/UnitTestForDubboNet/DubboClientApiTest.cs).

## 2. Metadata: determine Java types when needed

Without explicit types, the client obtains service definitions through the metadata manager; the signature resolver then selects Java parameter types. An unavailable or ambiguous signature can raise `DubboMetadataException` before dispatch. Supplying types skips inference, although the client may still make a best-effort metadata check for protocol restrictions on overloaded methods; failure of that check does not make metadata mandatory for an explicit call.

Depending on the Provider, the manager reads the metadata center and may fall back to Telnet `ls -l`. The metadata center can share the registry connection or be configured separately. **Finding Providers for definitions** does not create the business-call service driver. The public `GetMethodMetadataAsync` API inspects signatures; see the [metadata guide](../../README.md#metadata-strategy-and-cache). **Read:** [manager](../../Clients/DubboMetadataManager.cs), [resolver](../../Clients/DubboMethodMetadataResolver.cs), and [tests](../../Tests/UnitTestForDubboNet/DubboMetadataTest.cs).

## 3. Registry and discovery: find Providers

When the first call has no service driver, the client looks up Provider addresses by service name in ZooKeeper or Nacos. Metadata lookup may also use discovery to locate definitions. The registry answers “which addresses exist”; it does **not own the callable Provider state**. When Providers change, a ZooKeeper watcher or Nacos callback asks the client to refresh existing state. This update path runs independently of `QueryAsync`; not every request looks up the registry again.

Applications create the client rather than querying Providers themselves. See [configuration](../../README.md#configuration) for registry groups, a separate metadata center, and optional consumer registration. **Read:** [discovery entry](../../Clients/DubboClient.cs), [ZooKeeper](../../Clients/RegistryClient/MyZookeeper.cs), [Nacos](../../Clients/RegistryClient/MyNacos.cs), and [registry tests](../../Tests/UnitTestForDubboNet/MyZookeeperTest.cs).

## 4. Service selection and reservation

The driver owns each service's callable candidates. It filters by version, group, and any required protocol restriction; a configured load-balancing strategy then **selects and reserves** an invocation. If the driver does not yet exist, the entry point first discovers Providers and builds it before retrying selection. The reservation protects an in-flight call even if the Provider disappears during dispatch; it is released when the call finishes. Different services may still share an actuator. Applications configure `NowLoadBalanceMode` rather than manipulating drivers directly.

**Read:** [service driver](../../Clients/DubboServiceDriver.cs), [reservation](../../Clients/LoadBalancing/LoadBalancedInvocation.cs), and [driver tests](../../Tests/UnitTestForDubboNet/DubboServiceDriverTest.cs).

## 5. Transport and result

Registered `dubbo://` Providers use native Dubbo TCP/Hessian2; `tri://` Providers use Triple Unary HTTP/JSON. The selected actuator sends the request and reads the response; applications still receive results through the client. [Direct actuator usage](../../README.md#direct-actuator-usage) requires a known Provider address and exact Java signature and bypasses discovery, inference, and load balancing. Other scheme-based Telnet fallback retained inside the driver is not a normal registered Provider candidate, nor is it the Telnet **metadata probe**.

For wire formats and limits, see the [native protocol](protocols/native-dubbo.md) and [Triple protocol](protocols/triple.md). **Read:** [native actuator](../../DubboService/NativeDubboActuatorSuite.cs), [Triple actuator](../../DubboService/TripleDubboActuatorSuite.cs), and [codec tests](../../Tests/UnitTestForDubboNet/NativeDubboCodecTest.cs).

## A separate path: change notifications and shutdown

Registry events can trigger refreshes of Providers or cached metadata; the client also releases registry connection references, service drivers, and actuators. Unlike the business-call path, event handling involves reconnecting and reinstalling one-shot watches. Check the [client refresh entry](../../Clients/DubboClient.cs) and [ZooKeeper watcher](../../Clients/DubboClientZookeeperWatcher.cs) rather than inferring callback behavior from requests. To run cross-runtime tests, use the [end-to-end environment guide](../../Tests/EndToEndTestEnvironment/README.md).
