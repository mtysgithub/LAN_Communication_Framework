using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hik.Communication.Scs.Communication.EndPoints.Tcp;

using Middleware.Communication;
using Middleware.Communication.CommunicationConfig;
using Middleware.Device;
using Middleware.Communication.Package;
using Middleware.Communication.Tcp;
using Middleware.Communication.EndPoint.Tcp;
using Middleware.Communication.Message;

namespace Middleware.Interface.Ex
{
    

    public interface IExMiddlewareDevice
    {
        /// <summary>
        /// Middleware
        /// remark- virtual
        /// </summary>
        /// <param name="endPoint">ip + 端口 节点类</param>
        /// <param name="detail">设备名称</param>
        /// <param name="oprRules">设备允许向外发起的远程请求列表</param>
        /// <param name="opredRules">设备自身支持的远程操作请求</param>
        //void Initialization(MiddlewareTcpEndPoint endPoint, string detail, List<string> oprRules, List<string> opredRules);

        /// <summary>
        /// Middleware
        /// remark- virtual
        /// </summary>
        /// <param name="endPoint">ip + 端口 节点类</param>
        /// <param name="detail">设备名称</param>
        /// <param name="token">网络会话Token</param>
        /// <param name="oprRules">设备允许向外发起的远程请求列表</param>
        /// <param name="opredRules">设备自身支持的远程操作请求</param>
        //void Initialization(MiddlewareTcpEndPoint endPoint, string detail, string token, List<string> oprRules, List<string> opredRules);

        /// <summary>
        /// 停止中间件工作, 进入网络注销流程
        /// remark- virtual
        /// </summary>
        void Dispose();

        /// <summary>
        /// 创建一个请求通讯包
        /// </summary>
        /// <param name="communicateName">请求的名称</param>
        /// <param name="communicateType">请求执行的网络类型</param>
        /// <param name="reqtParamPkg">携带的参数属性包</param>
        /// <param name="targetDevice">目的设备</param>
        /// <param name="waitResponse">是否等待应答</param>
        /// <param name="callback">在异步情形下，收到应答后的通知事件</param>
        /// <returns></returns>
        RequestCommunicatePackage CreateRequestCommunicatePackage(string communicateName,
													CommunicatType communicateType,
													ParamPackage reqtParamPkg,
													ClientDevice targetDevice,
													bool waitResponse,
													AsynReponseHandler callback);

        /// <summary>
        /// 非阻塞式发起一次远程请求
        /// </summary>
        /// <param name="communicator">请求通讯包</param>
        void AsynSendRequest(RequestCommunicatePackage communicator);

        /// <summary>
        /// 阻塞式发起一次远程请求
        /// </summary>
        /// <param name="communicator">请求通讯包</param>
        /// <param name="timeMillionsecond">等待的上限时长</param>
        void SynSendRequest(RequestCommunicatePackage communicator, int timeMillionsecond);

        /// <summary>
        /// 创建一个应答通讯包
        /// </summary>
        /// <param name="reqtPkg">匹配的远程请求</param>
        /// <param name="statCodec">远程请求的执行结果反馈码</param>
        /// <param name="paramspkg">携带的参数属性包</param>
        /// <returns>应答通讯包</returns>
        ReplyCommunicatePackage CreateReplyCommunicatePackage(RequestPackage reqtPkg, ReplyPackage.Middleware_ReplyInfo statCodec, ParamPackage paramspkg);

        /// <summary>
        /// 反馈一个应答通讯包, 非阻塞式
        /// </summary>
        /// <param name="communicator">应答通讯包</param>
        void AsynFeedbackCommunicateReplyMessage(ReplyCommunicatePackage communicator);

        /// <summary>
        /// 反馈一个应答通讯包, 阻塞式
        /// </summary>
        /// <param name="communicator">应答通讯包</param>
        /// <param name="timeMillionsecond">等待的上限时长</param>
        //void SynFeedbackCommunicateReplyMessage(ReplyCommunicatePackage communicator, int timeMillionsecond);

