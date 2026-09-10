// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 审批中心列表查询入参（OA高级版专享接口：待处理/已处理/已发起/我收到的）
/// </summary>
public class DingTalkProcessCentreQueryInput
{
    /// <summary>
    /// 审批任务执行人/抄送人的用户 userId
    /// </summary>
    [Required(ErrorMessage = "用户 userId 不能为空")]
    public string UserId { get; set; }

    /// <summary>
    /// 分页大小，从 1 开始，最大值 20
    /// </summary>
    [Range(1, 20, ErrorMessage = "分页大小 pageSize 取值范围为 1~20")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 页码，从 1 开始，最大限制 10
    /// </summary>
    [Range(1, 10, ErrorMessage = "页码 pageNumber 取值范围为 1~10")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 查询的待办任务创建时间要小于该值，为 null 则取全部。
    /// 该时间距离当前时间不能超过一年，仅“待处理任务”接口支持该参数。
    /// </summary>
    public string CreateBefore { get; set; }
}
