// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 按 processCode 初始化审批实例的结果
/// </summary>
public class DingTalkInitWorkflowOutput
{
    /// <summary>
    /// 实际扫描（拉取详情）的实例数量
    /// </summary>
    public int Scanned { get; set; }

    /// <summary>
    /// 新增落库的实例数量
    /// </summary>
    public int Inserted { get; set; }

    /// <summary>
    /// 更新落库的实例数量
    /// </summary>
    public int Updated { get; set; }

    /// <summary>
    /// 是否因达到遍历上限（MaxScan）而提前终止，未覆盖全部实例
    /// </summary>
    public bool Truncated { get; set; }
}
