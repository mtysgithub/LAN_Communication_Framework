using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Communication.Package;

namespace Middleware.Communication.Message
{
    public class BaseMessage : ParamPackage
    {
        private BaseMessageType mMessageType = null;

        public BaseMessageType Type
        {
            get { return mMessageType; }
            internal set { mMessageType = value; }
        }

        public BaseMessage(BaseMessageType t_msg)
        {
            mMessageType = t_msg;
        }
    }
}
