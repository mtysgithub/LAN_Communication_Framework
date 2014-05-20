using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using Hik.Communication.Scs;
using Hik.Communication;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Threading;

using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;

using Middleware.Communication.CommunicationConfig;
using Middleware.Interface.Ex;
using Middleware.Communication;
using Middleware.Interface;
using Middleware.Communication.Package;
using Middleware.Device;
using Middleware.Interface.ServerProcotolOperator;
using Middleware.Interface.ServerProcotolOperator.Request;
using Middleware.Interface.ServerProcotolOperator.Reply;
using Middleware.LayerProcessor;
using Middleware.Communication.EndPoint.Tcp;
using Middleware.Communication.Excetion;
using Middleware.Communication.Message;

namespace Middleware.Device
{
    public class MiddlewareDevice : MiddlewareCorelogicLayer, IExMiddlewareDevice
    {
        public MiddlewareDevice()
            : base()
        {

        }

        /// <summary>
        /// Middleware
        /// remark- virtual
        /// </summary>
        /// <param name="endPoint">ip + 端口 节点类</param>
        /// <param name="detail">设备名称</param>
        /// <param name="oprRules">设备允许向外发起的远程请求列表</param>
        /// <param name="opredRules">设备自身支持的远程操作请求</param>
        public virtual void Initialization(MiddlewareTcpEndPoint endPoint, string detail, List<string> oprRules, List<string> opredRules)
        {
            base.CoNewClientInitialization(endPoint, detail, oprRules, opredRules);

            base.CoRemotReqtRecived_OutsideNotify += this.__OutsideRemotReqtRecived;

            base.CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify += this.__AsynCommunicatErrorRecived;
            base.CoMiddleware2MiddlewareAsynReplyCommunicatErrorRecived_OutsideNotify += this.__AsynCommunicatErrorRecived;

            base.CoRemotRadioRecived_OutsideNotify += this.__OutsideRadioMessageRecived;
            base.CoRadioErrorRecived_OutsideNotify += this.__AsynCommunicatErrorRecived;

            base.CoStart();
        }

        /// <summary>
        /// Middleware
        /// remark- virtual
        /// </summary>
        /// <param name="endPoint">ip + 端口 节点类</param>
        /// <param name="detail">设备名称</param>
        /// <param name="token">网络会话Token</param>
        /// <param name="oprRules">设备允许向外发起的远程请求列表</param>
        /// <param name="opredRules">设备自身支持的远程操作请求</param>
        public virtual void Initialization(MiddlewareTcpEndPoint endPoint, string detail, string token, List<string> oprRules, List<string> opredRules)
        {
            base.CoOldClientInitialization(endPoint, detail, token, oprRules, opredRules);

            base.CoRemotReqtRecived_OutsideNotify += this.__OutsideRemotReqtRecived;

            base.CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify += this.__AsynCommunicatErrorRecived;
            base.CoMiddleware2MiddlewareAsynReplyCommunicatErrorRecived_OutsideNotify += this.__AsynCommunicatErrorRecived;

            base.CoRemotRadioRecived_OutsideNotify += this.__OutsideRadioMessageRecived;
            base.CoRadioErrorRecived_OutsideNotify += this.__AsynCommunicatErrorRecived;

            base.CoStart();
        }

        #region IExMiddlewareDevice

        /// <summary>
        /// 停止中间件工作
        /// remark- virtual
        /// </summary>
        public override void Dispose()
        {
            base.CoStop();
            base.Dispose();
        }

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
        public RequestCommunicatePackage CreateRequestCommunicatePackage(string communicationName,
                                                                        CommunicatType communicateType,
                                                                        ParamPackage reqtParamPkg,
                                                                        ClientDevice targetDevice,
                                                                        bool waitResponse,
                                                                        AsynReponseHandler callback)
        {
            return base.CoCreateRequestCommunicatePackage(communicationName,
                                                                                    communicateType,
                                                                                    reqtParamPkg,
                                                                                    targetDevice,
                                                                                    waitResponse,
                                                                                    callback);
        }

