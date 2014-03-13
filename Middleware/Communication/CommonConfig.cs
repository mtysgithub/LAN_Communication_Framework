using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.Device;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;
using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;

namespace Middleware.Communication.CommunicationConfig
{
    #region Event
    public class AsynResponseEventArg : EventArgs
    {
        public AsynResponseEventArg(ReplyPackage replyPackage)
        {
            _replyPackage = replyPackage;
        }
        public ReplyPackage RemotDeviceReplyPackage
        {
            get { return _replyPackage; }
        }
        ReplyPackage _replyPackage = null;
    }

    /// <summary>
    /// 非阻塞式的应答报文通知
    /// </summary>
    /// <param name="sender">该应答对应的发起通讯包</param>
    /// <param name="evtArg">事件包装类</param>
    public delegate void AsynReponseHandler(CommunicatePackage sender, AsynResponseEventArg evtArg);

    /// <summary>
    /// 收到一个远程请求通知
    /// </summary>
    /// <param name="reqtPkg">远程请求数据包</param>
    public delegate void RemotReqtRecivedHandler(RequestPackage reqtPkg);

    /// <summary>
    /// 收到一个远程广播通知
    /// </summary>
    /// <param name="radioPkg">远程广播数据包</param>
    public delegate void RemotRadioRecivedHandler(RadioPackage radioPkg);

    /// <summary>
    /// 非阻塞远程通讯触发的错误事件通知
    /// </summary>
    /// <param name="group">出现操作错误的通讯包</param>
    /// <param name="ex">通讯异常信息基类</param>
    public delegate void AsynCommunicatErrorHandler(CommunicatePackage sender, ComunicateExcetion ex);

    ///// <summary>
    ///// Radio操作触发的错误事件
    ///// </summary>
    ///// <param name="group">出现操作错误的通讯包</param>
    ///// <param name="ex">群组操作异常类</param>
    //public delegate void RadioErrorHandler(GroupComunicatePackage sender, RadioErrorExcetion ex);
    #endregion

    #region Type
    public enum RunningProtocolPlatform
    {
        Tcp,
    }
    public enum CommunicatType
    {
        Asynchronization,
        Synchronization
    }
    public enum GroupMemberRole
    {
        Listener,
        Speaker,
        Both
    }
    #endregion

    #region Excetion
    public class InitializationExtion : Exception
    {
        public InitializationExtion(string info)
            : base(info)
        {

        }
    }

    public class ComunicateExcetion : Exception
    {
        public ComunicateExcetion() : base() { }
        public ComunicateExcetion(string info)
            : base(info)
        {

        }
    }

    public class MiddlewareCommunicatErrorExcetion : ComunicateExcetion
    {
        public MiddlewareCommunicatErrorExcetion(string info)
            : base(info)
        {

        }
    }

    public class RadioErrorExcetion : ComunicateExcetion
    {
        public RadioErrorExcetion(string info)
            : base(info)
        {

        }
    }

    #endregion
}
