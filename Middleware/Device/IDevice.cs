using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Device
{
    interface IDevice
    {
        string Token
        {
            get;
        }
        string Detail
        {
            get;
        }
    }
}
