using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Device;

namespace Middleware.Communication
{
    public class RequestCommunicatePackage : CommunicatePackage
    {
        public new ClientDevice TargetDevice
        {
            get { return base.TargetDevice as ClientDevice; }
            set { base.TargetDevice = value; }
        }
    }
}
