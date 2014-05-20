using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Message.Interface
{
    interface IMessageFactory
    {
        void RegistMessage(BaseMessageType typMsg, Type t_Msg);
        BaseMessage CreateMessage(BaseMessageType typMsg);
    }
}
