using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Device;
using Middleware.Interface;
using Middleware.Communication.Package.Internal;

namespace Middleware.Communication.Package
{
    /// <summary>
    /// 中间件层传输基类
    /// 也向外界暴露，作为Request数据包在目的设备中的映射
    /// </summary>
    public class RequestPackage : ParamPackage, ICCSerializeOperat<CCCommunicateClass.Seria_RequestPackage>
    {
        public RequestPackage() : base() { }
        public RequestPackage(ClientDevice sourDevice, 
                                                    string communicationName, 
                                                    bool waitResponse, 
                                                    Dictionary<string, byte[]> _attrDefaultValues)
            : base("RequestTransferPackage",
                        _attrDefaultValues)
        {
            _communicationName = communicationName;
            _sourDevice = sourDevice;
            _waitResponse = waitResponse;
        }
        
        public string OperatName
        {
            get { return _communicationName; }
            set { _communicationName = value; }
        }

        public ClientDevice SourDevice
        {
            get { return _sourDevice; }
            set { _sourDevice = value; }
        }

        public bool WaittingResponse
        {
            get { return _waitResponse; }
            set { _waitResponse = value; }
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_RequestPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_RequestPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_ParamPackage);
            this.OperatName = obj.OperatName;
            this.SourDevice.ParseSerializeData(obj.SourDeviceInfo);
            this.WaittingResponse = obj.WaittingResponse;
        }
        public new CCCommunicateClass.Seria_RequestPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_RequestPackage ret = new CCCommunicateClass.Seria_RequestPackage(base.ExportSerializeData());
            ret.OperatName = this.OperatName;
            ret.SourDeviceInfo = this.SourDevice.ExportSerializeData();
            ret.WaittingResponse = this.WaittingResponse;
            return ret;
        }
        #endregion

        protected string _communicationName = string.Empty;
        protected ClientDevice _sourDevice = ClientDevice.Empty;
        protected bool _waitResponse = false;

        private bool mbIsEmpty = false;

        public bool IsEmpty
        {
            get { return this.mbIsEmpty; }
        }

        public static RequestPackage Empty
        {
            get 
            {
                RequestPackage obj = new RequestPackage();
                obj.mbIsEmpty = true;
                return obj;
            }
        }
    }
}
