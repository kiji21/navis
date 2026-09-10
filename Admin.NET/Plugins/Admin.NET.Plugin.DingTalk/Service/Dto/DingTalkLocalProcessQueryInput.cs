// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 本地审批单查询入参（数据来源：回调落库的 DingTalkWokerflowLog 表）
/// </summary>
public class DingTalkLocalProcessQueryInput
{
    /// <summary>
    /// 当前用户 userId
    /// </summary>
    [Required(ErrorMessage = "用户 userId 不能为空")]
    public string UserId { get; set; }

    /// <summary>
    /// 审批状态过滤：RUNNING/TERMINATED/COMPLETED，为空则不过滤
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 页码，从 1 开始
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码 page 必须大于 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页大小 pageSize 取值范围为 1~100")]
    public int PageSize { get; set; } = 20;
}
