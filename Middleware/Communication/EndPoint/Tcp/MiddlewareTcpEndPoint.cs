using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hik.Communication.Scs.Communication.EndPoints.Tcp;

namespace Middleware.Communication.EndPoint.Tcp
{
    public class MiddlewareTcpEndPoint : MiddlewareEndPoint
    {
        public MiddlewareTcpEndPoint(string ip, int port)
            : base()
        {
            _scsEndPoint = new ScsTcpEndPoint(ip, port);
        }
        internal new ScsTcpEndPoint TcpEndPoint
        {
            get { return _scsEndPoint as ScsTcpEndPoint; }
        }
    }
}
