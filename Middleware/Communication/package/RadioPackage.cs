using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Communication.Package;
using Middleware.Device;
using Middleware.Communication.Package.Internal;
using Middleware.Interface;

namespace Middleware.Communication.Package
{
    public class RadioPackage : ParamPackage, ICCSerializeOperat<CCCommunicateClass.Seria_RadioPackage>
    {
        public RadioPackage()
            : base()
        {
            this.RadioName = string.Empty;
            this.Group = GroupDevice.Empty;
        }

        public RadioPackage(GroupDevice targetGroup,
                                       string radioName,
                                       Dictionary<string, byte[]> _attrDefaultValues)
            : base("RadioPackage", _attrDefaultValues)
        {
            _targetGroup = targetGroup;
            _radioName = radioName;
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

            byte objTypeCodec = (byte)SerializObjectType.RadioPackage;

            byte[] bytWilSend = new byte[1 + bytRadioPkg.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytRadioPkg, 0, bytWilSend, 1, bytRadioPkg.Length);

            return bytWilSend;
        }
        #endregion

        #region ICCSerializeOperat<CommunicateClass.Seria_RadioPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_RadioPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_ParamPackage);
            this.Group.ParseSerializeData(obj.GroupInfo);
            this.RadioName = obj.RadioName;
        }

        public new CCCommunicateClass.Seria_RadioPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_RadioPackage serializeFormatPkg = new CCCommunicateClass.Seria_RadioPackage(base.ExportSerializeData());
            serializeFormatPkg.RadioName = this.RadioName;
            serializeFormatPkg.GroupInfo = this.Group.ExportSerializeData();
            return serializeFormatPkg;
        }
        #endregion

        public GroupDevice Group
        {
            get { return _targetGroup; }
            set { _targetGroup = value; }
        }
        private GroupDevice _targetGroup = null;

        public string RadioName
        {
            get { return _radioName; }
            set { _radioName = value; }
        }
        private string _radioName = null;

        public static RadioPackage Empty
        {
            get { return new RadioPackage(); }
        }
    }
}