        /// <summary>
        /// 创建一个群组
        /// </summary>
        /// <param name="detail">群组的自然语言描述，（UTF-8编码）</param>
        /// <param name="communicateType">发起该请求的网络形式</param>
        /// <returns>立即返回一个GroupDevice（可能未就绪）</returns>
        GroupDevice CreateGroup(string detail, CommunicatType communicateType);

        /// <summary>
        /// 获取一个指定描述名称的群组
        /// </summary>
        /// <param name="detail">群组的自然语言描述，（UTF-8编码）</param>
        /// <param name="communicateType">发起该请求的网络类型</param>
        /// <returns>立即返回一个GroupDevice（可能未就绪）</returns>
        GroupDevice GetGroup(string detail, CommunicatType communicateType);

        /// <summary>
        /// 加入一个群组
        /// </summary>
        /// <param name="group">指定的群组</param>
        /// <param name="role">设备在群组中的角色</param>
        /// <param name="communicateType">通讯的网络形式</param>
        void JoinGroup(GroupDevice group, GroupMemberRole role, CommunicatType communicateType);

        /// <summary>
        /// 退出一个群组
        /// </summary>
        /// <param name="group">指定的群组</param>
        /// <param name="communicateType">通讯的网络形式</param>
        void ExitGroup(GroupDevice group, CommunicatType communicateType);

        /// <summary>
        /// 创建一个群组通讯包
        /// </summary>
        /// <param name="radioName">广播通讯的名称</param>
        /// <param name="radioParamPkg">携带的参数属性包</param>
        /// <param name="targetGroup">目的群组</param>
        /// <returns>返回创建完毕的通讯包</returns>
        GroupComunicatePackage CreateRadioCommunicatePackage(string radioName,
																ParamPackage radioParamPkg,
																GroupDevice targetGroup);

        /// <summary>
        /// 发起一次广播通讯
        /// </summary>
        /// <param name="communicator">广播通讯包</param>
        void Radio(GroupComunicatePackage communicator);

        ///// <summary>
        ///// 广播操作触发的错误事件
        ///// </summary>
        //event RadioErrorHandler RadioErrorRecived;

        /// <summary>
        /// 非阻塞远程通讯触发的错误事件 
        /// </summary>
        event AsynCommunicatErrorHandler AsynCommunicatErrorRecived;

        /// <summary>
        /// 收到一个远程请求
        /// </summary>
        event RemotReqtRecivedHandler RemotReqtRecived;
		
		/// <summary>
		/// 收到一个远程广播通知
		/// </summary>
		event RemotRadioRecivedHandler RemotRadioRecived;

        /// <summary>
        /// 监听一种设备消息
        /// </summary>
        /// <param name="messenger">目标设备</param>
        /// <param name="typMsg">消息定义</param>
        void Listen(ClientDevice messenger, AbstractMessageType typMsg);

        /// <summary>
        /// 注册本设备支持的消息
        /// </summary>
        /// <param name="typMsg">消息定义</param>
        void RegistMessage(AbstractMessageType typMsg, Type t_Msg);

        /// <summary>
        /// 创建一条消息
        /// </summary>
        /// <param name="typMsg">消息定义</param>
        /// <returns></returns>
        AbstractMessage CreateMessage(AbstractMessageType typMsg);

        /// <summary>
        /// 发送一条消息
        /// </summary>
        /// <param name="msg">消息包</param>
        void SendMessage(AbstractMessage msg);

        /// <summary>
        /// 事件接收回调
        /// </summary>
        event MessageRecivedHandler MessageRecived;

        /// <summary>
        /// 设备会话Token
        /// </summary>
        string Token
        {
            get;
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        string Detail
        {
            get;
        }

        /// <summary>
        /// 在线状态
        /// </summary>
        bool Online
        {
            get;
        }
    }
}
