using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Message
{
    public abstract class AbstractMessageType
    {
        protected string mName;
        protected uint mId;

        public string Name
        {
            get { return mName; }
        }

        public uint Id
        {
            get { return mId; }
        }

        public AbstractMessageType(string name, uint id)
        {
            mName = name;
            mId = id;
        }
    }
}
