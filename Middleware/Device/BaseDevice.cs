using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Interface;

namespace Middleware.Device
{
    public class BaseDevice : IDevice, ICCSerializeOperat<CCCommunicateClass.Seria_Device>
    {
        #region ICCSerializeOperat<CCCommunicateClass.Seria_Device>
        public void ParseSerializeData(CCCommunicateClass.Seria_Device obj)
        {
            this.Token = obj.Token;
            this.Detail = obj.Detail;
        }

        public CCCommunicateClass.Seria_Device ExportSerializeData()
        {
            CCCommunicateClass.Seria_Device ret = new CCCommunicateClass.Seria_Device();
            ret.Token = this.Token;
            ret.Detail = this.Detail;
            return ret;
        }
        #endregion

        public string Token
        {
            get { return _deviceToken; }
            set { _deviceToken = value; }
        }

        public string Detail
        {
            get { return _detail; }
            set { _detail = value; }
        }

        protected string _deviceToken = BaseDevice.MakeDeviceToken();
        protected string _detail = string.Empty;

        static string MakeDeviceToken()
        {
            return string.Empty;
        }
    }
}
