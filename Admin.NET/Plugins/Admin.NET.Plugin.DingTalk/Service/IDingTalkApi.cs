// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Plugin.DingTalk.Service.Dto;

namespace Admin.NET.Plugin.DingTalk;

public interface IDingTalkApi : IHttpDeclarative
{
    /// <summary>
    /// 获取企业内部应用的access_token
    /// </summary>
    /// <param name="appkey">应用的唯一标识key</param>
    /// <param name="appsecret"> 应用的密钥。AppKey和AppSecret可在钉钉开发者后台的应用详情页面获取。</param>
    /// <returns></returns>
    [Get("https://oapi.dingtalk.com/gettoken")]
    Task<GetDingTalkTokenOutput> GetDingTalkToken([QueryParam] string appkey, [QueryParam] string appsecret);

    /// <summary>
    /// 获取在职员工列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/smartwork/hrm/employee/queryonjob")]
    Task<
        DingTalkBaseResponse<GetDingTalkCurrentEmployeesListOutput>
    > GetDingTalkCurrentEmployeesList(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentEmployeesListInput input
    );

    /// <summary>
    /// 获取员工花名册字段信息
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/smartwork/hrm/employee/v2/list")]
    Task<
        DingTalkBaseResponse<List<DingTalkEmpRosterFieldVo>>
    > GetDingTalkCurrentEmployeesRosterList(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentEmployeesRosterListInput input
    );

