using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.Communication;
using Middleware.Device;
using Middleware.Communication.Package;

namespace Middleware.Communication
{
    public abstract class CommunicatePackage : ICommunicatePackage
    {
        public CommunicatePackage() { }

        public ParamPackage ParamPackage
        {
            get { return _paramPackage; }
            set { _paramPackage = value; }
        }
        public string CommunicationName
        {
            get { return _communicationName; }
            set { _communicationName = value; }
        }
        public CommunicationConfig.CommunicatType CommunicateType
        {
            get { return _communicatType; }
            set { _communicatType = value; }
        }
        public bool WaitResponse
        {
            get { return _waitResponse; }
            set { _waitResponse = value; }
        }
        //public BaseDevice SourDevice
        //{
        //    get { return _sourDevice; }
        //    set { _sourDevice = value; }
        //}
        public BaseDevice TargetDevice
        {
            get { return _targetDevice; }
            set { _targetDevice = value; }
        }
        public ReplyPackage ResponsePackage
        {
            get { return _responsePackage; }
            internal set { _responsePackage = value; }
        }
        public string ID
        {
            get { return _msgid.ToString(); }
        }
        //应答报文 - 异步方式
        public AsynReponseHandler AsynchronousReponseCame = null;

        private ParamPackage _paramPackage = null;
        private string _communicationName = string.Empty;
        private CommunicationConfig.CommunicatType _communicatType = CommunicationConfig.CommunicatType.Asynchronization;
        private bool _waitResponse = false;
        //private BaseDevice _sourDevice = null;
        private BaseDevice _targetDevice = null;
        private ReplyPackage _responsePackage = null;

        private Guid _msgid = System.Guid.NewGuid(); 
    }
}
