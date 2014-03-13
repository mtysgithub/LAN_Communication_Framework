using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CSProtocol;
using Middleware.LayerProcessor;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;

namespace Middleware.Interface.ServerProcotolOperator.Reply
{
    interface IMiddleware2ServerHandShakeReply
    {
        void HandShakeRet(out bool ret, out string erroDetailOrToken);
    }
    interface IMiddleware2MiddlewareCommunicatReply
    {
        void SendMiddlewareMsgOprRet(out bool ret, out string errorDetail);
    }
    interface ICreateGroupReply
    {
        void CreateGroupRet(out bool ret, out string erroDetailOrToken);
    }
    interface IJoinGroupReply
    {
        void JoinGroupRet(out bool ret, out string erroDetail);
    }
    interface IExitGroupRely
    {
        void ExitGroupRet(out bool ret, out string errorDetail);
    }
    interface IRadioReply
    {
        void RadioRet(out bool ret, out string errorDetail);
    }
    interface IOnlineGroupReply
    {
        void OnlineGroupRet(out bool ret, out string errorDetail, out List<CSCommunicateClass.GroupInfo> gropInfoList);
    }
    interface IOnlineGroupClientReply
    {
        void OnlineGroupClientRet(out bool ret, out string errorDetail, out List<CSCommunicateClass.ClientInfo> gropClientInfoList);
    }
    interface IClientCancleReply
    {
        void ClientCancleRet(out bool ret);
    }

    interface IMiddlewareMessageIncomming
    {
        MiddlewareTransferPackage MTPkg
        {
            get;
        }
    }
    interface IRadioMessageIncomming
    {
        C2CRadioPackage RadioPkg
        {
            get;
        }
    }
}
