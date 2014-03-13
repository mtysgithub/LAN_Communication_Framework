using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

using ProtocolLibrary.CCProtocol;
using ProtocolLibrary.CSProtocol;
using Middleware.Interface;

namespace Middleware.Device
{
    public class ClientDevice : BaseDevice, ICCSerializeOperat<CCCommunicateClass.Seria_ClientDevice>
    {
        public ClientDevice() : base() { }

        public ClientDevice(string token, string detail)
            : base()
        {
            this.Token = token;
            this.Detail = detail;
        }

        internal static void Parse(CSCommunicateClass.ClientInfo clientInfo, ClientDevice cdInst)
        {
            cdInst.Token = clientInfo.Token;
            cdInst.Detail = clientInfo.Detail;
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_ClientDevice>
        public void ParseSerializeData(CCCommunicateClass.Seria_ClientDevice obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_Device);
        }
        public new CCCommunicateClass.Seria_ClientDevice ExportSerializeData()
        {
            CCCommunicateClass.Seria_ClientDevice ret = new CCCommunicateClass.Seria_ClientDevice(base.ExportSerializeData());
            return ret;
        }
        #endregion

        public static ClientDevice Empty
        {
            get 
            {
                return new ClientDevice();
            }
        }
    }
}