        /// <summary>
        /// 非阻塞式发起一次远程请求
        /// </summary>
        /// <param name="communicator">请求通讯包</param>
        public void AsynSendRequest(RequestCommunicatePackage communicator)
        {
            base.CoAsynSendRequest(communicator);
        }

        /// <summary>
        /// 阻塞式发起一次远程请求
        /// </summary>
        /// <param name="communicator">请求通讯包</param>
        /// <param name="timeMillionsecond">等待的上限时长</param>
        public void SynSendRequest(RequestCommunicatePackage communicator, int timeMillionsecond)
        {
            base.CoSynSendRequest(communicator, timeMillionsecond);
        }

        /// <summary>
        /// 创建一个应答通讯包
        /// </summary>
        /// <param name="reqtPkg">匹配的远程请求</param>
        /// <param name="statCodec">远程请求的执行结果反馈码</param>
        /// <param name="paramspkg">携带的参数属性包</param>
        /// <returns>应答通讯包</returns>
        public ReplyCommunicatePackage CreateReplyCommunicatePackage(RequestPackage reqtPkg, ReplyPackage.Middleware_ReplyInfo statCodec, ParamPackage paramspkg)
        {
            return base.CoCreateReplyCommunicatePackage(reqtPkg, statCodec, paramspkg);
        }

        /// <summary>
        /// 反馈一个应答通讯包, 非阻塞式
        /// </summary>
        /// <param name="communicator">应答通讯包</param>
        public void AsynFeedbackCommunicateReplyMessage(ReplyCommunicatePackage communicator)
        {
            base.CoAsynFeedbackCommunicateReplyMessage(communicator);
        }

        /// <summary>
        /// 反馈一个应答通讯包, 阻塞式
        /// </summary>
        /// <param name="communicator">应答通讯包</param>
        /// <param name="timeMillionsecond">等待的上限时长</param>
        //public void SynFeedbackCommunicateReplyMessage(ReplyCommunicatePackage communicator, int timeMillionsecond)
        //{
        //    try
        //    {
        //        base.CoSynFeedbackCommunicateReplyMessage(communicator, timeMillionsecond);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw ex;            	
        //    }
        //}

        /// <summary>
        /// 创建一个群组
        /// </summary>
        /// <param name="detail">群组的自然语言描述，（UTF-8编码）</param>
        /// <param name="communicateType">发起该请求的网络形式</param>
        /// <returns>立即返回一个GroupDevice（可能未就绪）</returns>
        public GroupDevice CreateGroup(string detail, CommunicatType communicateType)
        {
            if (CommunicatType.Synchronization == communicateType)
            {
                GroupDevice retGroupDevice = null;
                try
                {
                    retGroupDevice = base.CoSynCreateGroup(detail);
                }
                catch (Exception ex)
                {
                    throw (ex as CreateGroupExcetion);
                }
                return retGroupDevice;
            }
            else
            {
                return base.CoAsynCreateGroup(detail);
            }
        }

