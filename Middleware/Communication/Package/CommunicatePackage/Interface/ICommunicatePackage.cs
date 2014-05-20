using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.Device;
using Middleware.Communication;
using Middleware.Communication.Package;


namespace Middleware.Communication
{
    interface ICommunicatePackage
    {
        ParamPackage ParamPackage
        {
            get;
            set;
        }
        string CommunicationName
        {
            get;
            set;
        }
        CommunicationConfig.CommunicatType CommunicateType
        {
            get;
            set;
        }
        bool WaitResponse
        {
            get;
            set;
        }
        //BaseDevice SourDevice
        //{
        //    get;
        //    set;
        //}
        BaseDevice TargetDevice
        {
            get;
            set;
        }
        ReplyPackage ResponsePackage
        {
            get;
        }
        string ID
        {
            get;
        }
        //delegate CommunicationConfig.AsynchronousReponseHandler AsynchronousReponseCame;
    }
}
