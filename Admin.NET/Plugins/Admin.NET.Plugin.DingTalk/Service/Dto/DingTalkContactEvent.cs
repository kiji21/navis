// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 钉钉通讯录/组织/角色事件体（宽松承接各事件公共字段）。
/// </summary>
/// <remarks>
/// 说明：钉钉通讯录事件体仅携带 id 列表，不含姓名/工号/部门名/角色名等展示字段。
/// 用户类事件带 <see cref="UserId"/>；部门类事件带 <see cref="DeptId"/>；
/// 角色配置类事件带 <see cref="LabelIdList"/>；员工角色变更事件同时带 <see cref="UserId"/> 与 <see cref="LabelIdList"/>。
/// </remarks>
public class DingTalkContactEvent
{
    /// <summary>
    /// 事件类型，如 user_add_org / org_dept_modify / label_conf_add
    /// </summary>
    [Newtonsoft.Json.JsonProperty("eventType")]
    [System.Text.Json.Serialization.JsonPropertyName("eventType")]
    public string EventType { get; set; }

    /// <summary>
    /// 企业corpId
    /// </summary>
    [Newtonsoft.Json.JsonProperty("corpId")]
    [System.Text.Json.Serialization.JsonPropertyName("corpId")]
    public string CorpId { get; set; }

    /// <summary>
    /// 用户 userId 列表（用户增/改/离职、员工角色变更事件，JSON 字段名 userIdList）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("userIdList")]
    [System.Text.Json.Serialization.JsonPropertyName("userIdList")]
    public List<string> UserId { get; set; }

    /// <summary>
    /// 部门 id 列表（部门创建/修改/删除事件，JSON 字段名 deptId）。
    /// 注意：钉钉部门事件体字段是 deptId（非 deptIdList），与用户/角色事件命名不一致。
    /// </summary>
    [Newtonsoft.Json.JsonProperty("deptId")]
    [System.Text.Json.Serialization.JsonPropertyName("deptId")]
    public List<long> DeptId { get; set; }

    /// <summary>
    /// 部门 id 列表兼容字段（个别事件版本可能下发 deptIdList）。
    /// 取部门 id 时用 <see cref="DeptIds"/> 合并读取，避免因字段命名差异漏处理。
    /// </summary>
    [Newtonsoft.Json.JsonProperty("deptIdList")]
    [System.Text.Json.Serialization.JsonPropertyName("deptIdList")]
    public List<long> DeptIdListCompat { get; set; }

    /// <summary>
    /// 合并后的部门 id 列表（兼容 deptId 与 deptIdList 两种命名）。
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public List<long> DeptIds =>
        (DeptId ?? new List<long>())
            .Concat(DeptIdListCompat ?? new List<long>())
            .Distinct()
            .ToList();

    /// <summary>
    /// 角色/角色组 id 列表（角色配置类事件；员工角色变更时为涉及的角色id，JSON 字段名 labelIdList）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("labelIdList")]
    [System.Text.Json.Serialization.JsonPropertyName("labelIdList")]
    public List<long> LabelIdList { get; set; }

    /// <summary>
    /// 员工角色变更动作：add（新增角色）/ remove（移除角色）
    /// </summary>
    [Newtonsoft.Json.JsonProperty("action")]
    [System.Text.Json.Serialization.JsonPropertyName("action")]
    public string Action { get; set; }

    /// <summary>
    /// 事件发生时间，Unix 毫秒时间戳
    /// </summary>
    [Newtonsoft.Json.JsonProperty("timeStamp")]
    [System.Text.Json.Serialization.JsonPropertyName("timeStamp")]
    public long? TimeStamp { get; set; }
}
