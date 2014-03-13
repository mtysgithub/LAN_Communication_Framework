using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf.Meta;

using ProtocolLibrary.CSProtocol;
using System.IO;
using ProtocolLibrary.CCProtocol;

namespace ProtocolLibrary.JsonSerializtion
{
    public class JsonSerializtionFactory
    {
        /// <summary> 
        /// 转换List<T>的数据为JSON格式         
        /// </summary> 
        /// <typeparam name="T">类</typeparam>         
        /// <param name="vals">列表À值</param>         
        /// <returns>JSON格式数据</returns>         
        public static byte[] JSON<T>(List<T> list)
        {
            //return JsonConvert.SerializeObject(list);
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, list);
                byte[] buff = m.ToArray();
                return buff;
            }
        }

        /// <summary>
        /// 反序列化一个Json到T类型集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> DeJSON<T>(byte[] json)
        {
            //return JsonConvert.DeserializeObject<List<T>>(json);
            CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
            List<T> ret = null;
            using (MemoryStream m = new MemoryStream(json))
            {
                ret = deSerializeTool.Deserialize(m, null, typeof(List<T>)) as List<T>;
            }
            return ret;
        }

        /// <summary>
        /// 导出序列化类型集
        /// </summary>
        /// <returns></returns>
        public static List<Type> CreateSpecializationSerializeTypeSet()
        {
            List<Type> typList = new List<Type>();

            List<Type> listCSSerializeTypeSet = CSCommunicateClass.CreateSpecializationSerializeTypeSet();
            foreach (Type item in listCSSerializeTypeSet)
            {
                typList.Add(item);
            }

            List<Type> listCCSerializeTypeSet = CCCommunicateClass.CreateSpecializationSerializeTypeSet();
            foreach (Type item in listCCSerializeTypeSet)
            {
                typList.Add(item);
            }

            return typList;
        }
    }
}
