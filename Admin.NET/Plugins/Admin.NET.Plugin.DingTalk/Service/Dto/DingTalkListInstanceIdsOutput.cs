// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 获取审批实例ID列表响应（标准版接口）
/// </summary>
public class DingTalkListInstanceIdsOutput
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public DingTalkListInstanceIdsResult Result { get; set; }

    /// <summary>
    /// 接口请求是否成功
    /// </summary>
    public bool Success { get; set; }
}

/// <summary>
/// 审批实例ID列表结果集
/// </summary>
public class DingTalkListInstanceIdsResult
{
    /// <summary>
    /// 审批实例ID列表
    /// </summary>
    public List<string> List { get; set; }

    /// <summary>
    /// 分页游标，不为空表示有更多数据
    /// </summary>
    public string NextToken { get; set; }
}