        /// <summary>
        /// 获取一个指定描述名称的群组
        /// </summary>
        /// <param name="detail">群组的自然语言描述，（UTF-8编码）</param>
        /// <param name="communicateType">发起该请求的网络类型</param>
        /// <returns>立即返回一个GroupDevice（可能未就绪）</returns>
        public GroupDevice GetGroup(string detail, CommunicatType communicateType)
        {
            foreach (GroupDevice item in _localGroupDeviceBuffer)
            {
                if (detail.Equals(item.Detail))
                {
                    return item;
                }
            }
            if (CommunicatType.Synchronization == communicateType)
            {
                try
                {
                    return base.CoSynGetGroup(detail);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                return base.CoAsynGetGroup(detail);
            }
        }

        /// <summary>
        /// 加入一个群组
        /// </summary>
        /// <param name="group">指定的群组</param>
        /// <param name="role">设备在群组中的角色</param>
        /// <param name="communicateType">通讯的网络形式</param>
        public void JoinGroup(GroupDevice group, GroupMemberRole role, CommunicatType communicateType)
        {
            if (CommunicatType.Synchronization == communicateType)
            {
                try
                {
                    base.CoSynJoinGroup(group, role);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                base.CoAsynJoinGroup(group, role);
            }
        }

        /// <summary>
        /// 退出一个群组
        /// </summary>
        /// <param name="group">指定的群组</param>
        /// <param name="communicateType">通讯的网络形式</param>
        public void ExitGroup(GroupDevice group, CommunicatType communicateType)
        {
            if (CommunicatType.Synchronization == communicateType)
            {
                try
                {
                    base.CoSynExitGroup(group);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                base.CoAsynExitGroup(group);
            }
        }

        /// <summary>
        /// 创建一个群组通讯包
        /// </summary>
        /// <param name="radioName">广播通讯的名称</param>
        /// <param name="radioParamPkg">携带的参数属性包</param>
        /// <param name="targetGroup">目的群组</param>
        /// <returns>返回创建完毕的通讯包</returns>
        public GroupComunicatePackage CreateRadioCommunicatePackage(string radioName,
                                                                                                    ParamPackage radioParamPkg,
                                                                                                    GroupDevice targetGroup)
        {
            return base.CoCreateGroupCommunicatePackage(radioName, radioParamPkg, targetGroup);
        }

        /// <summary>
        /// 发起一次广播通讯
        /// </summary>
        /// <param name="communicator">广播通讯包</param>
        public void Radio(GroupComunicatePackage communicator)
        {
            base.CoAsynRadio(communicator);
        }

        ///// <summary>
        ///// 广播操作触发的错误事件
        ///// </summary>
        //public event RadioErrorHandler RadioErrorRecived = null;

        /// <summary>
        /// 收到一个远程广播通知
        /// </summary>
        public event RemotRadioRecivedHandler RemotRadioRecived = null;

        /// <summary>
        /// 收到一个远程请求
        /// </summary>
        public event RemotReqtRecivedHandler RemotReqtRecived = null;

        /// <summary>
        /// 非阻塞远程通讯触发的错误事件
        /// </summary>
        public event AsynCommunicatErrorHandler AsynCommunicatErrorRecived = null;

        /// <summary>
        /// 在线状态
        /// </summary>
        public bool Online
        {
            get { return base.Online; }
        }

        /// <summary>
        /// 设备会话Token
        /// </summary>
        public string Token
        {
            get { return base.SelfDevice.Token; }
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Detail
        {
            get { return base.SelfDevice.Detail; }
        }

        /// <summary>
        /// 监听一种设备消息
        /// </summary>
        /// <param name="messenger">目标设备</param>
        /// <param name="t_msg">消息定义</param>
        public void Listen(ClientDevice messenger, AbstractMessageType evt) { throw new NotImplementedException(); }

        /// <summary>
        /// 注册本设备支持的消息
        /// </summary>
        /// <param name="t_msg">消息定义</param>
        public void RegistMessage(AbstractMessageType t_msg) { throw new NotImplementedException(); }

        /// <summary>
        /// 创建一条消息
        /// </summary>
        /// <param name="t_msg">消息定义</param>
        /// <returns></returns>
        public AbstractMessage CreateMessage(AbstractMessageType t_msg) { throw new NotImplementedException(); }

        /// <summary>
        /// 事件接收回调
        /// </summary>
        public event MessageRecivedHandler MessageRecived = null;

        #endregion

        private void __OutsideRemotReqtRecived(RequestPackage reqtPkg)
        {
            if (null != this.RemotReqtRecived)
            {
                this.RemotReqtRecived(reqtPkg);
            }
        }

        private void __AsynCommunicatErrorRecived(CommunicatePackage sender, ComunicateExcetion ex)
        {
            if (null != this.AsynCommunicatErrorRecived)
            {
                this.AsynCommunicatErrorRecived(sender, ex);
            }
        }

        private void __OutsideRadioMessageRecived(RadioPackage radioPkg)
        {
            if (null != this.RemotRadioRecived)
            {
                this.RemotRadioRecived(radioPkg);
            }
        }
    }
}
