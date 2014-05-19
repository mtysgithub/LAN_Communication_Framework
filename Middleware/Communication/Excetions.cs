using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Excetion
{
    #region Excetion
    public class InitializationExtion : Exception
    {
        public InitializationExtion(string info)
            : base(info)
        {

        }
    }

    public class ComunicateExcetion : Exception
    {
        public ComunicateExcetion() : base() { }
        public ComunicateExcetion(string info)
            : base(info)
        {

        }
    }

    public class MiddlewareCommunicatErrorExcetion : ComunicateExcetion
    {
        public MiddlewareCommunicatErrorExcetion(string info)
            : base(info)
        {

        }
    }

    public class RadioErrorExcetion : ComunicateExcetion
    {
        public RadioErrorExcetion(string info)
            : base(info)
        {

        }
    }

    #endregion
}
