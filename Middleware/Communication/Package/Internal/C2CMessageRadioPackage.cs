using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CCProtocol;

using Middleware.Device;
using Middleware.Communication.Message;
using Middleware.Interface;

namespace Middleware.Communication.Package.Internal
{
    public class C2CMessageRadioPackage : C2CRadioPackage, ICCSerializeOperat<CCCommunicateClass.Seria_C2CMessageRadioPackage>
    {
        protected BaseMessage mMessage = null;
        public BaseMessage Message
        {
            get { return mMessage; }
        }
        public C2CMessageRadioPackage() : base() { }

        public C2CMessageRadioPackage(GroupDevice targetDevice, BaseMessage msg)
            : base(targetDevice, "C2CMessageRadioPackage_" + msg.Type.Name, null)
        {
            mMessage = msg;
        }

        public override byte[] SerializeMiddlewareMessage()
        {
            byte[] bytMessageRadioPkg = null;
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, this.ExportSerializeData());
                bytMessageRadioPkg = m.ToArray();
            }

            byte objTypeCodec = (byte)SerializObjectType.C2CMessageRadioPackage;

            byte[] bytWilSend = new byte[1 + bytMessageRadioPkg.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytMessageRadioPkg, 0, bytWilSend, 1, bytMessageRadioPkg.Length);

            return bytWilSend;
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_C2CMessageRadioPackage>
        public new CCCommunicateClass.Seria_C2CMessageRadioPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_C2CMessageRadioPackage serFormatPkg =
                new CCCommunicateClass.Seria_C2CMessageRadioPackage(base.ExportSerializeData());
            serFormatPkg.Message = this.mMessage.ExportSerializeData();
            return serFormatPkg;
        }

        public void ParseSerializeData(CCCommunicateClass.Seria_C2CMessageRadioPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_C2CRadioPackage);

            BaseMessage message = new BaseMessage();
            message.ParseSerializeData(obj.Message);
            this.mMessage = message;
        }

        public static C2CMessageRadioPackage Empty
        {
            get { return new C2CMessageRadioPackage(); }
        }
        #endregion
    }
}
