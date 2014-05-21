using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Interface;
using Middleware.Device;

namespace Middleware.Communication.Package.Internal
{
    public class C2CRadioPackage : RadioPackage, ICCSerializeOperat<CCCommunicateClass.Seria_C2CRadioPackage>
    {
        public C2CRadioPackage()
            : base()
        {
            this.OutsideMessage = RadioPackage.Empty;
        }
        public C2CRadioPackage(GroupDevice targetDevice,
                                            string radioName,
                                            Dictionary<string, byte[]> _attrDefaultValues)
            : base(targetDevice, radioName, _attrDefaultValues)
        {

        }

        #region SerializProcotol
        public override byte[] SerializeMiddlewareMessage()
        {
            byte[] bytRadioPkg = null;
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, this.ExportSerializeData());
                bytRadioPkg = m.ToArray();
            }

            byte objTypeCodec = (byte)SerializObjectType.C2CRadioPackage;

            byte[] bytWilSend = new byte[1 + bytRadioPkg.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytRadioPkg, 0, bytWilSend, 1, bytRadioPkg.Length);

            return bytWilSend;
        }
        #endregion

        #region ICCSerializeOperat<CCCommunicateClass.Seria_C2CRadioPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_C2CRadioPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_RadioPackage);
            this.OutsideMessage.ParseSerializeData(obj.OutsideMessage);
        }

        public new CCCommunicateClass.Seria_C2CRadioPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_C2CRadioPackage serializeFormatPkg = new CCCommunicateClass.Seria_C2CRadioPackage(base.ExportSerializeData());
            serializeFormatPkg.OutsideMessage = this.OutsideMessage.ExportSerializeData();
            return serializeFormatPkg;
        }
        #endregion

        public RadioPackage OutsideMessage
        {
            get { return _outsideMessage; }
            set { _outsideMessage = value; }
        }
        private RadioPackage _outsideMessage = null;

        public new static C2CRadioPackage Empty
        {
            get 
            { 
                return new C2CRadioPackage(); 
            }
        }
    }
}
