using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Message
{
    public class BaseMessageType
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

        public BaseMessageType(string name, uint id)
        {
            mName = name;
            mId = id;
        }
    }
}
