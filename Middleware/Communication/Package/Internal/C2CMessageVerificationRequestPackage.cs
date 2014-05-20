using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CCProtocol;

using Middleware.Communication.Message;
using Middleware.Device;
using Middleware.Interface;

namespace Middleware.Communication.Package.Internal
{
    class C2CMessageVerificationRequestPackage : C2CRequestPackage, ICCSerializeOperat<CCCommunicateClass.Seria_C2CMessageVerificationRequestPackage>
    {
        protected static string mVerfPkgPrefix = "ListenVerf_";
        protected BaseMessageType mTypeMessage;
        public BaseMessageType TypeMessage
        {
            get{return mTypeMessage;}
        }

        public C2CMessageVerificationRequestPackage() : base(){}
        public C2CMessageVerificationRequestPackage(ClientDevice selfDevice, BaseMessageType typMsg) 
            : base(selfDevice, mVerfPkgPrefix + "_" + selfDevice.Token + "_" + typMsg.Name, true, null)

        {
            this.mTypeMessage = typMsg;
        }

        /*
         * TODO.
         * 目前的序列化解决方案使得 C2CRequestPackage 无法实现再次派生, 暂时搁置
         */

        #region ICCSerializeOperat<CCCommunicateClass.Seria_C2CMessageVerificationRequestPackage>
        public new CCCommunicateClass.Seria_C2CMessageVerificationRequestPackage ExportSerializeData()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ParseSerializeData(CCCommunicateClass.Seria_C2CMessageVerificationRequestPackage obj)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
