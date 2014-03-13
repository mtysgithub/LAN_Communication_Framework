using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSProtocol
{
    /// <summary>
    /// C/S协议层对象序列化格式处理接口
    /// </summary>
    /// <typeparam name="TSerilizeFormat">导出到序列化格式T TSerilizeFormat</typeparam>
    interface ICSSerializeOperat <TSerilizeFormat>
    {
        void ParseSerializeData(TSerilizeFormat obj);
        TSerilizeFormat ExportSerializeData();
    }
}
