using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSProtocol.CommonClass;

namespace Middleware.Device
{
    [Serializable]
    public class ClientDevice : BaseDevice
    {
        [Serializable]
        public class ClientDeviceSerializeTransferInfo
        {
            ClientDeviceSerializeTransferInfo(string token, string detail)
            {
                _token = token;
                _detail = detail;
            }

            public string Detail
            {
                get { return _detail; }
                set { _detail = value; }
            }
            private string _detail = null;

            public string Token
            {
                get { return _token; }
                set { _token = value; }
            }
            private string _token = null;
        }

        public ClientDevice() : base() { }
        public ClientDevice(string token, string detail)
            : base()
        {
            this.Token = token;
            this.Detail = detail;
        }
        internal static void Parse(CommonClass.ClientInfo clientInfo, ClientDevice cdInst)
        {
            cdInst.Token = clientInfo.Token;
            cdInst.Detail = clientInfo.Detail;
        }
    }
}
