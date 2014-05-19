using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;
using Middleware.Device;

namespace Middleware.Communication
{
    public class ReplyCommunicatePackage : CommunicatePackage
    {
        public new ClientDevice TargetDevice
        {
            get { return base.TargetDevice as ClientDevice; }
            set { base.TargetDevice = value; }
        }
        public ReplyPackage.Middleware_ReplyInfo RemotRet
        {
            get { return _remotRet; }
            internal set { _remotRet = value; }
        }
        public RequestPackage RemotReqtPkg
        {
            get { return _remotReqtPkg; }
            internal set { _remotReqtPkg = value; }
        }
        protected ReplyPackage.Middleware_ReplyInfo _remotRet;
        protected RequestPackage _remotReqtPkg = null;
    }
}
