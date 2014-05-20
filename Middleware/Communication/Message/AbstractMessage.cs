using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Communication.Package;

namespace Middleware.Communication.Message
{
    public abstract class AbstractMessage : ParamPackage
    {
        private AbstractMessageType mMessageType = null;

        public AbstractMessageType Type
        {
            get { return mMessageType; }
            internal set { mMessageType = value; }
        }

        public AbstractMessage(AbstractMessageType t_msg)
        {
            mMessageType = t_msg;
        }
    }
}
