using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using ProtoBuf;

using ProtocolLibrary.CSProtocol;
using ProtocolLibrary.CCProtocol;

using Middleware.Interface;
using Middleware.Communication.Package.Internal;
using Middleware.Communication.Package;

namespace Middleware.Communication.Package
{
    public class ParamPackage : IParamPackage, ICCSerializeOperat<CCCommunicateClass.Seria_ParamPackage>
    {
        public ParamPackage() { }
        public ParamPackage(string _pkgName,
            Dictionary<string, byte[]> _attrDefaultValues)
        {
            _AttrDefalutValues = _attrDefaultValues;
        }

        public byte[] this[string name]
        {
            get
            {
                if (_AttrDefalutValues.ContainsKey(name))
                {
                    return _AttrDefalutValues[name];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _AttrDefalutValues.Add(name, value);
            }
        }

        #region <IParamPackage>
        protected enum SerializObjectType
        {
            ParamPackage,
            RadioPackage,
            C2CRadioPackage
        }
        /// <summary>
        /// 对主调实例做序列化
        /// </summary>
        /// <returns>序列化结果</returns>
        public virtual byte[] SerializeMiddlewareMessage()
        {
            byte[] bytRadioPkg = null;
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, this.ExportSerializeData());
                bytRadioPkg = m.ToArray();
            }

            byte objTypeCodec = (byte)SerializObjectType.ParamPackage;

            byte[] bytWilSend = new byte[1 + bytRadioPkg.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytRadioPkg, 0, bytWilSend, 1, bytRadioPkg.Length);

            return bytWilSend;
        }

        /// <summary>
        /// 对二进制段做反序列化
        /// </summary>
        /// <param name="bytes">目标二进制字段</param>
        /// <returns>反序列化结果</returns>
        public ParamPackage DeserializeMessage(byte[] bytes)
        {
            if ((null == bytes) || (0 == bytes.Length))
            {
                throw new Exception("Bin数据不存在或为空");
            }
            byte bytOpjTypeCodec = bytes[0];
            switch (bytOpjTypeCodec)
            {
                case (byte)SerializObjectType.ParamPackage:
                    {
                        try
                        {
                            byte[] bytObjContent = new byte[bytes.Length - 1];
                            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                            CCCommunicateClass.Seria_ParamPackage serializeFormatPkg = null;
                            RadioPackage ret = RadioPackage.Empty;
                            using (MemoryStream m = new MemoryStream(bytObjContent))
                            {
                                CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                                serializeFormatPkg = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_ParamPackage))
                                                                as CCCommunicateClass.Seria_ParamPackage;
                            }
                            ret.ParseSerializeData(serializeFormatPkg);
                            return ret;
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                        }
                    }
                //case (byte)SerializObjectType.RadioPackage:
                //    {
                //        try
                //        {
                //            byte[] bytObjContent = new byte[bytes.Length - 1];
                //            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                //            CCCommunicateClass.Seria_RadioPackage serializeFormatPkg = null;
                //            RadioPackage ret = RadioPackage.Empty;
                //            {
                //                using (MemoryStream m = new MemoryStream(bytObjContent))
                //                {
                //                    CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                //                    serializeFormatPkg = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_RadioPackage))
                //                                                    as CCCommunicateClass.Seria_RadioPackage;
                //                }
                //            }
                //            ret.ParseSerializeData(serializeFormatPkg);
                //            return ret;
                //        }
                //        catch (System.Exception ex)
                //        {
                //            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                //        }
                //    }
                //case (byte)SerializObjectType.C2CRadioPackage:
                //    {
                //        try
                //        {
                //            byte[] bytObjContent = new byte[bytes.Length - 1];
                //            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                //            CCCommunicateClass.Seria_C2CRadioPackage serializeFormatPkg = null;
                //            C2CRadioPackage ret = C2CRadioPackage.Empty;
                //            using (MemoryStream m = new MemoryStream(bytObjContent))
                //            {
                //                CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                //                serializeFormatPkg = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_C2CRadioPackage))
                //                                                as CCCommunicateClass.Seria_C2CRadioPackage;
                //            }
                //            ret.ParseSerializeData(serializeFormatPkg);
                //            return ret;
                //        }
                //        catch (System.Exception ex)
                //        {
                //            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                //        }
                //    }
                default:
                    {
                        throw new NotImplementedException("二进制数据指向无法识别的类型");
                    }
            }
        }

        /// <summary>
        /// 由于序列化情况较为复杂，T模板参数应尽量为基本数据类型或其简单复合，否则特化T可能引发预料之外的情况
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="key">字典键</param>
        /// <returns>值组封装到List</returns>
        public List<T> Values<T>(string key)
        {
            throw new NotImplementedException();
            //if (ParamDefalutValues.ContainsKey(key))
            //{
            //    List<T> _retList = new List<T>();
            //    if (null != (ParamDefalutValues[key] as JContainer))
            //    {
            //        JContainer jContainer = ParamDefalutValues[key] as JContainer;
            //        System.Collections.Generic.IEnumerator<T> it = jContainer.Values<T>().GetEnumerator();
            //        while (false != it.MoveNext())
            //        {
            //            _retList.Add(it.Current);
            //        }
            //    }
            //    else
            //    {
            //        throw new Exception("尝试反序列数据错误：不是一个列表类型数据");
            //    }
            //    return _retList;
            //}
            //throw new Exception("value is not found");
        }

        /// <summary>
        /// 特化键值到T类型，请操作简单类型
        /// </summary>
        /// <typeparam name="T">特化的目标类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>键值特化到T类型实例</returns>
        public T Value<T>(string key)
        {
            throw new NotImplementedException();
            //if (_AttrDefalutValues.ContainsKey(key))
            //{
            //    return (T)_AttrDefalutValues[key];
            //}
            //throw new Exception("value is not found");
        }

        public Dictionary<string, byte []> ParamDefalutValues
        {
            get { return _AttrDefalutValues; }
            set { _AttrDefalutValues = value; }
        }
        #endregion

        #region ICCSerializeOperat<CommunicateClass.Seria_ParamPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_ParamPackage obj)
        {
            this.ParamDefalutValues = obj.ParamDefalutValues;
        }

        public CCCommunicateClass.Seria_ParamPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_ParamPackage ret = new CCCommunicateClass.Seria_ParamPackage();
            ret.ParamDefalutValues = this._AttrDefalutValues;
            return ret;
        }
        #endregion

        #region 数据段
        private Dictionary<string, byte []> _AttrDefalutValues = null;
        #endregion
    }
}