using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CCProtocol;
using Middleware.Interface;
using Middleware.Communication.Package;

namespace Middleware.Communication.Message
{
    public class BaseMessage : ParamPackage, ICCSerializeOperat<CCCommunicateClass.Seria_BaseMessage>
    {
        private BaseMessageType mMessageType = null;

        public BaseMessageType Type
        {
            get { return mMessageType; }
            internal set { mMessageType = value; }
        }

        public BaseMessage() : base() { }

        public BaseMessage(BaseMessageType t_msg)
        {
            mMessageType = t_msg;
        }

        #region #region <IParamPackage>
        protected enum SerializObjectType
        {
            BaseMessage,
        }
        public override byte[] SerializeMiddlewareMessage()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public new BaseMessage DeserializeMessage(byte[] bytes)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion

        #region ICCSerializeOperat<CCCommunicateClass.Seria_BaseMessage>
        public void ParseSerializeData(CCCommunicateClass.Seria_BaseMessage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_ParamPackage);
            this.mMessageType.ParseSerializeData(obj.MessageType);
        }

        public new CCCommunicateClass.Seria_BaseMessage ExportSerializeData()
        {
            CCCommunicateClass.Seria_BaseMessage serFormatPkg =
                new CCCommunicateClass.Seria_BaseMessage(base.ExportSerializeData());
            serFormatPkg.MessageType = this.mMessageType.ExportSerializeData();
            return serFormatPkg;
        }
        #endregion
    }
}
