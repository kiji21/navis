// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 建立 Stream 长连接（POST /v1.0/gateway/connections/open）请求体
/// </summary>
public class DingTalkStreamOpenInput
{
    /// <summary>
    /// 应用 clientId（原 AppKey）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("clientId")]
    [System.Text.Json.Serialization.JsonPropertyName("clientId")]
    public string ClientId { get; set; }

    /// <summary>
    /// 应用 clientSecret（原 AppSecret）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("clientSecret")]
    [System.Text.Json.Serialization.JsonPropertyName("clientSecret")]
    public string ClientSecret { get; set; }

    /// <summary>
    /// 订阅的主题列表（EVENT/SYSTEM/CALLBACK）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("subscriptions")]
    [System.Text.Json.Serialization.JsonPropertyName("subscriptions")]
    public List<DingTalkStreamSubscription> Subscriptions { get; set; }

    /// <summary>
    /// 客户端标识（UserAgent），用于问题排查
    /// </summary>
    [Newtonsoft.Json.JsonProperty("ua")]
    [System.Text.Json.Serialization.JsonPropertyName("ua")]
    public string Ua { get; set; }

    /// <summary>
    /// 本地 IP（可选，便于排查）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("localIp")]
    [System.Text.Json.Serialization.JsonPropertyName("localIp")]
    public string LocalIp { get; set; }
}

/// <summary>
/// Stream 订阅项
/// </summary>
public class DingTalkStreamSubscription
{
    /// <summary>
    /// 订阅类型：EVENT（事件）/ SYSTEM（系统）/ CALLBACK（回调）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("type")]
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 主题：EVENT 类型固定为 "*"（订阅全部事件）；CALLBACK 类型为具体回调 topic
    /// </summary>
    [Newtonsoft.Json.JsonProperty("topic")]
    [System.Text.Json.Serialization.JsonPropertyName("topic")]
    public string Topic { get; set; }
}

/// <summary>
/// 建立 Stream 长连接响应体
/// </summary>
public class DingTalkStreamOpenOutput
{
    /// <summary>
    /// WebSocket 网关接入地址（wss://...），需拼接 ticket 后建连
    /// </summary>
    [Newtonsoft.Json.JsonProperty("endpoint")]
    [System.Text.Json.Serialization.JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>
    /// 建连票据，作为 query 参数 ticket 拼接到 endpoint 后
    /// </summary>
    [Newtonsoft.Json.JsonProperty("ticket")]
    [System.Text.Json.Serialization.JsonPropertyName("ticket")]
    public string Ticket { get; set; }
}

/// <summary>
/// Stream 网关下推的消息帧（EVENT / SYSTEM / CALLBACK）
/// </summary>
public class DingTalkStreamFrame
{
    /// <summary>
    /// 帧类型：EVENT / SYSTEM / CALLBACK
    /// </summary>
    [Newtonsoft.Json.JsonProperty("type")]
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 帧头（含 messageId、topic、eventType 等）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("headers")]
    [System.Text.Json.Serialization.JsonPropertyName("headers")]
    public DingTalkStreamFrameHeaders Headers { get; set; }

    /// <summary>
    /// 业务数据（JSON 字符串，需二次反序列化）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("data")]
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public string Data { get; set; }

    /// <summary>
    /// 规格版本
    /// </summary>
    [Newtonsoft.Json.JsonProperty("specVersion")]
    [System.Text.Json.Serialization.JsonPropertyName("specVersion")]
    public string SpecVersion { get; set; }
}

/// <summary>
/// Stream 消息帧头
/// </summary>
public class DingTalkStreamFrameHeaders
{
    /// <summary>
    /// 消息唯一ID，回 ACK 时需原样带回
    /// </summary>
    [Newtonsoft.Json.JsonProperty("messageId")]
    [System.Text.Json.Serialization.JsonPropertyName("messageId")]
    public string MessageId { get; set; }

    /// <summary>
    /// 主题：SYSTEM 帧为 ping/disconnect 等；EVENT 帧为具体事件主题
    /// </summary>
    [Newtonsoft.Json.JsonProperty("topic")]
    [System.Text.Json.Serialization.JsonPropertyName("topic")]
    public string Topic { get; set; }

    /// <summary>
    /// 事件类型（EVENT 帧），如 bpms_instance_change
    /// </summary>
    [Newtonsoft.Json.JsonProperty("eventType")]
    [System.Text.Json.Serialization.JsonPropertyName("eventType")]
    public string EventType { get; set; }

    /// <summary>
    /// 事件唯一ID（EVENT 帧），可用于幂等
    /// </summary>
    [Newtonsoft.Json.JsonProperty("eventId")]
    [System.Text.Json.Serialization.JsonPropertyName("eventId")]
    public string EventId { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    [Newtonsoft.Json.JsonProperty("contentType")]
    [System.Text.Json.Serialization.JsonPropertyName("contentType")]
    public string ContentType { get; set; }
}

/// <summary>
/// 回给 Stream 网关的 ACK 响应帧
/// </summary>
public class DingTalkStreamAck
{
    /// <summary>
    /// 状态码，200 表示处理成功
    /// </summary>
    [Newtonsoft.Json.JsonProperty("code")]
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    [Newtonsoft.Json.JsonProperty("message")]
    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// 响应头（回填 messageId）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("headers")]
    [System.Text.Json.Serialization.JsonPropertyName("headers")]
    public DingTalkStreamAckHeaders Headers { get; set; }

    /// <summary>
    /// 业务处理结果（JSON 字符串）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("data")]
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public string Data { get; set; }
}

/// <summary>
/// ACK 响应头
/// </summary>
public class DingTalkStreamAckHeaders
{
    /// <summary>
    /// 原样带回请求帧的 messageId
    /// </summary>
    [Newtonsoft.Json.JsonProperty("messageId")]
    [System.Text.Json.Serialization.JsonPropertyName("messageId")]
    public string MessageId { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    [Newtonsoft.Json.JsonProperty("contentType")]
    [System.Text.Json.Serialization.JsonPropertyName("contentType")]
    public string ContentType { get; set; }
}
