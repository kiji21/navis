// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 钉钉审批事件落库器 🧩
/// </summary>
/// <remarks>
/// 将审批实例变更事件（bpms_instance_change）持久化到 <see cref="DingTalkWokerflowLog"/>，
/// 供本地按 userId 查询“我发起的/我审批的/我收到的”。
/// 事件来源与传输方式（HTTP 推送 / Stream 长连接）无关，均复用本落库逻辑。
/// </remarks>
public class DingTalkWorkflowPersister : IScoped
{
    private readonly IDingTalkApi _dingTalkApi;
    private readonly DingTalkOptions _dingTalkOptions;
    private readonly SqlSugarRepository<DingTalkWokerflowLog> _dingTalkWokerflowLogRep;

    public DingTalkWorkflowPersister(
        IDingTalkApi dingTalkApi,
        IOptions<DingTalkOptions> dingTalkOptions,
        SqlSugarRepository<DingTalkWokerflowLog> dingTalkWokerflowLogRep
    )
    {
        _dingTalkApi = dingTalkApi;
        _dingTalkOptions = dingTalkOptions.Value;
        _dingTalkWokerflowLogRep = dingTalkWokerflowLogRep;
    }

    /// <summary>
    /// 将审批事件落库（upsert）。基础字段来自事件本身，审批人/抄送人列表调用实例详情接口补全。
    /// </summary>
    /// <param name="evt">审批实例变更事件</param>
    public async Task PersistAsync(DingTalkBpmsInstanceChangeEvent evt)
    {
        if (evt == null || string.IsNullOrEmpty(evt.ProcessInstanceId))
            return;

        var isFinish = string.Equals(evt.Type, "finish", StringComparison.OrdinalIgnoreCase);
        var status = isFinish ? "COMPLETED" : "RUNNING";

        var log =
            await _dingTalkWokerflowLogRep.GetFirstAsync(t => t.instanceId == evt.ProcessInstanceId)
            ?? new DingTalkWokerflowLog
            {
                instanceId = evt.ProcessInstanceId,
                CreateTime = DateTime.Now,
            };

        log.WorkflowName ??= evt.ProcessCode;
        log.Title = evt.Title;
        log.WorkflowId = evt.BusinessId;
        log.OriginatorUserId = evt.StaffId;
        log.Status = status;
        log.Result = evt.Result;
        log.UpdateTime = DateTime.Now;
        if (isFinish && evt.FinishTime.HasValue)
            log.EndTime = DateTimeOffset.FromUnixTimeMilliseconds(evt.FinishTime.Value).LocalDateTime;

        // 拉取实例详情，补全审批人/抄送人列表（供“我审批的/我收到的”归类）
        try
        {
            var token = (
                await _dingTalkApi.GetDingTalkToken(
                    _dingTalkOptions.ClientId,
                    _dingTalkOptions.ClientSecret
                )
            ).AccessToken;
            var detail = await _dingTalkApi.GetProcessInstances(token, evt.ProcessInstanceId);
            var data = detail?.Result;
            if (data != null)
            {
                log.Title ??= data.Title;
                log.OriginatorUserId ??= data.OriginatorUserId;
                log.Status = data.Status ?? log.Status;
                log.Result ??= data.Result;
                // 审批人：优先 approverUserIds，否则从任务节点提取
                log.ApproverUserIds =
                    data.ApproverUserIds is { Count: > 0 }
                        ? data.ApproverUserIds
                        : data
                            .Tasks?.Select(x => x.UserId)
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Distinct()
                            .ToList();
                log.CcUserIds = data.CcUserIds;
                if (!string.IsNullOrEmpty(data.BusinessId))
                    log.WorkflowId = data.BusinessId;
            }
        }
        catch (Exception)
        {
            // 详情补全失败不阻断落库，基础字段已能支撑“我发起的”
        }

        // 与业务发起端（CreatWorkflowProcessInstances）共用主键 instanceId。
        // Stream 只负责钉钉侧字段，绝不覆盖业务侧独有列（SourceDocument/CreateUserName/WorkflowName）。
        // 用 Storageable 按主键合并：不存在则整条插入，存在则仅更新 Stream 维护的列。
        var storage = await _dingTalkWokerflowLogRep
            .Context.Storageable(log)
            .WhereColumns(t => t.instanceId)
            .ToStorageAsync();
        // 不存在：整条插入（WorkflowName 用 ProcessCode 占位，发起端会用业务名修正）
        await storage.AsInsertable.ExecuteCommandAsync();
        // 已存在：仅更新钉钉侧字段，业务列（SourceDocument/CreateUserName/WorkflowName）保持不动
        await storage
            .AsUpdateable.UpdateColumns(t => new
            {
                t.Title,
                t.WorkflowId,
                t.OriginatorUserId,
                t.ApproverUserIds,
                t.CcUserIds,
                t.Status,
                t.Result,
                t.EndTime,
                t.UpdateTime,
            })
            .ExecuteCommandAsync();
    }
}
