// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using Microsoft.Extensions.Options;

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 钉钉通讯录/组织/角色事件落库器 🧩
/// </summary>
/// <remarks>
/// 将 Stream 下推的通讯录、部门、角色变更事件增量更新到本地表：
/// DingTalkUser / DingTalkRoleUser / SysOrg / SysRole / SysUserRole。
/// 字段映射与删除/软删约定沿用 SyncDingTalkUserJob、SyncDingTalkDeptJob、SyncDingTalkRoleJob。
/// 钉钉通讯录事件体仅携带 id，不含展示字段，故收到 新增/修改 事件时先查本地，
/// 本地无则调用对应“详情接口”补全后落库；有则用最新详情覆盖更新。
/// - 部门：/v2/department/get → SysOrg
/// - 角色：/role/getrole   → SysRole
/// - 用户：/v2/user/get     → DingTalkUser + SysUser + DingTalkRoleUser
/// </remarks>
public class DingTalkContactPersister : IScoped
{
    private readonly SqlSugarRepository<DingTalkUser> _dingTalkUserRep;
    private readonly SqlSugarRepository<DingTalkRoleUser> _dingTalkRoleUserRep;
    private readonly SqlSugarRepository<SysOrg> _sysOrgRep;
    private readonly SqlSugarRepository<SysRole> _sysRoleRep;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly SqlSugarRepository<SysUserRole> _sysUserRoleRep;
    private readonly IDingTalkApi _dingTalkApi;
    private readonly DingTalkOptions _dingTalkOptions;

    public DingTalkContactPersister(
        SqlSugarRepository<DingTalkUser> dingTalkUserRep,
        SqlSugarRepository<DingTalkRoleUser> dingTalkRoleUserRep,
        SqlSugarRepository<SysOrg> sysOrgRep,
        SqlSugarRepository<SysRole> sysRoleRep,
        SqlSugarRepository<SysUser> sysUserRep,
        SqlSugarRepository<SysUserRole> sysUserRoleRep,
        IDingTalkApi dingTalkApi,
        IOptions<DingTalkOptions> dingTalkOptions
    )
    {
        _dingTalkUserRep = dingTalkUserRep;
        _dingTalkRoleUserRep = dingTalkRoleUserRep;
        _sysOrgRep = sysOrgRep;
        _sysRoleRep = sysRoleRep;
        _sysUserRep = sysUserRep;
        _sysUserRoleRep = sysUserRoleRep;
        _dingTalkApi = dingTalkApi;
        _dingTalkOptions = dingTalkOptions.Value;
    }

    /// <summary>
    /// 按事件类型分发处理。返回 true 表示已识别并处理（无论是否有数据变更）。
    /// </summary>
    public async Task<bool> HandleAsync(string eventType, DingTalkContactEvent evt)
    {
        if (evt == null)
            return false;

        switch (eventType)
        {
            case DingTalkConst.Events.UserAddOrg:
            case DingTalkConst.Events.UserModifyOrg:
                await UpsertUsersAsync(evt.UserId);
                return true;

            case DingTalkConst.Events.UserLeaveOrg:
                await LeaveUsersAsync(evt.UserId);
                return true;

            case DingTalkConst.Events.OrgDeptCreate:
            case DingTalkConst.Events.OrgDeptModify:
                await UpsertDeptsAsync(evt.DeptIds, eventType);
                return true;

            case DingTalkConst.Events.OrgDeptRemove:
                await RemoveDeptsAsync(evt.DeptIds);
                return true;

            case DingTalkConst.Events.LabelConfAdd:
            case DingTalkConst.Events.LabelConfModify:
                await UpsertRolesAsync(evt.LabelIdList, eventType);
                return true;

            case DingTalkConst.Events.LabelConfDel:
                await RemoveRolesAsync(evt.LabelIdList);
                return true;

            case DingTalkConst.Events.LabelUserChange:
                // 员工角色变更：事件后 /v2/user/get 的 role_list 已反映最新角色，
                // 直接复用用户 Upsert 按接口结果重建 DingTalkRoleUser / SysUserRole。
                await ChangeUserRolesAsync(evt.UserId, evt.LabelIdList, evt.Action);
                return true;

            default:
                return false;
        }
    }

    #region 主动同步入口（供 API 单条拉取后落库复用）

