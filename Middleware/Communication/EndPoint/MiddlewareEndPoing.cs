using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Communication.EndPoints;

namespace Middleware.Communication.EndPoint
{
    public class MiddlewareEndPoint
    {
        internal ScsEndPoint TcpEndPoint
        {
            get { return _scsEndPoint; }
        }
        protected ScsEndPoint _scsEndPoint = null;
    }
}
