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

public class DingTalkAddPreentryInput
{
    /// <summary>
    /// 添加待入职人员
    /// </summary>
    public PreEntryEmployeeAddParam param { get; set; }
}
public class PreEntryEmployeeAddParam 
{
    /// <summary>
    /// 预期入职时间。
    /// </summary>
    public DateTime pre_entry_time { get; set; }

    /// <summary>
    /// 待入职员工姓名。
    /// </summary>
    public string name { get; set; }


    public Extend_Info extend_info { get; set; } = new Extend_Info();

}


public class Extend_Info 
{
    /// <summary>
    /// 部门ID列表，多个部门用"|"分隔
    /// </summary>
    public string depts { get; set; }

    /// <summary>
    /// 员工类型枚举值：0：无类型 1：全职 2：兼职 3：实习 4：劳务派遣 5：退休返聘 6：劳务外包
    /// </summary>
    public string employeeType { get; set; }

    /// <summary>
    /// 主部门ID
    /// </summary>
    public string mainDeptId { get; set; }

    /// <summary>
    /// 主部门名称
    /// </summary>
    public string mainDeptName { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    public string position { get; set; }

    /// <summary>
    /// 工作地点
    /// </summary>
    public string workPlace { get; set; }

}
