using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Communication.Package;

namespace Middleware.Communication.Message
{
    public abstract class AbstractMessageType
    {
        private string mName;
        private uint mId;

        public AbstractMessageType(string name, uint id)
        {
            mName = name;
            mId = id;
        }
    }

    public abstract class AbstractMessage : ParamPackage
    {
        private AbstractMessageType mMessageType = null;

        public AbstractMessageType Type
        {
            get { return mMessageType; }
            set { mMessageType = value; }
        }

        public AbstractMessage()
        {

        }
    }
}
