// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 审批单归类查询入参（标准版方案：轮询遍历 processCode 下实例后按 userId 归类）
/// </summary>
public class DingTalkMyProcessQueryInput
{
    /// <summary>
    /// 当前用户 userId（用于归类“我发起的/我审批的/我收到的”）
    /// </summary>
    [Required(ErrorMessage = "用户 userId 不能为空")]
    public string UserId { get; set; }

    /// <summary>
    /// 审批流的唯一码 processCode
    /// </summary>
    [Required(ErrorMessage = "审批模板 processCode 不能为空")]
    public string ProcessCode { get; set; }

    /// <summary>
    /// 审批实例开始时间，Unix 时间戳，单位毫秒。
    /// 说明：仅传 startTime 时距当前不超过 120 天；同时传 startTime 与 endTime 时范围不超过 120 天，且 startTime 距当前不超过 365 天。
    /// </summary>
    [Required(ErrorMessage = "审批实例开始时间 startTime 不能为空")]
    public long StartTime { get; set; }

    /// <summary>
    /// 审批实例结束时间，Unix 时间戳，单位毫秒。不传默认取当前时间
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 单次拉取实例ID分页大小，最多 20
    /// </summary>
    [Range(1, 20, ErrorMessage = "每页大小 maxResults 取值范围为 1~20")]
    public long MaxResults { get; set; } = 20;

    /// <summary>
    /// 最多遍历的实例数量上限（防止数据量过大导致长时间阻塞），默认 200
    /// </summary>
    [Range(1, 10000, ErrorMessage = "遍历上限 maxScan 取值范围为 1~10000")]
    public int MaxScan { get; set; } = 200;
}
