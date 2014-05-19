using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;

using Middleware.Device;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;
using Middleware.Communication.Event;
using Middleware.Communication.Excetion;
using Middleware.Communication.Package.CommunicatePackage;

namespace Middleware.Communication.CommunicationConfig
{
    public enum RunningProtocolPlatform
    {
        Tcp,
    }
    public enum CommunicatType
    {
        Asynchronization,
        Synchronization
    }
    public enum GroupMemberRole
    {
        Listener,
        Speaker,
        Both
    }
}
