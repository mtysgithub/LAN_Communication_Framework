using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Communication.Package
{
    interface IParamPackage
    {
        byte[] SerializeMiddlewareMessage();
        ParamPackage DeserializeMessage(byte[] bytes);

        Dictionary<string, byte []> ParamDefalutValues
        {
            get;
            set;
        }
        List<T> Values<T>(string key);
        T Value<T>(string key);
    }
}
