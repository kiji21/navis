// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 审批实例变更事件（bpms_instance_change）数据结构。
/// Stream 长连接下即为 EVENT 帧 data 反序列化后的对象。
/// </summary>
public class DingTalkBpmsInstanceChangeEvent
{
    /// <summary>
    /// 事件类型，如 bpms_instance_change / bpms_task_change
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// 审批实例ID
    /// </summary>
    public string ProcessInstanceId { get; set; }

    /// <summary>
    /// 审批模板唯一码
    /// </summary>
    public string ProcessCode { get; set; }

    /// <summary>
    /// 企业corpId
    /// </summary>
    public string CorpId { get; set; }

    /// <summary>
    /// 审批标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 变更类型 start：审批实例开始 finish：审批实例结束
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 发起人 userId
    /// </summary>
    public string StaffId { get; set; }

    /// <summary>
    /// 审批结果 agree：同意 refuse：拒绝（type=finish 时返回）
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// 审批单业务编号
    /// </summary>
    public string BusinessId { get; set; }

    /// <summary>
    /// 详情页链接
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 创建时间，Unix 毫秒时间戳
    /// </summary>
    public long? CreateTime { get; set; }

    /// <summary>
    /// 结束时间，Unix 毫秒时间戳（type=finish 时返回）
    /// </summary>
    public long? FinishTime { get; set; }
}
