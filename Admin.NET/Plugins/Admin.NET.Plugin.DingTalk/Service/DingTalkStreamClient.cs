// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Net.WebSockets;
using System.Text;
using Furion.JsonSerialization;
using Furion.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 钉钉 Stream 模式长连接客户端（后台常驻服务）🧩
/// </summary>
/// <remarks>
/// 以 Stream 模式接收钉钉事件订阅：应用主动向钉钉网关建立 WebSocket 长连接，
/// 网关经该连接下推事件（EVENT）与系统指令（SYSTEM）。相比 HTTP 推送，无需公网地址/域名/验签解密。
/// 流程：调用 gateway/connections/open 获取 endpoint+ticket → 建连 → 收帧处理并回 ACK → 断线自动重连。
/// 事件落库复用 <see cref="DingTalkWorkflowPersister"/>。
/// </remarks>
public class DingTalkStreamClient : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DingTalkOptions _dingTalkOptions;

    public DingTalkStreamClient(
        IServiceScopeFactory scopeFactory,
        IOptions<DingTalkOptions> dingTalkOptions
    )
    {
        _scopeFactory = scopeFactory;
        _dingTalkOptions = dingTalkOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_dingTalkOptions.StreamEnabled)
        {
            return;
        }

        if (string.IsNullOrEmpty(_dingTalkOptions.ClientId)
            || string.IsNullOrEmpty(_dingTalkOptions.ClientSecret))
        {
            return;
        }

        var attempt = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndReceiveAsync(stoppingToken);
                attempt = 0; // 正常返回（如收到 disconnect）后立即重连
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // 应用关闭
            }
            catch (Exception)
            {
                // 连接异常，进入下方退避重连
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            // 指数退避重连，上限 60s
            attempt++;
            var delay = Math.Min(_dingTalkOptions.ReconnectBaseSeconds * attempt, 60);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// 建立一次连接并持续接收，直到断开/取消/收到 disconnect
    /// </summary>
    private async Task ConnectAndReceiveAsync(CancellationToken stoppingToken)
    {
        // 1. 获取网关接入端点与票据（IDingTalkApi 为 Scoped/Transient，后台服务需在 scope 内解析）
        DingTalkStreamOpenOutput open;
        using (var scope = _scopeFactory.CreateScope())
        {
            var api = scope.ServiceProvider.GetRequiredService<IDingTalkApi>();
            open = await api.OpenStreamConnection(
                new DingTalkStreamOpenInput
                {
                    ClientId = _dingTalkOptions.ClientId,
                    ClientSecret = _dingTalkOptions.ClientSecret,
                    Ua = "admin.net-dingtalk-plugin/1.0",
                    Subscriptions = new List<DingTalkStreamSubscription>
                    {
                        // 订阅全部事件（含 bpms_instance_change）
                        new() { Type = _dingTalkOptions.StreamTypeEvent, Topic = "*" },
                    },
                }
            );
        }

        if (open == null || string.IsNullOrEmpty(open.Endpoint) || string.IsNullOrEmpty(open.Ticket))
            throw new InvalidOperationException("获取 Stream 接入端点失败（endpoint/ticket 为空）");

        var uri = new Uri($"{open.Endpoint}?ticket={Uri.EscapeDataString(open.Ticket)}");

        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(uri, stoppingToken);

        await ReceiveLoopAsync(ws, stoppingToken);
    }

    /// <summary>
    /// 接收循环：拼接分片消息 → 解析帧 → 分发处理 → 回 ACK
    /// </summary>
    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken stoppingToken)
    {
        var buffer = new byte[8 * 1024];
        while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
        {
            using var msgStream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }
                msgStream.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            var text = Encoding.UTF8.GetString(msgStream.ToArray());
            if (string.IsNullOrWhiteSpace(text))
                continue;

            DebugLog($"收到原始帧：{text}");
            await HandleFrameAsync(ws, text, stoppingToken);
        }
    }

    /// <summary>
    /// 处理单帧消息
    /// </summary>
    private async Task HandleFrameAsync(ClientWebSocket ws, string text, CancellationToken stoppingToken)
    {
        DingTalkStreamFrame frame;
        try
        {
            frame = JSON.Deserialize<DingTalkStreamFrame>(text);
        }
        catch (Exception)
        {
            return;
        }

        var type = frame?.Type;
        var topic = frame?.Headers?.Topic;

        DebugLog(
            $"解析帧 → type={type}，topic={topic}，eventType={frame?.Headers?.EventType}，"
                + $"eventId={frame?.Headers?.EventId}，messageId={frame?.Headers?.MessageId}"
        );

        // 系统指令：ping 需回 pong；disconnect 需断开重连
        if (string.Equals(type, _dingTalkOptions.StreamTypeSystem, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(topic, "ping", StringComparison.OrdinalIgnoreCase))
            {
                await SendAckAsync(ws, frame, frame.Data, stoppingToken);
                return;
            }
            if (string.Equals(topic, "disconnect", StringComparison.OrdinalIgnoreCase))
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "reconnect", stoppingToken);
                return;
            }
            // 其他系统指令：确认即可
            await SendAckAsync(ws, frame, "{\"status\":\"SUCCESS\"}", stoppingToken);
            return;
        }

        // 业务事件
        if (string.Equals(type, _dingTalkOptions.StreamTypeEvent, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await HandleEventAsync(frame);
            }
            catch (Exception)
            {
                // 处理失败也回成功 ACK，避免网关重推与限流；失败仅忽略后续补偿
            }
            await SendAckAsync(ws, frame, "{\"status\":\"SUCCESS\",\"message\":\"OK\"}", stoppingToken);
            return;
        }

        // 未知类型：回成功确认
        await SendAckAsync(ws, frame, "{\"status\":\"SUCCESS\"}", stoppingToken);
    }

    /// <summary>
    /// 处理业务事件：按 eventType 分发到对应落库器（审批实例 / 通讯录·部门·角色）
    /// </summary>
    private async Task HandleEventAsync(DingTalkStreamFrame frame)
    {
        var eventType = frame?.Headers?.EventType;
        if (string.IsNullOrEmpty(frame?.Data))
            return;

        // 后台服务需自建 scope 使用 Scoped 依赖（含数据库仓储）
        using var scope = _scopeFactory.CreateScope();

        // 审批实例变更
        if (string.Equals(eventType, DingTalkConst.Events.BpmsInstanceChange, StringComparison.OrdinalIgnoreCase))
        {
            var evt = JSON.Deserialize<DingTalkBpmsInstanceChangeEvent>(frame.Data);
            if (evt == null || string.IsNullOrEmpty(evt.ProcessInstanceId))
            {
                DebugLog($"审批事件缺少 processInstanceId，跳过落库。data={frame.Data}");
                return;
            }

            DebugLog(
                $"审批事件落库 → instanceId={evt.ProcessInstanceId}，type={evt.Type}，"
                    + $"processCode={evt.ProcessCode}，staffId={evt.StaffId}，result={evt.Result}"
            );

            var persister = scope.ServiceProvider.GetRequiredService<DingTalkWorkflowPersister>();
            await persister.PersistAsync(evt);
            return;
        }

        // 通讯录 / 部门 / 角色 变更
        var contactEvt = JSON.Deserialize<DingTalkContactEvent>(frame.Data);
        var contactPersister = scope.ServiceProvider.GetRequiredService<DingTalkContactPersister>();
        var handled = await contactPersister.HandleAsync(eventType, contactEvt);
        if (handled)
        {
            DebugLog(
                $"通讯录/部门/角色事件落库 → eventType={eventType}，userId={FmtList(contactEvt?.UserId)}，"
                    + $"deptId={FmtList(contactEvt?.DeptId)}，labelIds={FmtList(contactEvt?.LabelIdList)}"
            );
        }
        else
        {
            DebugLog($"忽略未订阅处理的事件：eventType={eventType}");
        }
    }

    private static string FmtList<T>(IEnumerable<T> items) =>
        items == null ? "" : string.Join(",", items);

    /// <summary>
    /// 回 ACK 响应帧。messageId 需回填请求帧的 messageId，网关据此确认收到。
    /// </summary>
    private async Task SendAckAsync(
        ClientWebSocket ws,
        DingTalkStreamFrame req,
        string dataJson,
        CancellationToken stoppingToken
    )
    {
        if (ws.State != WebSocketState.Open)
            return;

        var ack = new DingTalkStreamAck
        {
            Code = 200,
            Message = "OK",
            Data = dataJson,
            Headers = new DingTalkStreamAckHeaders
            {
                ContentType = "application/json",
                MessageId = req?.Headers?.MessageId,
            },
        };

        var payload = Encoding.UTF8.GetBytes(JSON.Serialize(ack));
        DebugLog($"回 ACK → messageId={ack.Headers.MessageId}，data={ack.Data}");
        await ws.SendAsync(
            new ArraySegment<byte>(payload),
            WebSocketMessageType.Text,
            true,
            stoppingToken
        );
    }

    /// <summary>
    /// 按开关（StreamDebugLog）输出 Stream 收发帧的调试日志。默认关闭以免刷屏。
    /// </summary>
    private void DebugLog(string message)
    {
        if (_dingTalkOptions.StreamDebugLog)
            Log.Information($"[钉钉Stream调试] {message}");
    }
}