// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 获取审批实例ID列表入参（标准版接口，可作为“我发起的”兜底方案）
/// </summary>
/// <remarks>
/// 若只传 startTime，要求时间距当前不超过 120 天；同时传 startTime 与 endTime 时，
/// 时间范围不能超过 120 天，且 startTime 距当前时间不能超过 365 天。
/// </remarks>
public class DingTalkListInstanceIdsInput
{
    /// <summary>
    /// 审批流的唯一码 processCode
    /// </summary>
    [Required(ErrorMessage = "审批模板 processCode 不能为空")]
    public string ProcessCode { get; set; }

    /// <summary>
    /// 审批实例开始时间，Unix 时间戳，单位毫秒
    /// </summary>
    [Required(ErrorMessage = "审批实例开始时间 startTime 不能为空")]
    public long StartTime { get; set; }

    /// <summary>
    /// 审批实例结束时间，Unix 时间戳，单位毫秒。不传默认取当前时间
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 分页游标。首次调用传 0，非首次传上次返回的 nextToken
    /// </summary>
    public long NextToken { get; set; } = 0;

    /// <summary>
    /// 分页参数，每页大小，最多传 20
    /// </summary>
    [Range(1, 20, ErrorMessage = "每页大小 maxResults 取值范围为 1~20")]
    public long MaxResults { get; set; } = 20;

    /// <summary>
    /// 发起人 userId 列表，最大列表长度为 10
    /// </summary>
    public List<string> UserIds { get; set; }

    /// <summary>
    /// 流程实例状态：RUNNING（审批中）、TERMINATED（已撤销）、COMPLETED（审批完成）。
    /// 未传值代表查询所有状态
    /// </summary>
    public List<string> Statuses { get; set; }
}
