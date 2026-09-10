// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 获取用户详情入参（/v2/user/get）
/// </summary>
public class GetDingTalkUserDetailInput
{
    /// <summary>
    /// 用户的userId
    /// </summary>
    [Newtonsoft.Json.JsonProperty("userid")]
    [System.Text.Json.Serialization.JsonPropertyName("userid")]
    public string userid { get; set; }

    /// <summary>
    /// 通讯录语言（zh_CN/en_US）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("language")]
    [System.Text.Json.Serialization.JsonPropertyName("language")]
    public string language { get; set; } = "zh_CN";
}

/// <summary>
/// 用户详情返回（/v2/user/get）
/// </summary>
public class DingTalkUserDetailOutput
{
    /// <summary>
    /// 用户的userId
    /// </summary>
    [Newtonsoft.Json.JsonProperty("userid")]
    [System.Text.Json.Serialization.JsonPropertyName("userid")]
    public string UserId { get; set; }

    /// <summary>
    /// 员工在当前开发者企业账号范围内的唯一标识
    /// </summary>
    [Newtonsoft.Json.JsonProperty("unionid")]
    [System.Text.Json.Serialization.JsonPropertyName("unionid")]
    public string UnionId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    [Newtonsoft.Json.JsonProperty("name")]
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Newtonsoft.Json.JsonProperty("mobile")]
    [System.Text.Json.Serialization.JsonPropertyName("mobile")]
    public string Mobile { get; set; }

    /// <summary>
    /// 员工工号
    /// </summary>
    [Newtonsoft.Json.JsonProperty("job_number")]
    [System.Text.Json.Serialization.JsonPropertyName("job_number")]
    public string JobNumber { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [Newtonsoft.Json.JsonProperty("title")]
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [Newtonsoft.Json.JsonProperty("avatar")]
    [System.Text.Json.Serialization.JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    /// <summary>
    /// 所属部门id列表
    /// </summary>
    [Newtonsoft.Json.JsonProperty("dept_id_list")]
    [System.Text.Json.Serialization.JsonPropertyName("dept_id_list")]
    public List<long> DeptIdList { get; set; }

    /// <summary>
    /// 用户所属角色列表（/v2/user/get 返回，仅含 id/name/group_name，无 group_id）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("role_list")]
    [System.Text.Json.Serialization.JsonPropertyName("role_list")]
    public List<DingTalkUserRoleItem> RoleList { get; set; }
}

/// <summary>
/// 用户角色项（/v2/user/get 的 role_list 元素）
/// </summary>
public class DingTalkUserRoleItem
{
    /// <summary>
    /// 角色id
    /// </summary>
    [Newtonsoft.Json.JsonProperty("id")]
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [Newtonsoft.Json.JsonProperty("name")]
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 角色所属的角色组名称
    /// </summary>
    [Newtonsoft.Json.JsonProperty("group_name")]
    [System.Text.Json.Serialization.JsonPropertyName("group_name")]
    public string GroupName { get; set; }
}
