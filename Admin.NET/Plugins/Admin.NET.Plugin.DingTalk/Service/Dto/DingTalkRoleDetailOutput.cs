// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 获取角色详情入参（/role/getrole）
/// </summary>
public class GetDingTalkRoleDetailInput
{
    /// <summary>
    /// 角色id
    /// </summary>
    [Newtonsoft.Json.JsonProperty("roleId")]
    [System.Text.Json.Serialization.JsonPropertyName("roleId")]
    public long roleId { get; set; }
}

/// <summary>
/// 角色详情返回（/role/getrole）
/// </summary>
/// <remarks>
/// 该接口的 role 直接位于返回体顶层（与 errcode/errmsg 平级），不在 result 之下，
/// 故单独定义响应类型，避免用 DingTalkBaseResponse&lt;T&gt;（Result）解析导致 role 为 null。
/// </remarks>
public class DingTalkRoleGetResponse
{
    /// <summary>
    /// 返回码（0 表示成功）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("errcode")]
    [System.Text.Json.Serialization.JsonPropertyName("errcode")]
    public int ErrCode { get; set; }

    /// <summary>
    /// 返回码描述
    /// </summary>
    [Newtonsoft.Json.JsonProperty("errmsg")]
    [System.Text.Json.Serialization.JsonPropertyName("errmsg")]
    public string ErrMsg { get; set; }

    /// <summary>
    /// 请求Id
    /// </summary>
    [Newtonsoft.Json.JsonProperty("request_id")]
    [System.Text.Json.Serialization.JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    /// <summary>
    /// 角色详情（顶层字段）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("role")]
    [System.Text.Json.Serialization.JsonPropertyName("role")]
    public DingTalkRoleDetail role { get; set; }
}

/// <summary>
/// 角色详情返回（/role/getrole）
/// </summary>
public class DingTalkRoleDetailOutput
{
    /// <summary>
    /// 角色详情
    /// </summary>
    [Newtonsoft.Json.JsonProperty("role")]
    [System.Text.Json.Serialization.JsonPropertyName("role")]
    public DingTalkRoleDetail role { get; set; }
}

/// <summary>
/// 角色详情内容
/// </summary>
public class DingTalkRoleDetail
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Newtonsoft.Json.JsonProperty("name")]
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string name { get; set; }

    /// <summary>
    /// 角色所属角色组id
    /// </summary>
    [Newtonsoft.Json.JsonProperty("groupId")]
    [System.Text.Json.Serialization.JsonPropertyName("groupId")]
    public long groupId { get; set; }
}
