using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Package.CommunicatePackage
{
    public class AsynReplyCommunicatePackage : ReplyCommunicatePackage
    {
        public AsynReplyCommunicatePackage(ReplyPackage replyPackage)
        {
            _replyPackage = replyPackage;
        }
        public ReplyPackage RemotDeviceReplyPackage
        {
            get { return _replyPackage; }
        }
        ReplyPackage _replyPackage = null;
    }
}