    /// <summary>
    /// 按部门Id调用 /v2/department/get 拉取详情并落库 SysOrg（无则新增/有则更新）。
    /// </summary>
    public Task UpsertDeptsByIdsAsync(List<long> deptIds) =>
        UpsertDeptsAsync(deptIds, "api/department/get");

    /// <summary>
    /// 按用户Id调用 /v2/user/get 拉取详情并落库 DingTalkUser + SysUser + 角色关系（无则新增/有则更新）。
    /// </summary>
    public Task UpsertUsersByIdsAsync(List<string> userIds) => UpsertUsersAsync(userIds);

    /// <summary>
    /// 按角色Id调用 /role/getrole 拉取详情并落库 SysRole（无则新增/有则更新）。
    /// </summary>
    public Task UpsertRolesByIdsAsync(List<long> roleIds) =>
        UpsertRolesAsync(roleIds, "api/role/getrole");

    #endregion 主动同步入口（供 API 单条拉取后落库复用）

    #region 用户

    /// <summary>
    /// 用户增加/更改：按 DingTalkUserId 落库。
    /// 本地不存在则调用 /v2/user/get 补全后新增；本地存在则用最新详情覆盖更新（含恢复软删）。
    /// 随后同步关联 SysUser（按工号匹配账号），并重建 DingTalkRoleUser / SysUserRole 角色关系。
    /// </summary>
    private async Task UpsertUsersAsync(List<string> userIds)
    {
        var ids = Normalize(userIds);
        if (ids.Count == 0)
            return;

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return;

        var existing = await _dingTalkUserRep
            .AsQueryable()
            .Where(t => ids.Contains(t.DingTalkUserId))
            .ToListAsync();
        var existingMap = existing.ToDictionary(t => t.DingTalkUserId, t => t);

        var toInsert = new List<DingTalkUser>();
        var toUpdate = new List<DingTalkUser>();
        // 记录每个用户从 /v2/user/get 拉到的角色列表，用于重建 DingTalkRoleUser / SysUserRole
        var roleListMap = new Dictionary<string, List<DingTalkUserRoleItem>>();

        foreach (var id in ids)
        {
            var detail = await FetchUserDetailAsync(token, id);
            if (detail == null)
                continue;

            roleListMap[id] = detail.RoleList ?? new List<DingTalkUserRoleItem>();

            if (existingMap.TryGetValue(id, out var local))
            {
                local.IsDelete = false;
                ApplyUserDetail(local, detail);
                toUpdate.Add(local);
            }
            else
            {
                var user = new DingTalkUser { DingTalkUserId = id };
                ApplyUserDetail(user, detail);
                toInsert.Add(user);
            }
        }

        if (toInsert.Count > 0)
        {
            await _dingTalkUserRep.InsertRangeAsync(toInsert);
        }
        if (toUpdate.Count > 0)
        {
            await _dingTalkUserRep
                .AsUpdateable(toUpdate)
                .UpdateColumns(u => new
                {
                    u.UnionId,
                    u.Name,
                    u.Mobile,
                    u.JobNumber,
                    u.DeptId,
                    u.Dept,
                    u.Position,
                    u.Avatar,
                    u.IsDelete,
                    u.UpdateTime,
                })
                .ExecuteCommandAsync();
        }

        // 同步关联 SysUser 与角色关系
        var affected = toInsert.Concat(toUpdate).ToList();
        await LinkSysUserAsync(affected);
        await SyncUserRolesFromDetailAsync(affected, roleListMap);

        // 触发用户同步事件（区分新增/更新），供其他插件联动
        if (toInsert.Count > 0)
            await DingTalkContactSyncEvents.RaiseUserSyncedAsync(this, new DingTalkUserSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Added,
                Users = toInsert,
            });
        if (toUpdate.Count > 0)
            await DingTalkContactSyncEvents.RaiseUserSyncedAsync(this, new DingTalkUserSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Updated,
                Users = toUpdate,
            });
    }

    /// <summary>
    /// 拉取单个用户详情，失败返回 null。
    /// </summary>
    private async Task<DingTalkUserDetailOutput> FetchUserDetailAsync(string token, string userId)
    {
        var res = await _dingTalkApi.GetDingTalkUserDetail(
            token,
            new GetDingTalkUserDetailInput { userid = userId }
        );
        if (res.ErrCode != 0 || res.Result == null)
        {
            return null;
        }
        return res.Result;
    }

    /// <summary>
    /// 将钉钉用户详情映射到 DingTalkUser（主部门取部门列表首个）。
    /// </summary>
    private static void ApplyUserDetail(DingTalkUser user, DingTalkUserDetailOutput detail)
    {
        user.UnionId = detail.UnionId;
        user.Name = detail.Name;
        user.Mobile = detail.Mobile;
        user.JobNumber = detail.JobNumber;
        user.Position = detail.Title;
        user.Avatar = detail.Avatar;
        var mainDeptId = detail.DeptIdList?.FirstOrDefault() ?? 0;
        if (mainDeptId > 0)
        {
            user.DeptId = mainDeptId;
            user.Dept = mainDeptId.ToString();
        }
    }

    /// <summary>
    /// 按工号（JobNumber == SysUser.Account）关联系统用户，并回写 DingTalkUser.SysUserId、SysUser.OrgId。
    /// 工号在 SysUser 中无对应账号时，自动新建一个 SysUser（随机 8 位密码）后再关联。
    /// </summary>
    private async Task LinkSysUserAsync(List<DingTalkUser> users)
    {
        var jobNumbers = users
            .Where(u => !string.IsNullOrWhiteSpace(u.JobNumber))
            .Select(u => u.JobNumber)
            .Distinct()
            .ToList();
        if (jobNumbers.Count == 0)
            return;

        var sysUsers = await _sysUserRep
            .AsQueryable()
            .Where(u => jobNumbers.Contains(u.Account))
            .ToListAsync();
        var sysUserMap = sysUsers
            .Where(u => !string.IsNullOrWhiteSpace(u.Account))
            .GroupBy(u => u.Account)
            .ToDictionary(g => g.Key, g => g.First());

        // 工号在 SysUser 中缺失的钉钉用户 → 自动建号
        var missing = users
            .Where(u => !string.IsNullOrWhiteSpace(u.JobNumber) && !sysUserMap.ContainsKey(u.JobNumber))
            .GroupBy(u => u.JobNumber)
            .Select(g => g.First())
            .ToList();
        foreach (var created in await CreateSysUsersAsync(missing))
            sysUserMap[created.Account] = created;

        var sysUsersToUpdate = new List<SysUser>();
        var dingUsersToUpdate = new List<DingTalkUser>();

        foreach (var u in users)
        {
            if (string.IsNullOrWhiteSpace(u.JobNumber)
                || !sysUserMap.TryGetValue(u.JobNumber, out var sysUser))
                continue;

            if (u.SysUserId != sysUser.Id)
            {
                u.SysUserId = sysUser.Id;
                dingUsersToUpdate.Add(u);
            }

            if (u.DeptId is > 0 && sysUser.OrgId != u.DeptId.Value)
            {
                sysUser.OrgId = u.DeptId.Value;
                sysUsersToUpdate.Add(sysUser);
            }
        }

        if (dingUsersToUpdate.Count > 0)
        {
            await _dingTalkUserRep
                .AsUpdateable(dingUsersToUpdate)
                .UpdateColumns(u => new { u.SysUserId, u.UpdateTime })
                .ExecuteCommandAsync();
        }
        if (sysUsersToUpdate.Count > 0)
        {
            await _sysUserRep
                .AsUpdateable(sysUsersToUpdate)
                .UpdateColumns(u => new { u.OrgId })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 为工号在 SysUser 中缺失的钉钉用户批量新建系统账号（Account=工号，随机 8 位初始密码），返回新建的 SysUser。
    /// 密码按项目统一策略 CryptogramUtil.Encrypt 存储；OrgId 取钉钉主部门。
    /// </summary>
    private async Task<List<SysUser>> CreateSysUsersAsync(List<DingTalkUser> missing)
    {
        if (missing == null || missing.Count == 0)
            return new List<SysUser>();

        var toInsert = new List<SysUser>();
        foreach (var u in missing)
        {
            var plainPwd = GenerateRandomPassword(8);
            toInsert.Add(new SysUser
            {
                Account = u.JobNumber,
                Password = CryptogramUtil.Encrypt(plainPwd),
                RealName = u.Name,
                NickName = u.Name,
                Phone = u.Mobile,
                Avatar = u.Avatar,
                JobNum = u.JobNumber,
                OrgId = u.DeptId is > 0 ? u.DeptId.Value : 0,
                AccountType = AccountTypeEnum.NormalUser,
                Status = StatusEnum.Enable,
                Remark = "钉钉同步自动建号",
                TenantId = _dingTalkOptions.DefaultTenantId
            });
        }

        // 交由仓储 AOP 填充 Id/CreateTime 等
        await _sysUserRep.InsertRangeAsync(toInsert);
        return toInsert;
    }

    /// <summary>
    /// 生成指定长度的随机初始密码（含大小写字母与数字，避免易混淆字符）。
    /// </summary>
    private static string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length);
        var sb = new System.Text.StringBuilder(length);
        foreach (var b in bytes)
            sb.Append(chars[b % chars.Length]);
        return sb.ToString();
    }

    /// <summary>
    /// 依据 /v2/user/get 返回的 role_list 重建这批用户的角色关系（以接口结果为准）：
    /// - DingTalkRoleUser：接口有的角色→新增或恢复软删并刷新角色名/组名；接口没有的→软删；
    /// - SysUserRole：对已关联 SysUser 的用户，按上述角色集合增删，确保两表一致。
    /// </summary>
    private async Task SyncUserRolesFromDetailAsync(
        List<DingTalkUser> users,
        Dictionary<string, List<DingTalkUserRoleItem>> roleListMap
    )
    {
        var valid = users.Where(u => !string.IsNullOrWhiteSpace(u.DingTalkUserId)).ToList();
        if (valid.Count == 0)
            return;

        var dingUserIds = valid.Select(u => u.DingTalkUserId).Distinct().ToList();

        // 该批用户已存的钉钉角色关系（含软删，便于恢复）
        var existRel = await _dingTalkRoleUserRep
            .AsQueryable()
            .Where(t => dingUserIds.Contains(t.DingTalkUserId))
            .ToListAsync();
        var existRelMap = existRel
            .GroupBy(r => r.DingTalkUserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var relToInsert = new List<DingTalkRoleUser>();
        var relToUpdate = new List<DingTalkRoleUser>();

        foreach (var dingUserId in dingUserIds)
        {
            var wantRoles = roleListMap.TryGetValue(dingUserId, out var list)
                ? list ?? new List<DingTalkUserRoleItem>()
                : new List<DingTalkUserRoleItem>();
            var wantRoleIds = wantRoles.Select(r => r.Id).Where(id => id > 0).ToHashSet();
            existRelMap.TryGetValue(dingUserId, out var currentRels);
            currentRels ??= new List<DingTalkRoleUser>();
            var currentMap = currentRels.GroupBy(r => r.roleId).ToDictionary(g => g.Key, g => g.First());

            // 接口有的角色：新增 / 恢复软删 / 刷新名称
            foreach (var role in wantRoles.Where(r => r.Id > 0))
            {
                if (currentMap.TryGetValue(role.Id, out var rel))
                {
                    rel.IsDelete = false;
                    rel.roleName = role.Name;
                    rel.groupName = role.GroupName;
                    relToUpdate.Add(rel);
                }
                else
                {
                    relToInsert.Add(new DingTalkRoleUser
                    {
                        DingTalkUserId = dingUserId,
                        roleId = role.Id,
                        roleName = role.Name,
                        groupId = 0, // /v2/user/get 不返回 group_id
                        groupName = role.GroupName,
                    });
                }
            }

            // 接口没有、但本地未软删的角色：软删
            foreach (var rel in currentRels.Where(r => !r.IsDelete && !wantRoleIds.Contains(r.roleId)))
            {
                rel.IsDelete = true;
                relToUpdate.Add(rel);
            }
        }

        if (relToInsert.Count > 0)
            await _dingTalkRoleUserRep.InsertRangeAsync(relToInsert);
        if (relToUpdate.Count > 0)
        {
            await _dingTalkRoleUserRep
                .AsUpdateable(relToUpdate)
                .UpdateColumns(r => new { r.IsDelete, r.roleName, r.groupName, r.UpdateTime })
                .ExecuteCommandAsync();
        }

        // 同步 SysUserRole（仅处理已关联 SysUser 的用户）
        var linked = valid.Where(u => u.SysUserId > 0).ToList();
        if (linked.Count == 0)
            return;

        var sysUserIdMap = linked
            .GroupBy(u => u.DingTalkUserId)
            .ToDictionary(g => g.Key, g => g.First().SysUserId);

        var expected = new List<SysUserRole>();
        foreach (var dingUserId in sysUserIdMap.Keys)
        {
            if (!roleListMap.TryGetValue(dingUserId, out var list) || list == null)
                continue;
            var sysUserId = sysUserIdMap[dingUserId];
            foreach (var role in list.Where(r => r.Id > 0))
                expected.Add(new SysUserRole { UserId = sysUserId, RoleId = role.Id });
        }
        var comparer = new SysUserRoleComparer();
        expected = expected.Distinct(comparer).ToList();

        var sysUserIds = sysUserIdMap.Values.Distinct().ToList();
        var current = await _sysUserRoleRep
            .AsQueryable()
            .Where(t => sysUserIds.Contains(t.UserId))
            .ToListAsync();

        var toAdd = expected.Except(current, comparer).ToList();
        var toDel = current.Except(expected, comparer).ToList();

        if (toAdd.Count > 0)
            await _sysUserRoleRep.InsertRangeAsync(toAdd);
        if (toDel.Count > 0)
            await _sysUserRoleRep.DeleteAsync(toDel);

        // 触发用户角色关系变更事件，供其他插件联动
        if (toAdd.Count > 0 || toDel.Count > 0)
            await DingTalkContactSyncEvents.RaiseUserRoleChangedAsync(this, new DingTalkUserRoleChangedEventArgs
            {
                Added = toAdd,
                Removed = toDel,
            });
    }

    /// <summary>
    /// 用户离职：软删 DingTalkUser，并清理其角色关系（DingTalkRoleUser 软删、SysUserRole 物理删）。
    /// </summary>
    private async Task LeaveUsersAsync(List<string> userIds)
    {
        var ids = Normalize(userIds);
        if (ids.Count == 0)
            return;

        var users = await _dingTalkUserRep
            .AsQueryable()
            .Where(t => ids.Contains(t.DingTalkUserId))
            .ToListAsync();
        if (users.Count > 0)
        {
            foreach (var u in users)
                u.IsDelete = true;
            await _dingTalkUserRep
                .AsUpdateable(users)
                .UpdateColumns(u => new { u.IsDelete, u.UpdateTime })
                .ExecuteCommandAsync();

            // 清理系统用户角色关系（依据已关联的 SysUserId）
            var sysUserIds = users.Where(u => u.SysUserId > 0).Select(u => u.SysUserId).Distinct().ToList();
            if (sysUserIds.Count > 0)
                await _sysUserRoleRep.DeleteAsync(t => sysUserIds.Contains(t.UserId));

            // 触发用户离职（Removed）事件，供其他插件联动
            await DingTalkContactSyncEvents.RaiseUserSyncedAsync(this, new DingTalkUserSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Removed,
                Users = users,
            });
        }

        // 软删钉钉用户角色关系
        var roleUsers = await _dingTalkRoleUserRep
            .AsQueryable()
            .Where(t => ids.Contains(t.DingTalkUserId))
            .ToListAsync();
        if (roleUsers.Count > 0)
        {
            foreach (var r in roleUsers)
                r.IsDelete = true;
            await _dingTalkRoleUserRep
                .AsUpdateable(roleUsers)
                .UpdateColumns(r => new { r.IsDelete, r.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 员工角色信息变更（label_user_change）：事件后 /v2/user/get 已反映最新角色集合，
    /// 故直接复用 UpsertUsersAsync 按接口 role_list 重建 DingTalkRoleUser / SysUserRole，
    /// 无需再按事件里的 add/remove 单独 diff（避免与接口结果冲突）。
    /// </summary>
    private async Task ChangeUserRolesAsync(List<string> userIds, List<long> roleIds, string action)
    {
        var uids = Normalize(userIds);
        if (uids.Count == 0)
        {
            return;
        }

        await UpsertUsersAsync(uids);
    }

    #endregion 用户

    #region 部门

    /// <summary>
    /// 部门创建/修改：调用 /v2/department/get 拉取详情。
    /// SysOrg 已存在则更新（名称/编码/上级），不存在则新增。
    /// </summary>
    private async Task UpsertDeptsAsync(List<long> deptIds, string eventType)
    {
        var ids = deptIds?.Where(d => d > 0).Distinct().ToList() ?? new List<long>();
        if (ids.Count == 0)
            return;

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return;

        var existing = await _sysOrgRep.AsQueryable().Where(t => ids.Contains(t.Id)).ToListAsync();
        var existingMap = existing.ToDictionary(t => t.Id, t => t);

        var toInsert = new List<SysOrg>();
        var toUpdate = new List<SysOrg>();

        foreach (var deptId in ids)
        {
            var res = await _dingTalkApi.GetDingTalkDeptDetail(
                token,
                new GetDingTalkDeptInput { dept_id = deptId }
            );
            if (res.ErrCode != 0 || res.Result == null)
            {
                continue;
            }

            var d = res.Result;
            var pid = d.parent_id == 1 ? _dingTalkOptions.RootOrgPid : d.parent_id;
            if (existingMap.TryGetValue(deptId, out var local))
            {
                local.Name = d.name;
                local.Code = d.name;
                local.Pid = pid;
                toUpdate.Add(local);
            }
            else
            {
                toInsert.Add(new SysOrg
                {
                    Id = deptId,
                    Name = d.name,
                    Code = d.name,
                    Pid = pid,
                    TenantId = _dingTalkOptions.DefaultTenantId,
                    Remark = _dingTalkOptions.DingTalkInfoRemark,
                });
            }
        }

        if (toInsert.Count > 0)
        {
            await _sysOrgRep.InsertRangeAsync(toInsert);
        }
        if (toUpdate.Count > 0)
        {
            await _sysOrgRep
                .AsUpdateable(toUpdate)
                .UpdateColumns(o => new { o.Name, o.Code, o.Pid })
                .ExecuteCommandAsync();
        }

        // 触发部门同步事件（区分新增/更新），供其他插件联动
        if (toInsert.Count > 0)
            await DingTalkContactSyncEvents.RaiseDeptSyncedAsync(this, new DingTalkDeptSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Added,
                Orgs = toInsert,
            });
        if (toUpdate.Count > 0)
            await DingTalkContactSyncEvents.RaiseDeptSyncedAsync(this, new DingTalkDeptSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Updated,
                Orgs = toUpdate,
            });
    }

    /// <summary>
    /// 部门删除：仅删除由钉钉同步生成（Remark=钉钉同步）的部门，避免误删手工机构。
    /// </summary>
    private async Task RemoveDeptsAsync(List<long> deptIds)
    {
        var ids = deptIds?.Where(d => d > 0).Distinct().ToList() ?? new List<long>();
        if (ids.Count == 0)
            return;

        var toRemove = await _sysOrgRep
            .AsQueryable()
            .Where(t => ids.Contains(t.Id) && t.Remark == _dingTalkOptions.DingTalkInfoRemark)
            .ToListAsync();
        if (toRemove.Count > 0)
        {
            await _sysOrgRep.DeleteAsync(toRemove);

            // 触发部门删除（Removed）事件，供其他插件联动
            await DingTalkContactSyncEvents.RaiseDeptSyncedAsync(this, new DingTalkDeptSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Removed,
                Orgs = toRemove,
            });
        }
    }

    #endregion 部门

    #region 角色

    /// <summary>
    /// 角色/角色组增加/修改：调用 /role/getrole 拉取详情。
    /// SysRole 已存在则更新（名称/编码），不存在则新增。
    /// </summary>
    private async Task UpsertRolesAsync(List<long> labelIds, string eventType)
    {
        var ids = labelIds?.Where(d => d > 0).Distinct().ToList() ?? new List<long>();
        if (ids.Count == 0)
            return;

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return;

        var existing = await _sysRoleRep.AsQueryable().Where(t => ids.Contains(t.Id)).ToListAsync();
        var existingMap = existing.ToDictionary(t => t.Id, t => t);

        var toInsert = new List<SysRole>();
        var toUpdate = new List<SysRole>();

        foreach (var roleId in ids)
        {
            var res = await _dingTalkApi.GetDingTalkRoleDetail(
                token,
                new GetDingTalkRoleDetailInput { roleId = roleId }
            );
            // 注意：/role/getrole 的 role 在返回体顶层，成功语义以 ErrCode==0 为准
            if (res.ErrCode != 0 || res.role == null)
            {
                continue;
            }

            var role = res.role;
            if (existingMap.TryGetValue(roleId, out var local))
            {
                local.Name = role.name;
                local.Code = role.name;
                toUpdate.Add(local);
            }
            else
            {
                toInsert.Add(new SysRole
                {
                    Id = roleId,
                    Name = role.name,
                    Code = role.name,
                    OrderNo = 100,
                    DataScope = DataScopeEnum.DeptChild,
                    Remark = _dingTalkOptions.DingTalkInfoRemark,
                    TenantId = _dingTalkOptions.DefaultTenantId,
                });
            }
        }

        if (toInsert.Count > 0)
        {
            await _sysRoleRep.InsertRangeAsync(toInsert);
        }
        if (toUpdate.Count > 0)
        {
            await _sysRoleRep
                .AsUpdateable(toUpdate)
                .UpdateColumns(r => new { r.Name, r.Code })
                .ExecuteCommandAsync();
        }

        // 触发角色同步事件（区分新增/更新），供其他插件联动
        if (toInsert.Count > 0)
            await DingTalkContactSyncEvents.RaiseRoleSyncedAsync(this, new DingTalkRoleSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Added,
                Roles = toInsert,
            });
        if (toUpdate.Count > 0)
            await DingTalkContactSyncEvents.RaiseRoleSyncedAsync(this, new DingTalkRoleSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Updated,
                Roles = toUpdate,
            });
    }

    /// <summary>
    /// 角色/角色组删除：删除钉钉同步角色(Remark=钉钉同步角色)及其用户关系。
    /// </summary>
    private async Task RemoveRolesAsync(List<long> labelIds)
    {
        var ids = labelIds?.Where(d => d > 0).Distinct().ToList() ?? new List<long>();
        if (ids.Count == 0)
            return;

        var roles = await _sysRoleRep
            .AsQueryable()
            .Where(t => ids.Contains(t.Id) && t.Remark == _dingTalkOptions.DingTalkInfoRemark)
            .ToListAsync();
        if (roles.Count > 0)
        {
            var roleIds = roles.Select(r => r.Id).ToList();
            await _sysRoleRep.DeleteAsync(roles);
            // 物理清理系统用户-角色关系
            await _sysUserRoleRep.DeleteAsync(t => roleIds.Contains(t.RoleId));

            // 触发角色删除（Removed）事件，供其他插件联动
            await DingTalkContactSyncEvents.RaiseRoleSyncedAsync(this, new DingTalkRoleSyncedEventArgs
            {
                ChangeType = DingTalkSyncChangeType.Removed,
                Roles = roles,
            });
        }

        // 软删钉钉用户角色关系（按 roleId）
        var roleUsers = await _dingTalkRoleUserRep
            .AsQueryable()
            .Where(t => ids.Contains(t.roleId))
            .ToListAsync();
        if (roleUsers.Count > 0)
        {
            foreach (var r in roleUsers)
                r.IsDelete = true;
            await _dingTalkRoleUserRep
                .AsUpdateable(roleUsers)
                .UpdateColumns(r => new { r.IsDelete, r.UpdateTime })
                .ExecuteCommandAsync();
        }
    }

    #endregion 角色

    #region 辅助

    /// <summary>
    /// 获取企业内部应用 access_token，失败返回空。
    /// </summary>
    private async Task<string> GetTokenAsync()
    {
        var tokenRes = await _dingTalkApi.GetDingTalkToken(
            _dingTalkOptions.ClientId,
            _dingTalkOptions.ClientSecret
        );
        if (tokenRes.ErrCode != 0 || string.IsNullOrEmpty(tokenRes.AccessToken))
        {
            return null;
        }
        return tokenRes.AccessToken;
    }

    private static List<string> Normalize(List<string> ids) =>
        ids?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList() ?? new List<string>();

    /// <summary>
    /// 系统用户-角色关系去重比较器（UserId + RoleId）。
    /// </summary>
    private sealed class SysUserRoleComparer : IEqualityComparer<SysUserRole>
    {
        public bool Equals(SysUserRole x, SysUserRole y)
        {
            if (x == null || y == null)
                return x == y;
            return x.UserId == y.UserId && x.RoleId == y.RoleId;
        }

        public int GetHashCode(SysUserRole obj) =>
            obj == null ? 0 : HashCode.Combine(obj.UserId, obj.RoleId);
    }

    #endregion 辅助
}