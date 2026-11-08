# First-phase Triple Implementation

[中文](../../zh/protocols/triple.md) · [English home](../index.md)

`TripleDubboActuatorSuite` handles Dubbo 3.3+ Java Interface Unary HTTP/JSON calls. The service driver selects it for a registered `tri://` Provider URL.

## Request format

- HTTP method: `POST`.
- Path: `/{full Java interface name}/{method name}`.
- Content-Type: `application/json`.
- Body: a JSON array in method-parameter order. No arguments use `[]`; even a single argument remains an array, such as `[false]`.
- The first implementation uses HTTP/1.1. Although a Dubbo 3.3 Triple port can also accept HTTP/2 and HTTP/3, these are outside the current implementation scope.

Example:

```http
POST /com.example.DataTypeService/negate HTTP/1.1
Content-Type: application/json
tri-service-version: 1.0.0
tri-service-group: demo
tri-service-timeout: 10000

[false]
```

Dubbo 3.3.6's default Triple HTTP/JSON mapping cannot reliably distinguish every Java overload: when an overload has no arguments, `~signature` may still match both methods; `[` in a primitive-array signature also interacts with URL encoding. When `DubboClient` finds multiple signatures for one method, it restricts selection to native `dubbo://` Providers **before load balancing**. If only `tri://` Providers are registered, it returns an explicit compatibility error instead of silently calling the wrong overload. Non-overloaded methods can continue over Triple HTTP/JSON.

## Service routing and attachments

The actuator prefers `DubboInvocation.Version` and `DubboInvocation.Group`. When not explicitly set, it uses `version`, `group`, and `timeout` from the registered Provider URL. The corresponding Triple headers are `tri-service-version`, `tri-service-group`, and `tri-service-timeout`.

`DubboInvocation.Attachments` become HTTP headers. Attachments cannot override protocol-reserved headers, pseudo-headers, `Host`, `Content-Length`, or `Content-Type`.

For a 2xx response, the raw JSON body is preserved. For a non-2xx response, `QuerySuccess=false` and `ErrorMeaasge` contains both the HTTP status and Provider error body. Response headers are kept in `ResponseAttachments`.

## Deadline, cancellation, and addresses

- The Provider URL's `timeout` takes precedence over the default `DubboRequestTimeout` and can be longer or shorter; the minimum is 1 ms.
- The per-call deadline covers both response headers and the complete body. The internal `HttpClient.Timeout` is infinite so it does not compete with the per-call deadline.
- `SendQuery(invocation, cancellationToken)` keeps the Native/Triple failure-result contract: caller cancellation returns `QuerySuccess=false` rather than throwing. Triple errors distinguish `canceled by caller` from `timed out after N ms`; if both happen, caller cancellation takes precedence. The caller should inspect the result/token to decide whether to stop a batch of calls.
- Host/port constructors accept DNS names, IPv4, and bracketed or unbracketed IPv6. The `IPEndPoint` constructor also supports IPv6.

## First-phase boundaries

Supported:

- Dubbo 3.3+ Java Interface.
- Unary calls.
- HTTP/1.1 and `application/json`.
- Classic ZooKeeper interface-level discovery of `tri://` Providers.

Not supported:

- Protobuf/IDL services.
- Client/server/bidirectional streaming.
- Binary gRPC/Triple framing.
- Client-side negotiation of TLS, HTTP/2, or HTTP/3.
- Dubbo 3 application-level service discovery.

## Design references

- [Apache Dubbo: Java Interface + Triple](https://dubbo.apache.org/en/overview/mannual/java-sdk/tasks/protocols/triple/interface/)
- [Apache Dubbo: Triple Protocol Specification](https://dubbo.apache.org/en/overview/reference/protocols/triple-spec/)
- [Apache Dubbo: Triple 3.3 New Features](https://dubbo.apache.org/en/overview/mannual/java-sdk/reference-manual/protocol/triple-3.3/)

The implementation was also checked against the HTTP/JSON decoder, `DefaultRequestMappingRegistry`, `TypeUtils.buildSig`, `TripleHeaderEnum`, and group/version routing conditions in the Apache Dubbo 3.3.6 source.
