using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.JsonSerializtion;
using Middleware;
using ProtoBuf.Meta;

namespace NetFrameworkSerializeToolGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            __CJNetFrameworkSerialzetionToolCompile();
        }

        private static void __CJNetFrameworkSerialzetionToolCompile()
        {
            RuntimeTypeModel rtCSProtocolSerializeTypSet = TypeModel.Create();
            List<Type> CSProtocolSerializeTypSet = JsonSerializtionFactory.CreateSpecializationSerializeTypeSet();
            foreach (Type typeItem in CSProtocolSerializeTypSet)
            {
                rtCSProtocolSerializeTypSet.Add(typeItem, true);
            }
            rtCSProtocolSerializeTypSet.Compile("CJNet_SerializeTool", "CJNet_SerializeTool.dll");

            //RuntimeTypeModel rtMiddlewareProtocolSerializeTypSet = TypeModel.Create();
            //List<Type> MiddlewareSerializeTypSet = Middleware.Middleware.JsonSerializtionFactory.CreateSpecializationSerializeTypeSet();
            //foreach (Type typeItem in MiddlewareSerializeTypSet)
            //{
            //    rtMiddlewareProtocolSerializeTypSet.Add(typeItem, true);
            //}
            //rtMiddlewareProtocolSerializeTypSet.Compile("CJNet_MiddlewareProtocolSerializeTool", "CJNet_MiddlewareProtocolSerializeTool.dll");
        }
    }
}
