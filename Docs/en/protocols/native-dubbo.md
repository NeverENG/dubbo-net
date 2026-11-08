# Dubbo2 Native TCP Protocol Implementation

[中文](../../zh/protocols/native-dubbo.md) · [English home](../index.md)

`NativeDubboActuatorSuite` uses Dubbo2 TCP frames, Hessian2, and generic `$invoke` calls. It does not rely on the Provider's Telnet `invoke` command.

## Version fields

Several fields in a ZooKeeper Provider URL have different meanings:

- `release=2.7.3`: the Provider's Dubbo framework release, not the wire protocol selector.
- `dubbo=2.0.2`: the classic Dubbo protocol version; the native request body also begins with `2.0.2`.
- `version=...`: the service version, included in the request body's service version and attachments.
- `group=...`: the service group, included in the attachments.
- URL scheme `dubbo://`: selects the native Dubbo2 TCP actuator.
- URL scheme `tri://`: selects the separate `TripleDubboActuatorSuite`; see the [first-phase Triple support](triple.md).

The Provider URL scheme, **not** the value of `release`, selects the actuator.

## 16-byte frame header

| Offset | Length | Meaning |
| --- | ---: | --- |
| 0 | 2 | Magic, fixed at `0xda 0xbb` |
| 2 | 1 | Flag: bit 7=request, bit 6=two-way, bit 5=event, low 5 bits=serialization id |
| 3 | 1 | Response status; `0` in a request frame, `20` for a successful response |
| 4 | 8 | Request ID, big-endian `int64` |
| 12 | 4 | Body length, big-endian `int32` |

The flag of a normal two-way Hessian2 request is `0xc2`. A normal Hessian2 response has `2` in the low five bits.

## Generic invocation request body

The body contains consecutive Hessian2 values, not one JSON object. The implementation writes, in order:

1. Dubbo protocol version: `"2.0.2"`.
2. Service path: the full interface name.
3. Service version: `null` when none is set.
4. Method: `"$invoke"`.
5. Parameter descriptor: `"Ljava/lang/String;[Ljava/lang/String;[Ljava/lang/Object;"`.
6. Actual method name.
7. Array of Java parameter type names, such as `["java.lang.String", "java.lang.Long"]`.
8. Array of argument values.
9. Attachments Map.

The default attachments contain `path`, `interface`, `dubbo=2.0.2`, `generic=true`, and `timeout`, plus optional `version`, `group`, and caller-defined entries.

Complex POJO arguments are sent as generic Maps with `class=<Java FQCN>`. For example, for a parameter of type `com.foo.User`, the anonymous object `{ name = "demo" }` becomes:

```json
{
  "class": "com.foo.User",
  "name": "demo"
}
```

## Response body

When the status is not `20`, the body is a Hessian2 string, usually containing Dubbo/RpcException details.

For status `20`, the first Hessian2 int identifies the result type:

| Value | Structure |
| ---: | --- |
| 0 | Exception |
| 1 | Value |
| 2 | Null value |
| 3 | Exception + attachments |
| 4 | Value + attachments |
| 5 | Null value + attachments |

Decoded values are recursively converted to .NET scalars, `List<object>`, and `Dictionary<string, object>`, preserving POJO `class` fields, nested collections, binary values, and response attachments. Cyclic references become `"[Circular reference]"` to avoid infinite recursion during JSON serialization.

`DubboRequestResult` exposes `Result` (JSON text), `RawResult` (the dynamic .NET value before JSON conversion), `RequestId`, `ResponseStatus`, `SerializationId`, `ResponseAttachments`, and `RequestElapsed`.

Classic Dubbo responses do not normally contain the Provider method's execution time, so `ServiceElapsedAvailable` is `false` for native results.

## Current boundaries

- Only Hessian2 (serialization id `2`) is implemented.
- Calls use generic `$invoke`; the .NET application does not need the Java interface JAR.
- `DubboClient.QueryAsync(endpoint, arguments)` reads and caches the exact signature. For a Provider with `release >= 2.7.3`, it tries the metadata center before falling back to Telnet `ls -l`; for a known older release, it starts with Telnet; for an unrecognized release, it first tries the metadata center.
- The metadata center defaults to the registry connection; `MetadataCenterAddress` and `MetadataRootPath` can configure it separately.
- Telnet metadata and the negative "Telnet unsupported" result are cached per service and probed only once per `DubboClient` lifetime. If a metadata node is later created, a ZooKeeper watch can switch back to the higher-priority metadata-center result.
- Transient registry or metadata-center connection failures are not permanently cached. Later calls retry the higher-priority source while reusing existing positive or negative Telnet cache entries without reconnecting. If the metadata center returns only some methods or overloads, a failed method resolution can supplement the result once from Telnet.
- Before writing Telnet extension output into `$invoke` `parameterTypes`, source-style generic Java type names are erased to loadable raw class names.
- If neither Provider source supplies metadata, a zero-argument method can still be called directly. A method with arguments raises `DubboMetadataException` containing both failure reasons and advises using the explicit-signature overload rather than guessing `int` vs. `Integer` or `long` vs. `Long`.
- One TCP connection multiplexes concurrent requests by Request ID; writes are serialized, while responses may arrive out of order.
- The default body limit is 8 MiB.
- The client responds to two-way event/heartbeat frames from the Provider.
- A failed connection write does not automatically resend a business request, to avoid duplicate calls after an uncertain outcome; subsequent requests reconnect.

Telnet does not carry native business calls. It is used here only for a one-shot `ls -l` signature fallback when required, and continues to support explicit diagnostics such as `ls`, `status`, and `trace`.
