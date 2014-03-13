using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

using ProtocolLibrary.CCProtocol;

using Middleware.Device;
using Middleware.Interface;

namespace Middleware.Communication.Package.Internal
{
    /// <summary>
    /// 中间件 - 中间件应答传输包装类
    /// </summary>
    public class C2CReplyPackage : ReplyPackage, ICCSerializeOperat<CCCommunicateClass.Seria_C2CReplyPackage>
    {
        public C2CReplyPackage() : base() { }
        public C2CReplyPackage(ReplyPackage.Middleware_ReplyInfo state,
                                        Dictionary<string, byte[]> _attrDefaultValues)
            : base(state, _attrDefaultValues)
        {
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_ReplyPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_C2CReplyPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_ReplyPackage);
            this.OutSideMessage.ParseSerializeData(obj.OutsideMessage);
        }
        public CCCommunicateClass.Seria_C2CReplyPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_C2CReplyPackage ret = new CCCommunicateClass.Seria_C2CReplyPackage(base.ExportSerializeData());
            ret.OutsideMessage = this.OutSideMessage.ExportSerializeData();
            return ret;
        }
        #endregion

        public ReplyPackage OutSideMessage
        {
            set { _outsideMessage = value; }
            get { return _outsideMessage; }
        }
        protected ReplyPackage _outsideMessage = ReplyPackage.Empty;

        public static C2CReplyPackage Empty
        {
            get
            {
                return new C2CReplyPackage();
            }
        }
    }
}
