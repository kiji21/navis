// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 审批单归类查询结果（标准版方案，一次返回“我发起的/我审批的/我收到的”三类）
/// </summary>
public class DingTalkMyProcessListOutput
{
    /// <summary>
    /// 我发起的（originatorUserId == 当前 userId）
    /// </summary>
    public List<DingTalkMyProcessItem> Submitted { get; set; } = new();

    /// <summary>
    /// 我审批的（tasks[].userId 或 approverUserIds 含当前 userId）
    /// </summary>
    public List<DingTalkMyProcessItem> Approve { get; set; } = new();

    /// <summary>
    /// 我收到的（ccUserIds 含当前 userId）
    /// </summary>
    public List<DingTalkMyProcessItem> Received { get; set; } = new();

    /// <summary>
    /// 实际遍历的实例数量
    /// </summary>
    public int ScannedCount { get; set; }

    /// <summary>
    /// 是否因达到遍历上限（MaxScan）而提前终止，未覆盖全部实例
    /// </summary>
    public bool Truncated { get; set; }
}

/// <summary>
/// 归类后的审批单条目
/// </summary>
public class DingTalkMyProcessItem
{
    /// <summary>
    /// 审批实例ID
    /// </summary>
    public string ProcessInstanceId { get; set; }

    /// <summary>
    /// 审批实例标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 审批单业务编号
    /// </summary>
    public string BusinessId { get; set; }

    /// <summary>
    /// 发起人 userId
    /// </summary>
    public string OriginatorUserId { get; set; }

    /// <summary>
    /// 发起人部门名称
    /// </summary>
    public string OriginatorDeptName { get; set; }

    /// <summary>
    /// 审批状态 RUNNING：审批中 TERMINATED：已撤销 COMPLETED：审批完成
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 审批结果 agree：同意 refuse：拒绝
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 当前用户在“我审批的”中的任务状态（仅 Approve 列表有意义）：
    /// RUNNING 表示待我审批，其他表示我已处理。
    /// </summary>
    public string MyTaskStatus { get; set; }
}
