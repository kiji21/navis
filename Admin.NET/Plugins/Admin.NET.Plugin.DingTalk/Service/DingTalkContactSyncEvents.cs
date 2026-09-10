// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 钉钉同步变更类型（新增/更新/删除）。
/// </summary>
[IgnoreEnumToDict]
public enum DingTalkSyncChangeType
{
    /// <summary>
    /// 新增（本地原本不存在）。
    /// </summary>
    Added,

    /// <summary>
    /// 更新（本地已存在，用最新详情覆盖，含恢复软删）。
    /// </summary>
    Updated,

    /// <summary>
    /// 删除（部门/角色物理删除，用户离职软删）。
    /// </summary>
    Removed,
}

/// <summary>
/// 钉钉用户同步变更事件参数。
/// </summary>
public class DingTalkUserSyncedEventArgs : EventArgs
{
    /// <summary>
    /// 变更类型。
    /// </summary>
    public DingTalkSyncChangeType ChangeType { get; init; }

    /// <summary>
    /// 本次受影响的钉钉用户（Added/Updated 时为落库后的实体；Removed 时为被软删的实体）。
    /// </summary>
    public IReadOnlyList<DingTalkUser> Users { get; init; } = new List<DingTalkUser>();
}

/// <summary>
/// 钉钉部门同步变更事件参数。
/// </summary>
public class DingTalkDeptSyncedEventArgs : EventArgs
{
    /// <summary>
    /// 变更类型。
    /// </summary>
    public DingTalkSyncChangeType ChangeType { get; init; }

    /// <summary>
    /// 本次受影响的系统机构（Added/Updated 为落库后的实体；Removed 为被删除的实体）。
    /// </summary>
    public IReadOnlyList<SysOrg> Orgs { get; init; } = new List<SysOrg>();
}

/// <summary>
/// 钉钉角色同步变更事件参数。
/// </summary>
public class DingTalkRoleSyncedEventArgs : EventArgs
{
    /// <summary>
    /// 变更类型。
    /// </summary>
    public DingTalkSyncChangeType ChangeType { get; init; }

    /// <summary>
    /// 本次受影响的系统角色（Added/Updated 为落库后的实体；Removed 为被删除的实体）。
    /// </summary>
    public IReadOnlyList<SysRole> Roles { get; init; } = new List<SysRole>();
}

/// <summary>
/// 钉钉用户角色关系变更事件参数（对应 SyncUserRolesFromDetail 重建的结果）。
/// </summary>
public class DingTalkUserRoleChangedEventArgs : EventArgs
{
    /// <summary>
    /// 新增到 SysUserRole 的关系。
    /// </summary>
    public IReadOnlyList<SysUserRole> Added { get; init; } = new List<SysUserRole>();

    /// <summary>
    /// 从 SysUserRole 移除的关系。
    /// </summary>
    public IReadOnlyList<SysUserRole> Removed { get; init; } = new List<SysUserRole>();
}

/// <summary>
/// 钉钉通讯录同步变更事件中心 🧩
/// 供其他插件在自身 Startup（或任意初始化处）订阅，感知部门/角色/用户及用户角色关系的落库变更，从而做联动业务。
/// </summary>
/// <remarks>
/// 使用注意：
/// 1. 事件为 <b>静态</b> 事件，订阅方需在合适时机（一次性）订阅，避免重复订阅导致回调多次；
///    若订阅者为可释放对象，务必在释放时取消订阅，防止内存泄漏。
/// 2. 事件在对应同步操作 <b>落库成功后</b> 触发，且仅当本批确有变更时才触发（无变更不触发）。
/// 3. 回调在钉钉 Stream 后台线程上下文触发；订阅方若需数据库/仓储，请自行创建作用域（如 App.GetService/CreateScope）。
/// 4. 事件采用异步委托，触发方会 await 所有订阅者；订阅方应自行 try/catch，避免异常影响其他订阅者与主流程。
/// </remarks>
public static class DingTalkContactSyncEvents
{
    /// <summary>
    /// 异步事件处理委托。
    /// </summary>
    public delegate Task AsyncEventHandler<in TArgs>(object sender, TArgs args)
        where TArgs : EventArgs;

    /// <summary>
    /// 用户同步变更（新增/更新/离职）后触发。
    /// </summary>
    public static event AsyncEventHandler<DingTalkUserSyncedEventArgs> UserSynced;

    /// <summary>
    /// 部门同步变更（新增/更新/删除）后触发。
    /// </summary>
    public static event AsyncEventHandler<DingTalkDeptSyncedEventArgs> DeptSynced;

    /// <summary>
    /// 角色同步变更（新增/更新/删除）后触发。
    /// </summary>
    public static event AsyncEventHandler<DingTalkRoleSyncedEventArgs> RoleSynced;

    /// <summary>
    /// 用户角色关系（SysUserRole）重建后触发。
    /// </summary>
    public static event AsyncEventHandler<DingTalkUserRoleChangedEventArgs> UserRoleChanged;

    internal static Task RaiseUserSyncedAsync(object sender, DingTalkUserSyncedEventArgs args) =>
        InvokeAsync(UserSynced, sender, args);

    internal static Task RaiseDeptSyncedAsync(object sender, DingTalkDeptSyncedEventArgs args) =>
        InvokeAsync(DeptSynced, sender, args);

    internal static Task RaiseRoleSyncedAsync(object sender, DingTalkRoleSyncedEventArgs args) =>
        InvokeAsync(RoleSynced, sender, args);

    internal static Task RaiseUserRoleChangedAsync(object sender, DingTalkUserRoleChangedEventArgs args) =>
        InvokeAsync(UserRoleChanged, sender, args);

    /// <summary>
    /// 依次 await 每个订阅者，单个订阅者异常不影响其他订阅者与主流程（仅记录日志）。
    /// </summary>
    private static async Task InvokeAsync<TArgs>(AsyncEventHandler<TArgs> handler, object sender, TArgs args)
        where TArgs : EventArgs
    {
        if (handler == null)
            return;

        foreach (var invocation in handler.GetInvocationList().Cast<AsyncEventHandler<TArgs>>())
        {
            try
            {
                await invocation(sender, args);
            }
            catch (Exception ex)
            {
                Furion.Logging.Log.Error($"钉钉同步事件订阅者执行异常（{typeof(TArgs).Name}）：{ex.Message}", ex);
            }
        }
    }
}