    /// <summary>
    /// 发送钉钉互动卡片
    /// </summary>
    /// <param name="token">调用该接口的访问凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <remarks>
    /// 钉钉官方文档显示接口不再支持新应用接入, 已接入的应用可继续调用
    /// 推荐更新接口https://open.dingtalk.com/document/orgapp/create-and-deliver-cards?spm=ding_open_doc.document.0.0.67fc50988Pf0mc
    /// </remarks>
    [Post("https://api.dingtalk.com/v1.0/im/interactiveCards/send")]
    [Obsolete]
    Task<DingTalkSendInteractiveCardsOutput> DingTalkSendInteractiveCards(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true)]
            DingTalkSendInteractiveCardsInput input
    );

    /// <summary>
    /// 获取钉钉卡片消息读取状态
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Get("https://api.dingtalk.com/v1.0/robot/oToMessages/readStatus")]
    Task<GetDingTalkCardMessageReadStatusOutput> GetDingTalkCardMessageReadStatus(
        [Header("x-acs-dingtalk-access-token")] string token,
        [QueryParam] GetDingTalkCardMessageReadStatusInput input
    );

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/role/list")]
    Task<DingTalkBaseResponse<DingTalkRoleListOutput>> GetDingTalkRoleList(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentRoleListInput input
    );

    /// <summary>
    /// 获取指定角色的员工列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/role/simplelist")]
    Task<DingTalkBaseResponse<DingTalkRoleSimplelistOutput>> GetDingTalkRoleSimplelist(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentRoleSimplelistInput input
    );

    /// <summary>
    /// 创建并投放钉钉消息卡片
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/card/instances/createAndDeliver")]
    Task<DingTalkCreateAndDeliverOutput> DingTalkCreateAndDeliver(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true)]
            DingTalkCreateAndDeliverInput input
    );

    /// <summary>
    /// 获取部门列表列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/v2/department/listsub")]
    Task<DingTalkBaseResponse<List<DingTalkDeptOutput>>> GetDingTalkDept(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkDeptInput input
    );

    /// <summary>
    /// 获取指定部门详情（按 dept_id 查询单个部门，含名称、上级部门）
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/v2/department/get")]
    Task<DingTalkBaseResponse<DingTalkDeptOutput>> GetDingTalkDeptDetail(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkDeptInput input
    );

    /// <summary>
    /// 获取角色详情（按 roleId 查询单个角色，含角色名、所属角色组）
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <remarks>返回体的 role 位于顶层（与 errcode 平级），故用专用响应类型 DingTalkRoleGetResponse。</remarks>
    [Post("https://oapi.dingtalk.com/topapi/role/getrole")]
    Task<DingTalkRoleGetResponse> GetDingTalkRoleDetail(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkRoleDetailInput input
    );

    /// <summary>
    /// 获取用户详情（按 userid 查询单个用户，含姓名/工号/手机/职位/部门列表）
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/v2/user/get")]
    Task<DingTalkBaseResponse<DingTalkUserDetailOutput>> GetDingTalkUserDetail(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkUserDetailInput input
    );

    /// <summary>
    /// 发起审批实例
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processInstances")]
    Task<DingTalkWorkflowProcessInstancesOutput> DingTalkWorkflowProcessInstances(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkWorkflowProcessInstancesInput input
    );

    /// <summary>
    /// 查询审批实例
    /// </summary>
    /// <param name="token"></param>
    /// <param name="processInstanceId"></param>
    /// <returns></returns>
    [Get("https://api.dingtalk.com/v1.0/workflow/processInstances")]
    Task<DingTalkGetProcessInstancesOutput> GetProcessInstances(
        [Header("x-acs-dingtalk-access-token")] string token,
        [QueryParam] string processInstanceId
    );


    /// <summary>
    /// 添加企业待入职员工
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/smartwork/hrm/employee/addpreentry")]
    Task<DingTalkAddPreentryOutput> DingTalkAddPreentry(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkAddPreentryInput input
    );


    /// <summary>
    /// 添加企业加入待离职
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/smartwork//hrm/pendingDismission/start")]
    Task<DingTalkpendingDismissionOutput> DingTalkpendingDismission(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkpendingDismissionInput input
    );

    /// <summary>
    /// 通过免登码获取用户信息
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input">包含前端传来的免登授权码 code</param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/v2/user/getuserinfo")]
    Task<DingTalkBaseResponse<GetDingTalkUserInfoOutput>> GetDingTalkUserInfo(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkUserInfoInput input
    );

    /// <summary>
    /// 查询“我审批的”待处理审批任务列表（OA高级版专享）
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processCentres/tasks/todo/query")]
    Task<DingTalkProcessCentreTaskOutput> GetProcessCentreTodoTasks(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkProcessCentreQueryInput input
    );

    /// <summary>
    /// 查询“我审批的”已处理审批任务列表（OA高级版专享）
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processCentres/tasks/done/query")]
    Task<DingTalkProcessCentreTaskOutput> GetProcessCentreDoneTasks(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkProcessCentreQueryInput input
    );

    /// <summary>
    /// 查询“我发起的”审批实例列表（OA高级版专享）
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processCentres/instances/submitted/query")]
    Task<DingTalkProcessCentreInstanceOutput> GetProcessCentreSubmittedInstances(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkProcessCentreQueryInput input
    );

    /// <summary>
    /// 查询“我收到的”审批实例列表（抄送我的，OA高级版专享）
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processCentres/instances/noticed/query")]
    Task<DingTalkProcessCentreInstanceOutput> GetProcessCentreNoticedInstances(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkProcessCentreQueryInput input
    );

    /// <summary>
    /// 获取审批实例ID列表（标准版，可按发起人过滤）
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processes/instanceIds/query")]
    Task<DingTalkListInstanceIdsOutput> ListProcessInstanceIds(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkListInstanceIdsInput input
    );

    /// <summary>
    /// 建立 Stream 长连接，获取 WebSocket 接入端点与建连票据
    /// </summary>
    /// <remarks>
    /// 使用 clientId/clientSecret 直接换取网关接入地址，无需 access_token。
    /// 返回的 endpoint 需拼接 ticket 作为 query 参数后再用 WebSocket 建连。
    /// </remarks>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/gateway/connections/open")]
    Task<DingTalkStreamOpenOutput> OpenStreamConnection(
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkStreamOpenInput input
    );
}