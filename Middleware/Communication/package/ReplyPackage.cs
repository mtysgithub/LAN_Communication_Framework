using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Communication.Package.Internal;
using Middleware.Interface;

namespace Middleware.Communication.Package
{
    /// <summary>
    /// 上层应答传输包装类，向中间件使用者暴露
    /// </summary>
    public class ReplyPackage : ParamPackage, ICCSerializeOperat<CCCommunicateClass.Seria_ReplyPackage>
    {
        public enum Middleware_ReplyInfo
        {
            S_OK,
            S_FAILD,
            E_FAILD
        }
        public ReplyPackage() : base() { }
        public ReplyPackage(Middleware_ReplyInfo state,
                                            Dictionary<string, byte[]> _attrDefaultValues)
            : base("ReplyParamPackage", _attrDefaultValues)
        {
            ReplyState = state;
        }

        public Middleware_ReplyInfo ReplyState
        {
            get { return _replyState; }
            set { _replyState = value; }
        }
        protected Middleware_ReplyInfo _replyState;

        #region ICCSerializeOperat<CCCommunicateClass.Seria_ReplyPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_ReplyPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_ParamPackage);
            switch (obj.ReplyState)
            {
                case (int)Middleware_ReplyInfo.S_OK:
                    {
                        this.ReplyState = Middleware_ReplyInfo.S_OK;
                        break;
                    }
                case (int)Middleware_ReplyInfo.S_FAILD:
                    {
                        this.ReplyState = Middleware_ReplyInfo.S_FAILD;
                        break;
                    }
                case (int)Middleware_ReplyInfo.E_FAILD:
                    {
                        this.ReplyState = Middleware_ReplyInfo.E_FAILD;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("CCCommunicateClass.Seria_ReplyPackage.ReplyState无效");
                    }
            }
        }
        public CCCommunicateClass.Seria_ReplyPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_ReplyPackage ret = new CCCommunicateClass.Seria_ReplyPackage(base.ExportSerializeData());
            ret.ReplyState = (int)this.ReplyState;
            return ret;
        }
        #endregion

        public static ReplyPackage Empty
        {
            get 
            {
                return new ReplyPackage();
            }
        }
    }
}
