using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.Interface.ServerProcotolOperator.Reply;
using Middleware.Device;

namespace Middleware.Interface.ServerProcotolOperator
{
    interface IServerReplyInfoOperator : IMiddleware2ServerHandShakeReply,
                                                        IMiddleware2MiddlewareCommunicatReply,   
                                                        ICreateGroupReply,
                                                        IJoinGroupReply,
                                                        IExitGroupRely,
                                                        IRadioReply,
                                                        IOnlineGroupReply,
                                                        IOnlineGroupClientReply,
                                                        IClientCancleReply,

                                                        IMiddlewareMessageIncomming,
                                                        IRadioMessageIncomming
    {
        IMiddleware2ServerHandShakeReply Active_MiddlewareHandShakeReply();
        IMiddleware2MiddlewareCommunicatReply Active_Middleware2MiddlewareCommunicatReply();
        ICreateGroupReply Active_CreateGroupReply();
        IJoinGroupReply Active_JoinGroupReply();
        IExitGroupRely Active_ExitGroupReply();
        IRadioReply Active_RadioGroupReply();
        IOnlineGroupReply Active_OnlineGroupReply();
        IOnlineGroupClientReply Active_OnlineGroupClientReply();
        IClientCancleReply Active_ClientCancleReply();

        IMiddlewareMessageIncomming Active_MiddlewareMessageIncomming();
        IRadioMessageIncomming Active_RadioMessageIncomming();
    }
}
