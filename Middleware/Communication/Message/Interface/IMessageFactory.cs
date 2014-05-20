using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Message.Interface
{
    interface IMessageFactory
    {
        void RegistMessage(AbstractMessageType typMsg, Type t_Msg);
        AbstractMessage CreateMessage(AbstractMessageType typMsg);
    }
}
