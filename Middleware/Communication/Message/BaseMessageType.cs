using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CCProtocol;

using Middleware.Interface;

namespace Middleware.Communication.Message
{
    public class BaseMessageType : ICCSerializeOperat<CCCommunicateClass.Seria_BaseMessageType>
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

        protected enum SerializObjectType
        {
            BaseMessageType,
        }
        public virtual byte[] SerializeMiddlewareMessage()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public BaseMessageType DeserializeMessage(byte[] bytes)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_BaseMessageType>
        public void ParseSerializeData(CCCommunicateClass.Seria_BaseMessageType obj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public new CCCommunicateClass.Seria_BaseMessageType ExportSerializeData()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
