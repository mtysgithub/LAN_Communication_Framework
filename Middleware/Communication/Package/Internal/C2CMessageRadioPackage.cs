using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Device;
using Middleware.Communication.Message;
namespace Middleware.Communication.Package.Internal
{
    public class C2CMessageRadioPackage : C2CRadioPackage
    {
        protected AbstractMessage mMessage = null;
        public AbstractMessage Message
        {
            get { return mMessage; }
        }

        public C2CMessageRadioPackage(GroupDevice targetDevice, AbstractMessage msg)
            : base(targetDevice, "C2CMessageRadioPackage_" + msg.Type.Name, null)
        {
            mMessage = msg;
        }
    }
}
