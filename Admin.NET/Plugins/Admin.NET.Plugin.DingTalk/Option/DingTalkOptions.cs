// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

public sealed class DingTalkOptions : IConfigurableOptions
{
    /// <summary>
    /// AppId
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// AgentId
    /// </summary>
    public string AgentId { get; set; }

    /// <summary>
    /// 原 AppKey 和 SuiteKey
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// 原 AppSecret 和 SuiteSecret
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// 钉钉卡片消息ID
    /// </summary>
    public string ApiCardTemplateId { get; set; }

    /// <summary>
    /// 钉钉机器人
    /// </summary>
    public string RobotCode { get; set; }

    /// <summary>
    /// 是否启用 Stream 长连接接收事件订阅（默认启用）。
    /// 关闭后不再建立 WebSocket 长连接，也不会接收/落库审批事件。
    /// </summary>
    public bool StreamEnabled { get; set; }

    /// <summary>
    /// 是否输出 Stream 收发帧的详细调试日志（默认关闭）。
    /// 首次联调时开启，可观察网关下推的原始帧与回 ACK 内容；生产环境建议关闭以免刷屏。
    /// </summary>
    public bool StreamDebugLog { get; set; }

    /// <summary>
    /// Stream 协议帧类型：系统指令（ping/disconnect 等）。默认 SYSTEM
    /// </summary>
    public string StreamTypeSystem { get; set; }

    /// <summary>
    /// Stream 协议帧类型：业务事件。默认 EVENT
    /// </summary>
    public string StreamTypeEvent { get; set; }

    /// <summary>
    /// Stream 断线重连间隔基准（秒），实际按尝试次数指数退避，上限 60s。默认 3
    /// </summary>
    public int ReconnectBaseSeconds { get; set; }

    /// <summary>
    /// 钉钉同步部门标记（写入 SysOrg.Remark，并作为同步部门的查询/删除条件）。默认「钉钉同步」
    /// </summary>
    public string DingTalkInfoRemark { get; set; }

    /// <summary>
    /// 默认租户Id（同步部门/角色/用户时写入 TenantId）。默认 1300000000001
    /// </summary>
    public long DefaultTenantId { get; set; }

    /// <summary>
    /// 根部门占位 Pid（钉钉根部门 parent_id=1 时映射到本地根机构）。默认同 DefaultTenantId
    /// </summary>
    public long RootOrgPid { get; set; }
}