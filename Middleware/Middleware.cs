using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProtoBuf.Meta;

using Middleware.Communication;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;
using Middleware.Device;
using Hik.Communication.Scs;
using Hik.Communication;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;

using ProtocolLibrary.CSProtocol;
using ProtocolLibrary.JsonSerializtion;

using Middleware.Interface.Ex;
using Middleware.Communication.EndPoint.Tcp;
using Middleware.LayerProcessor;

namespace Middleware
{
    internal enum MiddlewareErrorInfo
    {
        S_OK,
        S_FAILD,
        E_FAILD
    }
    public class Middleware
    {
        public static IExMiddlewareDevice CreateNewClient(MiddlewareTcpEndPoint endPoint, string detail, List<string> oprRules, List<string> opredRules)
        {
            MiddlewareDevice inst = new MiddlewareDevice();
            inst.Initialization(endPoint, detail, oprRules, opredRules);
            return inst as IExMiddlewareDevice;
        }

        public static IExMiddlewareDevice GetOldClient(MiddlewareTcpEndPoint endPoint, string detail, string token, List<string> oprRules, List<string> opredRules)
        {
            MiddlewareDevice inst = new MiddlewareDevice();
            inst.Initialization(endPoint, detail, token, oprRules, opredRules);
            return inst as IExMiddlewareDevice;
        }
    }
}
