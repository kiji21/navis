// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core;

/// <summary>
/// 异常安全序列化：System.Text.Json（JSON.Serialize 的底层实现）不支持 Exception.TargetSite 等
/// System.Reflection.MethodBase 成员，直接序列化原始 Exception 会抛 NotSupportedException，
/// 导致异常日志入库/写入失败。此处投影为可安全序列化的快照，再由调用方序列化。
/// </summary>
public static class ExceptionSanitizer
{
    /// <summary>
    /// 异常 → 可安全序列化的快照（type / message / source / stackTrace / innerException 递归）。
    /// 纯函数、零框架依赖（可在无宿主初始化的测试里直接断言结构）。
    /// 有意不含 TargetSite（STJ 无法序列化）、Data（含任意对象，可能触发同一问题）、HResult（噪音）。
    /// </summary>
    /// <param name="exception">待快照的异常（通常是被抛出后捕获的真实异常，TargetSite 等已填充）</param>
    /// <returns>匿名快照对象，字段名固定为 camelCase</returns>
    public static object Snapshot(Exception exception) => Build(exception);

    private static object Build(Exception exception) => new
    {
        type = exception.GetType().FullName,        // 异常类型全名
        message = exception.Message,                // 异常消息
        source = exception.Source,                  // 异常来源程序集
        stackTrace = exception.StackTrace,          // 堆栈
        innerException = exception.InnerException == null ? null : Build(exception.InnerException) // 内层异常递归
    };

    /// <summary>
    /// 序列化日志的异常字段：真实 Exception 先经 <see cref="Snapshot"/> 净化；其它对象
    /// （如监控 DTO 反序列化后的 JsonElement）原样序列化。
    /// 序列化失败兜底为纯文本（非 JSON —— Web 日志页仅做判空/高亮展示，不解析该字段），确保日志宁脏不丢。
    /// </summary>
    /// <param name="exception">原始 Exception，或已是反序列化结果的 JsonElement 等</param>
    /// <param name="serialize">宿主序列化入口（生产传 JSON.Serialize；测试可传 System.Text.Json，避免依赖框架静态初始化）</param>
    /// <returns>异常 JSON；入参为空返回 null</returns>
    public static string? ToJson(object? exception, Func<object, string> serialize)
    {
        if (exception == null) return null;

        var payload = exception is Exception ex ? Snapshot(ex) : exception;
        try
        {
            return serialize(payload);
        }
        catch
        {
            return exception is Exception raw ? raw.ToString() : payload.ToString(); // 兜底纯文本，日志不丢
        }
    }
}
