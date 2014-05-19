using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Device;

namespace Middleware.Communication
{
    public class GroupComunicatePackage : CommunicatePackage
    {
        public new GroupDevice TargetDevice
        {
            get { return base.TargetDevice as GroupDevice; }
            set { base.TargetDevice = value; }
        }
    }
}
