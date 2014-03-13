using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.LayerProcessor;
using Middleware.Device;
using Middleware.Communication.CommunicationConfig;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;

namespace Middleware.Interface.ServerProcotolOperator.Request
{
    interface IMiddleware2ServerHandShakeRequest
    {
        void HandShake(bool isReLink, string detailOrToken);
    }
    interface IMiddleware2MiddlewareCommunicatRequest
    {
        void WilSendMiddleware2MiddlewareMessage(BaseDevice targetDevice, MiddlewareTransferPackage wilSendPkg);
        MiddlewareTransferPackage MTPkg
        {
            get;
        }
    }
    interface ICreateGroupRequest
    {
        void CreateNewGroup(string groupDetail);
    }
    interface IJoinGroupRequest
    {
        void JoinGroup(string gropToken, GroupMemberRole memberRole);
    }
    interface IExitGroupRequest
    {
        void ExitGroup(string gropToken);
    }
    interface IRadioMessageRequest
    {
        void RadioMessage(string gropToken, RadioPackage radioPkg);
    }
    interface IOnlineGroupListRequest
    {
        void OnlineGroupRequest();
    }
    interface IOnlineGroupClientRequest
    {
        void OnlineGroupClientRequest(string gropToken);
    }
    interface IClientCancleRequest
    {
        void ClientCancle();
    }
}
