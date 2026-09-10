// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.NET.Plugin.DingTalk.Service.Dto;

public class DingTalkpendingDismissionInput
{
    /// <summary>
    /// 离职人userId。
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 最后工作日。
    /// </summary>
    public long lastWorkDate { get; set; }

    /// <summary>
    /// 主动离职原因id，可调用获取企业已有的所有离职原因接口获取。
    /// </summary>
    public List<string> terminationReasonVoluntary { get; set; }

    /// <summary>
    /// 被动离职原因id，可调用获取企业已有的所有离职原因接口获取。
    /// </summary>
    public List<string> terminationReasonPassive { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 是否加入HRM统计。
    /// </summary>
    public string partner { get; set; }

    /// <summary>
    /// 是否加入招聘黑名单。
    /// </summary>
    public string toHireBlackList {get; set; }

    /// <summary>
    /// 是否加入招聘人才库。
    /// </summary>
    public string toHireDismissionTalent { get; set; }

    /// <summary>
    /// 是否加入人事黑名单。
    /// </summary>
    public string toHrmBlackList { get; set; }

}
