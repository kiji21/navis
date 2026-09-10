// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 审批中心实例列表响应（已发起/我收到的实例，OA高级版专享）
/// </summary>
public class DingTalkProcessCentreInstanceOutput
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public DingTalkProcessCentreInstanceResult Result { get; set; }

    /// <summary>
    /// 接口是否调用成功
    /// </summary>
    public bool Success { get; set; }
}

/// <summary>
/// 审批中心实例列表结果集
/// </summary>
public class DingTalkProcessCentreInstanceResult
{
    /// <summary>
    /// 实例列表
    /// </summary>
    public List<DingTalkProcessCentreInstanceItem> List { get; set; }

    /// <summary>
    /// 是否还有下一页
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// 审批中心实例项（已发起/我收到的）
/// </summary>
public class DingTalkProcessCentreInstanceItem
{
    /// <summary>
    /// 实例ID
    /// </summary>
    public string ProcessInstanceId { get; set; }

    /// <summary>
    /// 实例处理结果 agree：同意 refuse：拒绝
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// 实例状态 RUNNING：审批中 TERMINATED：已撤销 COMPLETED：审批完成
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 实例发起时间，iso8601 格式
    /// </summary>
    public string ProcessCreateTime { get; set; }

    /// <summary>
    /// 实例完成时间，iso8601 格式
    /// </summary>
    public string ProcessEndTime { get; set; }

    /// <summary>
    /// 发起人姓名
    /// </summary>
    public string OriginatorName { get; set; }

    /// <summary>
    /// 发起人ID
    /// </summary>
    public string OriginatorId { get; set; }

    /// <summary>
    /// 发起人头像
    /// </summary>
    public string OriginatorPhoto { get; set; }

    /// <summary>
    /// 摘要
    /// </summary>
    public string FormMassage { get; set; }

    /// <summary>
    /// 实例详情页链接
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 审批类型 0：官方OA审批 1：自有OA审批
    /// </summary>
    public int ProcessType { get; set; }
}
