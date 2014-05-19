using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.Device;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;
using Middleware.Communication.Message;
using Middleware.Communication.Excetion;
using Middleware.Communication.Package.CommunicatePackage;

namespace Middleware.Communication
{
    /// <summary>
    /// 非阻塞式的应答报文通知
    /// </summary>
    /// <param name="sender">该应答对应的发起通讯包</param>
    /// <param name="evtArg">事件包装类</param>
    public delegate void AsynReponseHandler(CommunicatePackage sender, AsynReplyCommunicatePackage evtArg);

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    public delegate void MessageRecivedHandler(ClientDevice sender, AbstractMessage msg);

    ///// <summary>
    ///// Radio操作触发的错误事件
    ///// </summary>
    ///// <param name="group">出现操作错误的通讯包</param>
    ///// <param name="ex">群组操作异常类</param>
    //public delegate void RadioErrorHandler(GroupComunicatePackage sender, RadioErrorExcetion ex);
}
