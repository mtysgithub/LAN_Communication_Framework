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
        protected enum SerializObjectType
        {
            RadioPackage,
            C2CRadioPackage
        }
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
        /// <summary>
        /// 对二进制段做反序列化
        /// </summary>
        /// <param name="bytes">目标二进制字段</param>
        /// <returns>反序列化结果</returns>
        public RadioPackage DeserializeMessage(byte[] bytes)
        {
            if ((null == bytes) || (0 == bytes.Length))
            {
                throw new Exception("Bin数据不存在或为空");
            }
            byte bytOpjTypeCodec = bytes[0];
            switch (bytOpjTypeCodec)
            {
                case (byte)SerializObjectType.RadioPackage:
                    {
                        try
                        {
                            byte[] bytObjContent = new byte[bytes.Length - 1];
                            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                            CCCommunicateClass.Seria_RadioPackage serializeFormatPkg = null;
                            RadioPackage ret = RadioPackage.Empty;
                            {
                                using (MemoryStream m = new MemoryStream(bytObjContent))
                                {
                                    CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                                    serializeFormatPkg = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_RadioPackage))
                                                                    as CCCommunicateClass.Seria_RadioPackage;
                                }
                            }
                            ret.ParseSerializeData(serializeFormatPkg);
                            return ret;
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                        }
                    }
                case (byte)SerializObjectType.C2CRadioPackage:
                    {
                        try
                        {
                            byte[] bytObjContent = new byte[bytes.Length - 1];
                            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                            CCCommunicateClass.Seria_C2CRadioPackage serializeFormatPkg = null;
                            C2CRadioPackage ret = C2CRadioPackage.Empty;
                            using (MemoryStream m = new MemoryStream(bytObjContent))
                            {
                                CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                                serializeFormatPkg = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_C2CRadioPackage))
                                                                as CCCommunicateClass.Seria_C2CRadioPackage;
                            }
                            ret.ParseSerializeData(serializeFormatPkg);
                            return ret;
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                        }
                    }
                default:
                    {
                        throw new NotImplementedException("二进制数据指向无法识别的类型");
                    }
            }
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
