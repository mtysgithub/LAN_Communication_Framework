using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Device;
using Middleware.Interface;

namespace Middleware.Communication.Package.Internal
{
    /// <summary>
    /// 中间件 - 中间件 内部请求传输类
    /// </summary>
    public class C2CRequestPackage : RequestPackage, ICCSerializeOperat<CCCommunicateClass.Seria_C2CRequestPackage>
    {
        public C2CRequestPackage() : base() { }
        public C2CRequestPackage(ClientDevice sourDevice, 
                                                        string oprName, 
                                                        bool waittingResp,
                                                        Dictionary<string, byte[]> _attrDefaultValues)
            : base(sourDevice, oprName, waittingResp,
                        _attrDefaultValues)
        {
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_C2CRequestPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_C2CRequestPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_RequestPackage);

            RequestPackage tmpReqtPkg = new RequestPackage();
            tmpReqtPkg.ParseSerializeData(obj.OutsideMessage);
            this.OutSideMessage = tmpReqtPkg;
        }
        public CCCommunicateClass.Seria_C2CRequestPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_C2CRequestPackage ret = new CCCommunicateClass.Seria_C2CRequestPackage(base.ExportSerializeData());
            ret.OutsideMessage = this.OutSideMessage.ExportSerializeData();
            return ret;
        }
        #endregion

        public RequestPackage OutSideMessage
        {
            set { _outsideMessage = value; }
            get { return _outsideMessage; }
        }
        protected RequestPackage _outsideMessage = RequestPackage.Empty;

        public static C2CRequestPackage Empty
        {
            get 
            {
                return new C2CRequestPackage();
            }
        }
    }
}
