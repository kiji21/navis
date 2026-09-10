// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 通过免登码获取用户信息出参
/// </summary>
public class GetDingTalkUserInfoOutput
{
    /// <summary>
    /// 用户的 userId
    /// </summary>
    [JsonProperty("userid")]
    [System.Text.Json.Serialization.JsonPropertyName("userid")]
    public string userid { get; set; }

    /// <summary>
    /// 用户的 unionId
    /// </summary>
    [JsonProperty("unionid")]
    [System.Text.Json.Serialization.JsonPropertyName("unionid")]
    public string unionid { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    [JsonProperty("name")]
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string name { get; set; }

    /// <summary>
    /// 员工的直属主管在钉钉群的 unionId（关联的 unionId）
    /// </summary>
    [JsonProperty("associated_unionid")]
    [System.Text.Json.Serialization.JsonPropertyName("associated_unionid")]
    public string associated_unionid { get; set; }

    /// <summary>
    /// 授权的手机设备号（登录时使用的设备）
    /// </summary>
    [JsonProperty("device_id")]
    [System.Text.Json.Serialization.JsonPropertyName("device_id")]
    public string device_id { get; set; }

    /// <summary>
    /// 是否是管理员账号
    /// </summary>
    [JsonProperty("sys")]
    [System.Text.Json.Serialization.JsonPropertyName("sys")]
    public bool sys { get; set; }

    /// <summary>
    /// 级别：1 主管理员；2 子管理员；100 老板；0 其他（如普通员工）
    /// </summary>
    [JsonProperty("sys_level")]
    [System.Text.Json.Serialization.JsonPropertyName("sys_level")]
    public int sys_level { get; set; }
}
