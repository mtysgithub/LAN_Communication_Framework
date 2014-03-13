using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.Device;
using Middleware.Interface.ServerProcotolOperator.Request;

namespace Middleware.Interface.ServerProcotolOperator
{
    interface IServerRequsetInfoOperator : IMiddleware2ServerHandShakeRequest, 
                                                            IMiddleware2MiddlewareCommunicatRequest,
                                                            ICreateGroupRequest,
                                                            IJoinGroupRequest,
                                                            IExitGroupRequest,
                                                            IRadioMessageRequest,
                                                            IOnlineGroupListRequest,
                                                            IOnlineGroupClientRequest,
                                                            IClientCancleRequest
    {
        IMiddleware2ServerHandShakeRequest Active_Middleware2ServerHandShakeRequest();
        IMiddleware2MiddlewareCommunicatRequest Active_MiddlewareCommunicatRequest();
        ICreateGroupRequest Active_CreateGroupRequest();
        IJoinGroupRequest Active_JoinGroupRequest();
        IExitGroupRequest Active_ExitGroupRequest();
        IRadioMessageRequest Active_RadioMessageRequest();
        IOnlineGroupListRequest Active_OnlineGroupListRequest();
        IOnlineGroupClientRequest Active_OnlineGroupClientRequest();
        IClientCancleRequest Active_ClientCancleRequest();
    }
}
