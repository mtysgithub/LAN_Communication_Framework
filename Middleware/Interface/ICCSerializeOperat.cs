using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Interface
{
    /// <summary>
    /// Middlewre对象序列化格式处理接口
    /// </summary>
    /// <typeparam name="TSerilizeFormat">导出到序列化格式 TSerilizeFormat</typeparam>
    interface ICCSerializeOperat<TSerilizeFormat>
    {
        /// <summary>
        /// 分析到一个指定的序列化格式
        /// </summary>
        /// <param name="obj"></param>
        void ParseSerializeData(TSerilizeFormat obj);

        /// <summary>
        /// 导出到一个指定的序列化格式
        /// </summary>
        /// <returns></returns>
        TSerilizeFormat ExportSerializeData();
    }
}
